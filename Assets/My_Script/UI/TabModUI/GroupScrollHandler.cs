using System;
using System.Collections;
using System.IO;
using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GroupScrollHandler : MonoBehaviour
{
    private const int TitleTextChildIndex = 0;
    private const int ImageMaskIconChildIndex = 1;
    private const int ImageIconChildIndexInMaskTransform = 0;
    private const float GroupImageIconDefaultWidth = 160;
    private const float GroupImageIconDefaultHeight = 160;

    [SerializeField] private GameObject reloadInfo;
    [SerializeField] private GameObject operationInfo;

    [SerializeField] private TextAsset groupManagerIniTemplate;

    [SerializeField] private GameObject groupContextMenu;
    [SerializeField] private Texture2D groupDefaultIcon;
    [SerializeField] private ModScrollHandler modScrollHandler;
    [SerializeField] private GameObject groupPrefab;
    [SerializeField] private GameObject groupPrefabAddButton;
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;
    [SerializeField] private Transform contentGroupTransform;
    [SerializeField] private float animationDuration;
    [SerializeField] private float selectedScale;
    [SerializeField] private float notSelectedScale;

    private GroupData _selectedGroupData;
    private int _previousTargetIndex = 0;
    private int _currentTargetIndex = 0;


    // Called from TabModManager after mod path validation and ModData loading
    public void InstantiateGroup()
    {
        ClearExistingGroups();

        for (int i = 0; i < TabModManager.modData.groupDatas.Count; i++)
        {
            if (i == 0)// Add button
            {
                simpleScrollSnap.Add(groupPrefabAddButton, i);
                continue;
            }

            simpleScrollSnap.Add(groupPrefab, i);
            SetGroupTitle(i);
            SetGroupIcon(i);
        }

        InitializeSelectedGroup();
        ScaleAllGroupsToDefault();
        ScaleFirstGroupToSelected();
    }

    // Called from PlayerInput
    public void OnGroupNavigate(InputAction.CallbackContext context)
    {
        //If typing
        GameObject selectedUIObject = EventSystem.current.currentSelectedGameObject;
        if(selectedUIObject != null)
        {
            bool isSelectingInputField = selectedUIObject.TryGetComponent<TMP_InputField>(out var inputField);
            if(isSelectingInputField) return;
        }

        //If not on Mod tab
        if(UIManager.CurrentTabState != TabState.Mod) return;

        if (context.phase != InputActionPhase.Performed || modScrollHandler.modContextMenu.activeSelf || groupContextMenu.activeSelf) return;

        if (context.ReadValue<float>() > 0)
        {
            simpleScrollSnap.GoToNextPanel();
        }
        else
        {
            simpleScrollSnap.GoToPreviousPanel();
        }
    }

    // Called from SimpleScrollSnap when a panel is being selected
    public void OnPanelSelecting(int targetIndex)
    {
        if (targetIndex == _previousTargetIndex) return;

        ScaleGroupToSelected(targetIndex);
        ScaleGroupToDefault(_previousTargetIndex);

        _previousTargetIndex = targetIndex;
    }

    // Called from SimpleScrollSnap when a panel is centered
    public void OnPanelCentered(int targetIndex, int previousIndex)
    {
        _currentTargetIndex = targetIndex;
        ScaleGroupToSelected(targetIndex);
        ScaleGroupToDefault(previousIndex);

        UpdateSelectedGroupData(targetIndex);
    }

    private void ClearExistingGroups()
    {
        int totalGroups = contentGroupTransform.childCount;
        for (int i = 0; i < totalGroups; i++)
        {
            simpleScrollSnap.RemoveFromFront();
        }
    }

    private void InitializeSelectedGroup()
    {
        _selectedGroupData = TabModManager.modData.groupDatas[0];
        modScrollHandler.UpdateModItems(0, _selectedGroupData);
    }

    private void ScaleAllGroupsToDefault()
    {
        foreach (Transform child in contentGroupTransform)
        {
            child.localScale = new Vector3(notSelectedScale, notSelectedScale, notSelectedScale);
            child.GetChild(TitleTextChildIndex).transform.DOScale(Vector2.zero, animationDuration);
        }
    }

    private void ScaleFirstGroupToSelected()
    {
        Transform firstGroup = contentGroupTransform.GetChild(0);
        firstGroup.localScale = new Vector3(selectedScale, selectedScale, selectedScale);
        firstGroup.GetChild(TitleTextChildIndex).transform.DOScale(Vector2.one, animationDuration);
    }

    private void ScaleGroupToSelected(int groupIndex)
    {
        Transform groupTransform = contentGroupTransform.GetChild(groupIndex);
        groupTransform.DOScale(new Vector3(selectedScale, selectedScale, selectedScale), animationDuration);
        groupTransform.GetChild(TitleTextChildIndex).transform.DOScale(Vector2.one, animationDuration);
    }

    private void ScaleGroupToDefault(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= contentGroupTransform.childCount) return;

        Transform groupTransform = contentGroupTransform.GetChild(groupIndex);
        groupTransform?.DOScale(new Vector3(notSelectedScale, notSelectedScale, notSelectedScale), animationDuration);
        groupTransform?.GetChild(TitleTextChildIndex).transform.DOScale(Vector2.zero, animationDuration);
    }

    private void UpdateSelectedGroupData(int targetIndex)
    {
        _selectedGroupData = TabModManager.modData.groupDatas[targetIndex];
        modScrollHandler.UpdateModItems(targetIndex, _selectedGroupData);
    }


    #region LOAD GROUP ICON & TITLE
    private void SetGroupTitle(int groupIndex)
    {
        Transform groupTransform = contentGroupTransform.GetChild(groupIndex);
        TMP_InputField titleInputField = groupTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
        titleInputField.text = TabModManager.modData.groupDatas[groupIndex].groupName;
    }

    private async void SetGroupIcon(int groupIndex)
    {
        string iconPath = Path.Combine(TabModManager.modData.groupDatas[groupIndex].groupPath, ConstantVar.ModData_Icon_File);
        Transform groupTransform = contentGroupTransform.GetChild(groupIndex);
        RawImage imageIcon = groupTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();
        if(File.Exists(iconPath))
        {
            byte[] ddsBytes = await File.ReadAllBytesAsync(iconPath);
            imageIcon.texture = LoadDDS(ddsBytes);
            imageIcon.transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            LoadDefaultImageIcon(imageIcon);
        }
    }

    private void LoadDefaultImageIcon(RawImage imageIcon)
    {
        imageIcon.texture = groupDefaultIcon;
        imageIcon.rectTransform.sizeDelta = new Vector2(GroupImageIconDefaultWidth, GroupImageIconDefaultHeight);
        imageIcon.transform.localScale = Vector3.one;
    }

    //DDS is faster, I think, jpg/png slow and unity can only load texture on main thread, so it will freeze/stutter if use png/jpg
    private Texture2D LoadDDS(byte[] ddsBytes)
    {
        int height = ddsBytes[13] * 256 + ddsBytes[12];
        int width = ddsBytes[17] * 256 + ddsBytes[16];
        Texture2D texture = new Texture2D(width, height, TextureFormat.BC7, false);


        int DDS_HEADER_SIZE = 148;
        byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();

        return texture;
    }
    #endregion

    #region CONTEXT MENU
    //Called from GroupItem/ButtonRightClick if clicked
    public void ShowContextMenu(Transform groupItem)
    {
        if(groupItem.GetSiblingIndex() == _currentTargetIndex && _currentTargetIndex != 0)
        {
            groupContextMenu.GetComponent<CanvasGroup>().alpha = 0;
            groupContextMenu.GetComponent<CanvasGroup>().DOFade(1, animationDuration);
            groupContextMenu.SetActive(!groupContextMenu.activeSelf);
            if(groupContextMenu.activeSelf)
            {
                EventSystem.current.SetSelectedGameObject(groupContextMenu.transform.GetChild(0).gameObject);//Child 0 hidden button as helper
                StartCoroutine(CheckToHideContextMenu());
            }
        }
    }
    private IEnumerator CheckToHideContextMenu()
    {
        while (groupContextMenu.activeSelf)
        {
            if(EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.transform.parent != groupContextMenu.transform)
            {
                groupContextMenu.SetActive(false);
            }
            yield return null;
        }
    }

    public void ReorderGroup(bool toLeft)
    {
        if(toLeft)
        {
            if(_currentTargetIndex == 1) return;
        }
        else
        {
            if(_currentTargetIndex == TabModManager.modData.groupDatas.Count -1) return;
        }
        
        int targetIndex = toLeft ? _currentTargetIndex - 1 : _currentTargetIndex + 1;

        GameObject currentGroupObject = Instantiate(simpleScrollSnap.Panels[_currentTargetIndex].gameObject);
        currentGroupObject.name = currentGroupObject.name.Replace("(Clone)", "");
        GroupData currentGroupData = TabModManager.modData.groupDatas[_currentTargetIndex];

        simpleScrollSnap.Remove(_currentTargetIndex);
        TabModManager.modData.groupDatas.RemoveAt(_currentTargetIndex);

        simpleScrollSnap.Add(currentGroupObject, targetIndex);
        TabModManager.modData.groupDatas.Insert(targetIndex, currentGroupData);

        Destroy(currentGroupObject);

        ModManagerUtils.SaveManagedModData();
        simpleScrollSnap.GoToPanel(targetIndex);
        _currentTargetIndex = targetIndex; //To make sure, incase OnPanelCentered not called
        ToggleOperationInfo("Group re-ordered");
    }

    //Called from context menu buttons & add group button
    public void AddGroupButton()
    {
        if(TabModManager.modData.groupDatas.Count >= ConstantVar.Total_MaxGroup)
        {
            ToggleOperationInfo("Max group reached.");
            return;
        }

        CheckAndCreateGroupManagerIni();

        string groupPath = GetAvailableGroupPath();
        string groupName = Path.GetFileName(groupPath);

        Directory.CreateDirectory(groupPath);

        //Template default group data
        GroupData newGroupData = new()
        {
            groupName = groupName,
            groupPath = groupPath,
            modNames = new()
            {
                "NoneButton"
            },
            modFolders = new()
            {
                "NoneButton"
            }
        };
        
        while(newGroupData.modFolders.Count < ConstantVar.Total_MaxModPerGroup)
        {
            newGroupData.modFolders.Add("Empty");
        }
        while(newGroupData.modNames.Count < ConstantVar.Total_MaxModPerGroup)
        {
            newGroupData.modNames.Add("Empty");
        }

        TabModManager.modData.groupDatas.Add(newGroupData);
        ModManagerUtils.SaveManagedModData();
        
        simpleScrollSnap.AddToBack(groupPrefab);

        Transform groupTransform = contentGroupTransform.GetChild(contentGroupTransform.childCount - 1);
        RawImage imageIcon = groupTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();
        LoadDefaultImageIcon(imageIcon);
        groupTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>().text = groupName;

        simpleScrollSnap.GoToLastPanel();
    }

    public void ChangeIconGroupButton()
    {
        var extensions = new []
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
        };
        string[] inputPath = StandaloneFileBrowser.OpenFilePanel($"Select image ({ConstantVar.WidthHeight_GroupIcon}:{ConstantVar.WidthHeight_GroupIcon}px)", "", extensions, false);
        if(inputPath.Length > 0)
        {
            ModManagerUtils.CreateIcon(inputPath[0], Path.Combine(TabModManager.modData.groupDatas[_currentTargetIndex].groupPath, "icon.png"), true);
        }
        SetGroupIcon(_currentTargetIndex);
    }

    public void RenameGroupButton()
    {
        Transform groupTransform = contentGroupTransform.GetChild(_currentTargetIndex);
        TMP_InputField titleInputField = groupTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
        titleInputField.interactable = true;
        EventSystem.current.SetSelectedGameObject(titleInputField.gameObject);
    }

    public void RemoveGroupButton()
    {
        string removedPath = Path.Combine(PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName), ConstantVar.Removed_Path);
        string targetGroupPath = TabModManager.modData.groupDatas[_currentTargetIndex].groupPath;
        string targetGroupPathRemoved = Path.Combine(removedPath, Path.GetFileName(targetGroupPath));
        
        try
        {
            if(!Directory.Exists(removedPath))
            {
                Directory.CreateDirectory(removedPath);
            }
            while(Directory.Exists(targetGroupPathRemoved))
            {
                targetGroupPathRemoved += '_';
            }

            Directory.Move(targetGroupPath, targetGroupPathRemoved);
            TabModManager.modData.groupDatas.RemoveAt(_currentTargetIndex);
            ModManagerUtils.SaveManagedModData();
            simpleScrollSnap.Remove(_currentTargetIndex);
            simpleScrollSnap.GoToPanel(_currentTargetIndex - 1);
            OnPanelCentered(_currentTargetIndex - 1, _currentTargetIndex);
            if(ModManagerUtils.RevertManagedMod(targetGroupPathRemoved))
            {
                reloadInfo.SetActive(true);
                ToggleOperationInfo($"Group removed and moved to <color=yellow>{ConstantVar.Removed_Path}</color>");
            }
            else
            {
                ToggleOperationInfo($"Group removed.");
            }
        }
        catch(IOException ex)
        {
            string errorMessage;
            if(ex.Message.Contains("denied"))
            {
                errorMessage = "Access denied. Close File Explorer or another apps.";
            }
            else if(ex.Message.Contains("in use"))
            {
                errorMessage = "Folder of the group in use. Close File Explorer or another apps.";
            }
            else
            {
                errorMessage = ex.Message;
            }

            ToggleOperationInfo(errorMessage);
        }
    }

    private string GetAvailableGroupPath()
    {
        int startingIndex = 1; //0 is Add group button, so it's 1
        string groupPath = Path.Combine(
            PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName),
            ConstantVar.Managed_Path,
            $"group_{startingIndex}"
        );

        while (Directory.Exists(groupPath))
        {
            startingIndex++;
            groupPath = Path.Combine(
                PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName),
                ConstantVar.Managed_Path,
                $"group_{startingIndex}"
            );
        }
        return groupPath;
    }

    //Called from group item inputfield
    public void OnDoneRename(string text)
    {
        Transform groupTransform = contentGroupTransform.GetChild(_currentTargetIndex);
        TMP_InputField titleInputField = groupTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();


        TabModManager.modData.groupDatas[_currentTargetIndex].groupName = text;
        ModManagerUtils.SaveManagedModData();

        titleInputField.text = text;
        titleInputField.interactable = false;
    }

    private void CheckAndCreateGroupManagerIni()
    {
        string groupManagerIniPath = Path.Combine(PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName),
                                    ConstantVar.Managed_Path, ConstantVar.IniFile_GroupManager);
        if(!File.Exists(groupManagerIniPath))
        {
            string content = groupManagerIniTemplate.text;
            File.WriteAllText(groupManagerIniPath, content);
            reloadInfo.SetActive(true);
        }
    }

    //Called from TabModManager if data loaded
    public void EnsureGroupManagerIniLatestVersion()
    {
        string groupManagerIniPath = Path.Combine(PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName),
                                    ConstantVar.Managed_Path, ConstantVar.IniFile_GroupManager);
        if(File.Exists(groupManagerIniPath))
        {
            string firstLine = "";
            using (StreamReader reader = new StreamReader(groupManagerIniPath))
            {
                firstLine = reader.ReadLine();
            }

            if(!firstLine.Contains(Application.version))
            {
                string content = groupManagerIniTemplate.text;
                File.WriteAllText(groupManagerIniPath, content);
                reloadInfo.SetActive(true);
            }   
        }
    }
    #endregion

    #region OPERATION INFO
    private Coroutine infoTextCoroutine;
    private const int infoTimeout = 5;
    private void ToggleOperationInfo(string info)
    {
        if(infoTextCoroutine != null) StopCoroutine(infoTextCoroutine);
        infoTextCoroutine = StartCoroutine(ToggleOperationInfoCoroutine(info));
    }
    private IEnumerator ToggleOperationInfoCoroutine(string info)
    {
        operationInfo.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        operationInfo.GetComponent<TextMeshProUGUI>().text = info;
        operationInfo.SetActive(true);
        yield return new WaitForSeconds(infoTimeout);
        operationInfo.SetActive(false);
        infoTextCoroutine = null;
    }
    #endregion
}