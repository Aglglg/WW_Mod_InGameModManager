using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ModManagerUtils
{
    [DllImport("dds_converter_from_mod_manager", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool convert_to_dds(string input_path, string output_path);
    
    public static void SaveManagedModData()
    {
        string pathJson = Path.Combine(
            PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName),
            ConstantVar.Managed_Path,
            ConstantVar.ModData_Json_File
        );
        string json = JsonUtility.ToJson(TabModManager.modData, true);
        File.WriteAllText(pathJson, json);
    }

    public static void ManageMod(string folder, int groupIndex, int modIndex)
    {
        string[] iniFiles = FindIniFiles.FindIniFilesRecursive(folder);
        string groupName = $"group_{groupIndex}";
        foreach (string iniFile in iniFiles)
        {
            string backupFile = Path.ChangeExtension(iniFile, ConstantVar.Managed_Backup_Extension);
            File.Copy(iniFile, backupFile);

            ModifyIniFile(iniFile, groupName, modIndex);
            ModifyIniFileKey(iniFile, groupName);
        }
    }

    public static bool RevertManagedMod(string folder)
    {
        string[] filesBackup = FindIniFiles.FindIniFilesManagedBackupRecursive(folder);
        foreach (string backupFile in filesBackup)
        {
            string originalFile = Path.ChangeExtension(backupFile, ".ini");
            if (File.Exists(backupFile))
            {
                File.Copy(backupFile, originalFile, true);
                File.Delete(backupFile);
            }
        }
        return filesBackup.Length > 0;
    }

    public static void CreateIcon(string inputPath, string outputPath, bool isGroupIcon)
    {
        Texture2D originalImage = LoadImage(inputPath);
        
        int targetWidth = isGroupIcon ? ConstantVar.WidthHeight_GroupIcon : ConstantVar.Width_ModIcon;
        int targetHeight = isGroupIcon ? ConstantVar.WidthHeight_GroupIcon : ConstantVar.Height_ModIcon;
        float ratio = (float)targetWidth/targetHeight;

        Texture2D modifiedImage = CropAndResize(originalImage, ratio, targetWidth, targetHeight);
        SaveResizedImage(modifiedImage, outputPath);

        string inputPathDds = outputPath;
        string outputPathDds = Path.ChangeExtension(outputPath, ".dds");

        bool success = convert_to_dds(inputPathDds, outputPathDds);

        if (success)
        {
            Debug.Log("DDS conversion successful!");
        }
        else
        {
            Debug.LogError("DDS conversion failed!");
        }
    }


    #region ImageProcessing
    private static Texture2D CropAndResize(Texture2D originalTexture, float targetAspectRatio, int targetWidth, int targetHeight)
    {
        // Step 1: Crop to aspect ratio
        Texture2D croppedTexture = CropToAspectRatio(originalTexture, targetAspectRatio);

        // Step 2: Resize to target resolution
        return ResizeTexture(croppedTexture, targetWidth, targetHeight);
    }

    private static Texture2D CropToAspectRatio(Texture2D texture, float aspectRatio)
    {
        int originalWidth = texture.width;
        int originalHeight = texture.height;

        float currentAspectRatio = (float)originalWidth / originalHeight;

        int cropWidth = originalWidth;
        int cropHeight = originalHeight;

        // Crop to the target aspect ratio
        if (currentAspectRatio > aspectRatio)
        {
            // Crop horizontally
            cropWidth = Mathf.RoundToInt(originalHeight * aspectRatio);
        }
        else
        {
            // Crop vertically
            cropHeight = Mathf.RoundToInt(originalWidth / aspectRatio);
        }

        int startX = (originalWidth - cropWidth) / 2;
        int startY = (originalHeight - cropHeight) / 2;

        Color[] pixels = texture.GetPixels(startX, startY, cropWidth, cropHeight);
        Texture2D croppedTexture = new Texture2D(cropWidth, cropHeight);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

    private static Texture2D ResizeTexture(Texture2D texture, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture.active = rt;

        Graphics.Blit(texture, rt);

        Texture2D resizedTexture = new Texture2D(width, height);
        resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resizedTexture.Apply();

        RenderTexture.active = null;
        rt.Release();

        return resizedTexture;
    }

    private static Texture2D LoadImage(string filePath)
    {
        byte[] imageData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageData);
        return texture;
    }

    private static void SaveResizedImage(Texture2D texture, string outputPath)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
    }
    #endregion


    private const string managedSlotCondition = "$managed_slot_id == $\\modmanageragl\\{groupName}\\active_slot";
    #region Modify Ini File
    private static void ModifyIniFile(string filePath, string groupName, int slotId)
    {
        // Define the condition with the groupName
        string modifiedManagedSlotCondition = managedSlotCondition.Replace("{groupName}", groupName);

        // Read all lines from the file
        var lines = File.ReadAllLines(filePath);
        var modifiedLines = new List<string>();
        string currentSection = null;
        modifiedLines.Add("[Constants]");
        modifiedLines.Add($"global $managed_slot_id = {slotId}\n");

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (Regex.IsMatch(trimmedLine, @"^;.*$"))
            {
                modifiedLines.Add(line);
                continue; // Skip this line (comment)
            }

            // Check if the line is a section header
            var sectionMatch = Regex.Match(trimmedLine, @"^\[(.*)\]$");
            if (sectionMatch.Success)
            {
                currentSection = sectionMatch.Groups[1].Value;

                // Add the section header to the modified lines
                modifiedLines.Add(line);

                // Add the condition or if statement based on the section type
                if (!ShouldExcludeSection(currentSection))
                {
                    modifiedLines.Add($"if {modifiedManagedSlotCondition}");
                }
            }
            else if (!string.IsNullOrEmpty(trimmedLine) && !ShouldExcludeSection(currentSection))
            {
                modifiedLines.Add(line);
            }
            else
            {
                // Add the line as-is (empty lines or excluded sections)
                modifiedLines.Add(line);
            }
        }

        // Add endif for non-excluded sections
        for (int i = 0; i < modifiedLines.Count; i++)
        {
            var line = modifiedLines[i];
            var sectionMatch = Regex.Match(line, @"^\[(.*)\]$");
            if (sectionMatch.Success)
            {
                var section = sectionMatch.Groups[1].Value;
                if (!ShouldExcludeSection(section) && !section.StartsWith("Key", StringComparison.OrdinalIgnoreCase))
                {
                    // Find the next section or end of file to insert endif
                    int j = i + 1;
                    while (j < modifiedLines.Count && !Regex.IsMatch(modifiedLines[j], @"^\[.*\]$"))
                    {
                        j++;
                    }
                    modifiedLines.Insert(j, "endif\n\n\n");
                }
            }
        }

        // Write the modified lines back to the file
        File.WriteAllLines(filePath, modifiedLines);
    }

    private static bool ShouldExcludeSection(string section)
    {
        // Exclude sections starting with Constants, Resource, or Key
        return section.StartsWith("Constants", StringComparison.OrdinalIgnoreCase) ||
               section.StartsWith("Resource", StringComparison.OrdinalIgnoreCase) ||
               section.StartsWith("Key", StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region Modify Ini File Key Section
    private static readonly string[] KeySectionsPrefixes = new[]
    {
        "Key"
    };

    private static void ModifyIniFileKey(string filePath, string groupName)
    {
        StringBuilder newContent = new StringBuilder();
        bool insideKeySection = false;
        bool conditionExists = false;
        string conditionToAdd = managedSlotCondition.Replace("{groupName}", groupName);

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmedLine = line.Trim();

            if (Regex.IsMatch(trimmedLine, @"^;.*$"))
            {
                newContent.AppendLine(line);
                continue; // Skip this line (comment)
            }

            // Check if the line is a section header
            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                // If we were inside a Key section and no condition was added, add it now
                if (insideKeySection && !conditionExists)
                {
                    newContent.AppendLine($"condition = {conditionToAdd}\n\n\n");
                }

                insideKeySection = false;
                conditionExists = false;

                string sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2);

                // Check if the section is a Key section
                if (IsKeySection(sectionName))
                {
                    insideKeySection = true;
                }

                newContent.AppendLine(line);
            }
            else if (insideKeySection)
            {
                // Check if the line is a condition line
                if (trimmedLine.StartsWith("condition", StringComparison.OrdinalIgnoreCase))
                {
                    // Append the new condition to the existing one
                    newContent.AppendLine($"{line} && {conditionToAdd}");
                    conditionExists = true;
                }
                else
                {
                    newContent.AppendLine(line);
                }
            }
            else
            {
                newContent.AppendLine(line);
            }
        }

        // If the file ends while inside a Key section and no condition was added, add it now
        if (insideKeySection && !conditionExists)
        {
            newContent.AppendLine($"condition = {conditionToAdd}");
        }

        // Write the modified content back to the file
        File.WriteAllText(filePath, newContent.ToString());
    }

    private static bool IsKeySection(string sectionName)
    {
        foreach (string prefix in KeySectionsPrefixes)
        {
            if (sectionName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}
