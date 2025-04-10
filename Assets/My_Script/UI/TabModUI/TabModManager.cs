using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TabModManager : MonoBehaviour
{
    public static ModData modData;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private GroupScrollHandler groupScrollHandler;
    [SerializeField] private ModScrollHandler modScrollHandler;
    [SerializeField] private GameObject textInfo;
    [SerializeField] private GameObject reloadInfo;
    [SerializeField] private GameObject reloadManualInfo;
    [SerializeField] private TextAsset templateBackgroundKeypress;
    [SerializeField] private GameObject modSelectionObject;

    private string _currentModPath;
    private bool _modDataWasLoaded = false;

    // Called from a button click
    public void GoToSetting()
    {
        uiManager.ChangeTab((int)TabState.Setting);
    }

    // Called from PlayerInput, if f10 presssed
    public void HideReloadInfo()
    {
        reloadInfo.SetActive(false);
    }
    // Called from reloadInfo button
    public void ReloadMod()
    {
        KeyPressSimulator.SimulateKey(WindowsInput.VirtualKeyCode.F10);
    }
    public void ReloadManualInfoConfirmedButton()
    {
        InitializeTabMod();
    }

    private void OnEnable()
    {
        InitializeTabMod();
    }

    private void InitializeTabMod()
    {
        string modPathKey = ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName;

        if (IsValidModPath(modPathKey))
        {
            if(ModManagerUtils.CheckAndCreateBackgroundKeypressIni(templateBackgroundKeypress))
            {
                textInfo.SetActive(false);
                SetObjectsActiveState(false);
                reloadManualInfo.SetActive(true);
            }
            else
            {
                HandleValidModPath(modPathKey);
            }
        }
        else
        {
            HandleInvalidModPath();
        }
    }

    private bool IsValidModPath(string modPathKey)
    {
        return PlayerPrefs.HasKey(modPathKey)
               && Directory.Exists(PlayerPrefs.GetString(modPathKey))
               && PlayerPrefs.GetString(modPathKey).TrimEnd('\\').ToLower().EndsWith(ConstantVar.ModsFolder_ValidSuffix);
    }

    private void UpdateCurrentModPath(string modPathKey)
    {
        string newModPath = PlayerPrefs.GetString(modPathKey).TrimEnd('\\').ToLower();

        if (_currentModPath != newModPath)
        {
            _modDataWasLoaded = false;
            _currentModPath = newModPath;
        }
    }

    private void HandleValidModPath(string modPathKey)
    {
        reloadManualInfo.SetActive(false);
        UpdateCurrentModPath(modPathKey);

        textInfo.SetActive(false);
        SetObjectsActiveState(true);

        LoadModData();
    }

    private void HandleInvalidModPath()
    {
        reloadManualInfo.SetActive(false);
        textInfo.SetActive(true);
        SetObjectsActiveState(false);
    }

    private void SetObjectsActiveState(bool isActive)
    {
        modSelectionObject.SetActive(isActive);
    }

    private void LoadModData()
    {
        if (_modDataWasLoaded) return;

        string modPathKey = ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName;
        string managedPath = Path.Combine(PlayerPrefs.GetString(modPathKey), ConstantVar.Managed_Folder);
        string jsonPath = Path.Combine(managedPath, ConstantVar.ModData_Json_File);

        EnsureManagedDirectoryExists(managedPath);

        if (File.Exists(jsonPath))
        {
            LoadExistingModData(jsonPath);
        }
        else
        {
            CreateNewModData(jsonPath);
        }

        groupScrollHandler.InstantiateGroup();
        groupScrollHandler.EnsureGroupManagerIniLatestVersion();
        modScrollHandler.EnsureGroupIniLatestVersion();
        _modDataWasLoaded = true;
    }

    private void EnsureManagedDirectoryExists(string managedPath)
    {
        if (!Directory.Exists(managedPath))
        {
            Directory.CreateDirectory(managedPath);
        }
    }

    private void LoadExistingModData(string jsonPath)
    {
        string jsonData = File.ReadAllText(jsonPath);
        modData = JsonUtility.FromJson<ModData>(jsonData);

        EnsureGroupPathIsSync();
        EnsureModDataCompatibleWithLatestVersion();
    }

    private void EnsureModDataCompatibleWithLatestVersion()
    {
        bool changed = false;
        foreach (var item in modData.groupDatas)
        {
            if(item.groupName == "AddButton") continue;
            while(item.modFolders.Count < ConstantVar.Total_MaxModPerGroup)
            {
                changed = true;
                item.modFolders.Add("Empty");
            }
            while(item.modNames.Count < ConstantVar.Total_MaxModPerGroup)
            {
                changed = true;
                item.modNames.Add("Empty");
            }
        }
        if(changed) ModManagerUtils.SaveManagedModData();
    }

    //In case user move/copy "...MANAGED..." folder
    private void EnsureGroupPathIsSync()
    {
        string managedPath = Path.Combine(PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName), ConstantVar.Managed_Folder);
        bool groupPathDesync = false;

        foreach (var groupData in modData.groupDatas)
        {
            if(groupData.groupPath == "AddButton") continue;
            string parentPath = Path.GetDirectoryName(groupData.groupPath);
            if (parentPath != null)
            {
                if(parentPath != managedPath)
                {
                    parentPath = managedPath;
                    groupData.groupPath = Path.Combine(parentPath, Path.GetFileName(groupData.groupPath));
                    groupPathDesync = true;
                }
            }
        }

        if(groupPathDesync)
        {
            ModManagerUtils.SaveManagedModData();
        }
    }

    private void CreateNewModData(string jsonPath)
    {
        GroupData addButtonGroupData = new GroupData
        {
            groupName = "AddButton",
            groupPath = "AddButton",
            modNames = new(),
            modFolders = new(),
        };

        modData = new ModData
        {
            groupDatas = new List<GroupData> { addButtonGroupData }
        };

        string jsonDataToSave = JsonUtility.ToJson(modData, true);
        File.WriteAllText(jsonPath, jsonDataToSave);
    }
}