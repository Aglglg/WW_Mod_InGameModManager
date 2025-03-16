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
        GameObject selectedUIObject = EventSystem.current.currentSelectedGameObject;
        if(selectedUIObject != null)
        {
            bool isSelectingInputField = selectedUIObject.TryGetComponent<TMP_InputField>(out var inputField);
            if(isSelectingInputField) return;
        }

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
        titleInputField.text = Path.GetFileName(TabModManager.modData.groupDatas[groupIndex].groupPath).TrimEnd('_');
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
            imageIcon.transform.localScale = Vector3.one;
        }
    }

    private void LoadDefaultImageIcon(RawImage imageIcon)
    {
        imageIcon.texture = groupDefaultIcon;
        imageIcon.rectTransform.sizeDelta = new Vector2(GroupImageIconDefaultWidth, GroupImageIconDefaultHeight);
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

    #region ContextMenu
    //Called from GroupItem/ButtonRightClick if clicked
    public void ShowContextMenu(Transform groupItem)
    {
        if(groupItem.GetSiblingIndex() == _currentTargetIndex && _currentTargetIndex != 0)
        {
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

    //Called from context menu buttons & add group button
    public void AddGroupButton()
    {
        //Template default group
        string groupPath = GetAvailableGroupPath(contentGroupTransform.childCount);
        GroupData newGroupData = new()
        {
            groupPath = groupPath,
            modPaths = new[]
            {
                "NoneButton",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty",
                "Empty"
            }
        };

        TabModManager.modData.groupDatas.Add(newGroupData);
        ModManagerUtils.SaveManagedModData();
        
        simpleScrollSnap.AddToBack(groupPrefab);

        Transform groupTransform = contentGroupTransform.GetChild(contentGroupTransform.childCount - 1);
        RawImage imageIcon = groupTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();
        LoadDefaultImageIcon(imageIcon);
        groupTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>().text = Path.GetFileName(groupPath).TrimEnd('_');

        simpleScrollSnap.GoToLastPanel();
    }

    public void ChangeIconGroupButton()
    {
        var extensions = new []
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
        };
        string[] inputPath = StandaloneFileBrowser.OpenFilePanel($"Select image ({ConstantVar.WidthHeight_GroupIcon}:{ConstantVar.WidthHeight_GroupIcon})", "", extensions, false);
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

        //TO DO: REVERT INI FILES THAT HAVE BEEN MODIFIED, & Sometimes on removed folder already have same folder.
    }

    private string GetAvailableGroupPath(int index)
    {
        string groupPath = Path.Combine(
            PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName),
            ConstantVar.Managed_Path,
            $"Group {index}"
        ) + '_';

        while (Directory.Exists(groupPath))
        {
            index++;
            groupPath = Path.Combine(
                PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName),
                ConstantVar.Managed_Path,
                $"Group {index}"
            ) + '_';
        }
        Directory.CreateDirectory(groupPath);
        return groupPath;
    }

    //Called from group item inputfield
    public void OnDoneRename(string text)
    {
        string managedModPath = Path.Combine(PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName), ConstantVar.Managed_Path);
        string oldGroupPath = TabModManager.modData.groupDatas[_currentTargetIndex].groupPath;
        string newGroupPath = Path.Combine(managedModPath, text) + '_';

        Transform groupTransform = contentGroupTransform.GetChild(_currentTargetIndex);
        TMP_InputField titleInputField = groupTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();

        try
        {
            Directory.Move(oldGroupPath, newGroupPath);
            TabModManager.modData.groupDatas[_currentTargetIndex].groupPath = newGroupPath;
            ModManagerUtils.SaveManagedModData();
            titleInputField.text = Path.GetFileName(newGroupPath).TrimEnd('_');
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            titleInputField.text = Path.GetFileName(oldGroupPath).TrimEnd('_');
        }
        titleInputField.interactable = false;
    }
    #endregion
}