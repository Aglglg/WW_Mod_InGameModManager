using System;
using System.Collections;
using System.IO;
using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using TMPro;
using UnityEngine;
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

    // Called from TabModManager after mod path validation and ModData loading
    public void InstantiateGroup()
    {
        ClearExistingGroups();

        for (int i = 0; i < TabModManager.modData.groupDatas.Count; i++)
        {
            if (i == 0)
            {
                simpleScrollSnap.Add(groupPrefabAddButton, i); // Add button
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
        if (context.phase != InputActionPhase.Performed) return;

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
        titleInputField.text = Path.GetFileName(TabModManager.modData.groupDatas[groupIndex].groupPath);
    }

    private async void SetGroupIcon(int groupIndex)
    {
        string iconPath = Path.Combine(TabModManager.modData.groupDatas[groupIndex].groupPath, ConstantVar.MODDATA_ICON_FILE);
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
}