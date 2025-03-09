using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public static class ModsManager
{
    public static ManagedModDatas managedModDatas;

    public static void InitializeMods()
    {
        string managedModPath = Path.Combine(PlayerPrefs.GetString(ConstantVar.SUFFIX_PLAYERPREFKEY_MODPATH + Initialization.gameName),
                                            ConstantVar.MANAGED_PATH); //e.g: (D:\WWMI\Mods, Managed)

        if(Directory.Exists(managedModPath))
        {
            string pathModData = Path.Combine(managedModPath, ConstantVar.PATH_MODJSONDATA); //e.g: (D:\WWMI\Mods\Managed, modData.json)
            if(File.Exists(pathModData))
            {
                string jsonData = File.ReadAllText(pathModData);
                managedModDatas = JsonUtility.FromJson<ManagedModDatas>(jsonData);
                foreach (ManagedGroupData item in managedModDatas.groupDatas)
                {
                    Debug.Log(item.groupPath);
                    foreach (string i in item.modsPath)
                    {
                        Debug.Log(i);
                    }
                }
            }
            else
            {
                CreateDefaultModData(managedModPath, false);
            }
        }
        else
        {
            CreateDefaultModData(managedModPath, true);
        }
    }

    private static void CreateDefaultModData(string managedModPath, bool createManagedDirectory)
    {
        if(createManagedDirectory)
        {
            Directory.CreateDirectory(managedModPath);
        }

        string defaultGroupPath = Path.Combine(managedModPath, "DefaultGroup");
        Directory.CreateDirectory(defaultGroupPath);

        managedModDatas = new ManagedModDatas
        {
            groupDatas = new List<ManagedGroupData>
            {
                new ManagedGroupData
                {
                    groupPath = defaultGroupPath,
                    modsPath = new List<string>()
                }
            }
        };

        string json = JsonUtility.ToJson(managedModDatas, true);
        File.WriteAllText(Path.Combine(managedModPath, ConstantVar.PATH_MODJSONDATA), json);
    }
}
