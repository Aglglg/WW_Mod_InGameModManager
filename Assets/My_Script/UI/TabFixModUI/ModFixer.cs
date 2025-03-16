using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class ModFixer
{
    #region LOG TO UI
    private static SynchronizationContext unityContext;
    public static void Initialize()
    {
        unityContext = SynchronizationContext.Current;
    }
    private static void LogOnMainThread(string message, bool isError)
    {
        if(isError)
        {
            unityContext?.Post(_ => Debug.LogError(message), null);
        }
        else
        {
            unityContext?.Post(_ => Debug.Log(message), null);
        }
    }
    #endregion

    #region HASH REPLACEMENT
    public static async Task FixModHashReplacementAsync(ModFixData modFixData, string folder)
    {
        string[] iniFiles = FindIniFiles.FindIniFilesRecursive(folder);
        List<Task> tasks = new List<Task>();

        foreach (string filePath in iniFiles)
        {
            tasks.Add(ProcessIniFileHashReplacementAsync(filePath, modFixData.hashpair));
        }

        await Task.WhenAll(tasks);
    }

    private static async Task ProcessIniFileHashReplacementAsync(string filePath, Dictionary<string, string> hashpair)
    {
        string[] fileLines = await File.ReadAllLinesAsync(filePath);
        bool modified = false;

        for (int i = 0; i < fileLines.Length; i++)
        {
            string trimmedLine = fileLines[i].Trim();

            if (trimmedLine.StartsWith(';'))
            {
                continue; // Ignore commented lines
            }

            int pos = trimmedLine.IndexOf("hash");
            if (pos != -1)
            {
                string[] parts = trimmedLine.Substring(pos).Split('=').Select(s => s.Trim()).ToArray();
                if (parts.Length == 2)
                {
                    string hashValue = parts[1]; // Get the actual hash value
                    if (hashpair.TryGetValue(hashValue, out string replacement))
                    {
                        string modifiedLine = $"hash = {replacement}";
                        fileLines[i] = modifiedLine;
                        modified = true;
                    }
                }
            }
        }

        // Backup & rewrite asynchronously
        if (modified)
        {
            try
            {
                await BackupFileAsync(filePath);
                await File.WriteAllLinesAsync(filePath, fileLines);
                LogOnMainThread($"UI--MODIFIED {filePath} -- Should be fixed!\n", false);
            }
            catch
            {
                LogOnMainThread($"UI--FAILED {filePath} -- An error occurred", true);
            }
        }
        else
        {
            LogOnMainThread($"UI--SKIPPED {filePath} -- Nothing to be modified.", false);
        }
    }
    #endregion
    #region HASH ADDITION
    public static async Task FixModHashAdditionAsync(ModFixData modFixData, string folder)
    {
        string[] iniFiles = await Task.Run(() => FindIniFiles.FindIniFilesRecursive(folder));
        
        List<Task> tasks = new List<Task>();
        
        foreach (string filePath in iniFiles)
        {
            tasks.Add(Task.Run(async () =>
            {
                List<string> results = await Task.Run(() => ProcessIniFileHashAddition(filePath, modFixData.hashpair));
                
                if (!results.Any())
                {
                    LogOnMainThread($"UI--SKIPPED {filePath} -- Nothing to be added.", false);
                }
                else
                {
                    bool success = await Task.Run(() => SuccessAppendToIniFile(filePath, results));
                    if (success)
                    {
                        LogOnMainThread($"UI--MODIFIED {filePath} -- Should be fixed!\n", false);
                    }
                    else
                    {
                        LogOnMainThread($"UI--FAILED {filePath} -- An error occurred", true);
                    }
                }
            }));
        }
        
        await Task.WhenAll(tasks);
    }
    private static List<string> ProcessIniFileHashAddition(string filePath, Dictionary<string, string> hashpair)
    {
        List<string> sections = new List<string>();
        List<string> currentSection = new List<string>();
        bool inTextureSection = false;
        string sectionHeader = "";
        bool hasModifiedHash = false;
        HashSet<string> hashSet = new HashSet<string>();

        string[] fileLines = File.ReadAllLines(filePath);

        //First step, collect existing hashes
        foreach (string line in fileLines)
        {
            string trimmedLine = line.Trim();

            if(trimmedLine.StartsWith(';'))
            {
                continue; // Ignore commented lines
            }

            int pos = trimmedLine.IndexOf("hash");
            if (pos != -1)
            {
                //Trim to remove any spaces
                string[] parts = trimmedLine.Substring(pos).Split('=').Select(s => s.Trim()).ToArray();
                if (parts.Length == 2)
                {
                    hashSet.Add(parts[1]);
                }
            }
        }

        //Second step, modify the ini file
        foreach (string line in fileLines)
        {
            string trimmedLine = line.Trim();

            if(trimmedLine.StartsWith(';'))
            {
                continue; // Ignore commented lines
            }

            if(trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
            {
                // Store the previous section **ONLY IF IT HAD A MODIFIED HASH**
                if(inTextureSection && hasModifiedHash && currentSection.Any())
                {
                    //Join, give \n between each string in List<string> currentSection
                    sections.Add(string.Join("\n", currentSection));
                }

                // Reset for new section
                inTextureSection = trimmedLine.StartsWith("[Texture");
                hasModifiedHash = false;
                currentSection.Clear();
                sectionHeader = trimmedLine;

                if (inTextureSection)
                {
                    // Rename section by appending "_LOWQ"
                    sectionHeader = $"{sectionHeader.TrimEnd(']')}_FIXED_ADDITIONAL_HASH";
                }
                currentSection.Add(sectionHeader + "]");
            }
            else if(inTextureSection)
            {
                int pos = trimmedLine.IndexOf("hash");
                if (pos != -1)
                {
                    string[] parts = trimmedLine.Substring(pos).Split('=').Select(s => s.Trim()).ToArray();
                    if (parts.Length == 2)
                    {
                        string hashValue = parts[1]; // Get the actual hash value
                        if (hashpair.TryGetValue(hashValue, out string replacement))
                        {
                            // **Only modify if the replacement hash is NOT already in the file**
                            if (!hashSet.Contains(replacement))
                            {
                                string modifiedLine = $"hash = {replacement}";
                                currentSection.Add(modifiedLine);
                                hasModifiedHash = true; // Mark that this section should be added
                                continue;
                            }
                        }
                    }
                }
                //add the remaining lines on the section
                currentSection.Add(line);
            }
        }
        // Store the last section **ONLY IF IT HAD A MODIFIED HASH**
        if(inTextureSection && hasModifiedHash && currentSection.Any())
        {
            sections.Add(string.Join("\n", currentSection));
        }
        return sections;
    }
    private static async Task<bool> SuccessAppendToIniFile(string filePath, List<string> sections)
    {
        try
        {
            await BackupFileAsync(filePath);
            using (var file = new StreamWriter(filePath, true))
            {
                file.WriteLine();

                foreach (var section in sections)
                {
                    file.WriteLine(section);
                    file.WriteLine();
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    #region BACKUP
    private static async Task BackupFileAsync(string filePath)
    {
        await Task.Run(() => BackupFile(filePath));
    }
    private static void BackupFile(string filePath)
    {
        string stem = Path.GetFileNameWithoutExtension(filePath);
        if (stem != null)
        {
            string backupPath = Path.ChangeExtension(filePath, ConstantVar.Fix_Backup_Extension);

            if (!File.Exists(backupPath))
            {
                File.Copy(filePath, backupPath);
                LogOnMainThread($"UI--Backup created: {backupPath}", false);
            }
            else
            {
                LogOnMainThread($"UI--Backup already exists: {backupPath}", false);
            }
        }
    }
    #endregion
}
