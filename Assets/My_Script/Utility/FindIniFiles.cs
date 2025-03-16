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

    public static string[] FindIniFilesFixBackupRecursive(string mainFolder)
    {
        string[] iniFilesFixBackup = Directory.GetFiles(mainFolder, $"*.{ConstantVar.Fix_Backup_Extension}", SearchOption.AllDirectories);
        return iniFilesFixBackup;
    }

    public static string[] FindIniFilesManagedBackupRecursive(string mainFolder)
    {
        string[] iniFilesManagedBackup = Directory.GetFiles(mainFolder, $"*.{ConstantVar.Managed_Backup_Extension}", SearchOption.AllDirectories);
        return iniFilesManagedBackup;
    }
}
