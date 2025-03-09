using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
/// <summary>
/// WORKS FINE
/// </summary>
public static class IniFileModifier
{
    public static void ModifyIniFile(string filePath, string groupName, int slotId)
    {
        // Define the condition with the groupName
        string managedSlotCondition = ConstantVar.MANAGED_SLOT_CONDITION.Replace(ConstantVar.MANAGED_SLOT_CONDITION_TOBE_REPLACED, groupName);

        // Read all lines from the file
        var lines = File.ReadAllLines(filePath);
        var modifiedLines = new List<string>();
        string currentSection = null;
        modifiedLines.Add(ConstantVar.MANAGED_CONSTANTS_SECTION.Replace(ConstantVar.MANAGED_CONSTANTS_SECTION_TOBE_REPLACED, slotId.ToString()));

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (Regex.IsMatch(trimmedLine, @"^;.*$"))
            {
                modifiedLines.Add(line);
                continue; // Skip this line (comment) // Add the line as-is
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
                    modifiedLines.Add($"if {managedSlotCondition}");
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

    #region [Key] Section
    private static readonly string[] KeySectionsPrefixes = new[]
    {
        "Key"
    };

    public static void ModifyIniFileKey(string filePath, string groupName)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File does not exist.");
            return;
        }

        StringBuilder newContent = new StringBuilder();
        bool insideKeySection = false;
        bool conditionExists = false;
        string conditionToAdd = ConstantVar.MANAGED_SLOT_CONDITION.Replace(ConstantVar.MANAGED_SLOT_CONDITION_TOBE_REPLACED, groupName);

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