using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class FindIniFiles
{
    public static string[] FindIniFilesRecursive(string mainFolder)
    {
        string[] iniFiles = Directory.GetFiles(mainFolder, "*.ini", SearchOption.AllDirectories)
                            .Where(file => !file.EndsWith("desktop.ini", StringComparison.OrdinalIgnoreCase))
                            .ToArray();
        return iniFiles;
    }
}
