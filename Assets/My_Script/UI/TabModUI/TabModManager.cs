using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TabModManager : MonoBehaviour
{
    public static ModData modData;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private GroupScrollHandler groupScrollHandler;
    [SerializeField] private GameObject textInfo;
    [SerializeField] private GameObject modSelectionObject;

    private string _currentModPath;
    private bool _modDataWasLoaded = false;
    private int _selectedModSlot = 0;

    // Called from a button click
    public void GoToSetting()
    {
        uiManager.ChangeTab((int)TabState.Setting);
    }

    private void SelectMod(int modSlot)
    {
        _selectedModSlot = modSlot;
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
            UpdateCurrentModPath(modPathKey);
            HandleValidModPath();
            LoadModData();
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

    private void HandleValidModPath()
    {
        textInfo.SetActive(false);
        SetObjectsActiveState(true);
    }

    private void HandleInvalidModPath()
    {
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
        string managedPath = Path.Combine(PlayerPrefs.GetString(modPathKey), ConstantVar.Managed_Path);
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
    }

    private void CreateNewModData(string jsonPath)
    {
        GroupData addButtonGroupData = new GroupData
        {
            groupPath = "AddButton",
            modPaths = new string[]
            {}
        };

        modData = new ModData
        {
            groupDatas = new List<GroupData> { addButtonGroupData }
        };

        string jsonDataToSave = JsonUtility.ToJson(modData, true);
        File.WriteAllText(jsonPath, jsonDataToSave);
    }
}