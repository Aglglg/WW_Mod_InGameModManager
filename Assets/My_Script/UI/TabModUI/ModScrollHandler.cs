using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ModScrollHandler : MonoBehaviour
{
    private const int TitleTextChildIndex = 0;
    private const int ImageMaskIconChildIndex = 1;
    private const int ImageIconChildIndexInMaskTransform = 0;
    private const float ModImageIconDefaultWidth = 215;
    private const float ModImageIconDefaultHeight = 309;
    
    public GameObject modContextMenu;
    [SerializeField] private Texture2D modDefaultIcon;
    [SerializeField] private float animationDuration;
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;
    [SerializeField] private Transform contentModTransform;
    [SerializeField] private GameObject keyIcon;
    [SerializeField] private GameObject selectButton;
    [SerializeField] private float selectedScale;
    [SerializeField] private float notSelectedScale;

    private int _currentTargetIndex = 0;
    private int _previousTargetIndex = 0;

    private void Start()
    {
        InitializeModItems();
    }

    //Called from ModItem if clicked
    public void GoToSelectedMod(Transform modItem)
    {
        simpleScrollSnap.GoToPanel(modItem.GetSiblingIndex());
    }
    public void ShowContextMenu(Transform modItem)
    {
        if(modItem.GetSiblingIndex() == _currentTargetIndex && _currentTargetIndex != 0)
        {
            modContextMenu.SetActive(!modContextMenu.activeSelf);
            if(modContextMenu.activeSelf)
            {
                EventSystem.current.SetSelectedGameObject(modContextMenu.transform.GetChild(0).gameObject);//Child 0 hidden button as helper
                StartCoroutine(CheckToHideContextMenu());
            }
        }
    }
    private IEnumerator CheckToHideContextMenu()
    {
        while (modContextMenu.activeSelf)
        {
            if(EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.transform.parent != modContextMenu.transform)
            {
                modContextMenu.SetActive(false);
            }
            yield return null;
        }
    }

    // Called from GroupScrollHandler
    public void UpdateModItems(int selectedGroupIndex, GroupData selectedGroupData)
    {
        bool isAddButtonGroup = selectedGroupIndex == 0;

        SetModItemsActive(isAddButtonGroup, selectedGroupData); //If add button, disable
    }

    // Called from PlayerInput
    public void OnModNavigate(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed || modContextMenu.activeSelf) return;

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

        ScaleModToSelected(targetIndex);
        ScaleModToDefault(_previousTargetIndex);

        _previousTargetIndex = targetIndex;
    }

    // Called from SimpleScrollSnap when a panel is centered
    public void OnPanelCentered(int targetIndex, int previousIndex)
    {
        _currentTargetIndex = targetIndex;
        ScaleModToSelected(targetIndex);
        ScaleModToDefault(previousIndex);

        // SimulateKeyPress to change active mod (placeholder for additional logic)
    }

    private void InitializeModItems()
    {
        foreach (Transform child in contentModTransform)
        {
            child.localScale = new Vector3(notSelectedScale, notSelectedScale, notSelectedScale);
        }

        ScaleModToSelected(0); // Scale the first mod to selected state
    }

    private void SetModItemsActive(bool isAddButtonGroup, GroupData selectedGroupData)
    {
        //if group == add group button, first show it(1), then disable it(0), if not, disable it then show it(like refresh animation)
        GetComponent<CanvasGroup>().DOFade(isAddButtonGroup ? 1 : 0, animationDuration/2).OnComplete(
            () =>
            {
                if(!isAddButtonGroup)
                {
                    SetModTitle(selectedGroupData);
                    SetModIcon(selectedGroupData);
                    GetComponent<CanvasGroup>().interactable = true;
                }
                else
                {
                    GetComponent<CanvasGroup>().interactable = false;
                }
                GetComponent<CanvasGroup>().DOFade(isAddButtonGroup ? 0 : 1, animationDuration/2);
                EventSystem.current.SetSelectedGameObject(selectButton);
            }
        );
    }

    private void ScaleModToSelected(int modIndex)
    {
        contentModTransform.GetChild(modIndex).DOScale(new Vector3(selectedScale, selectedScale, selectedScale), animationDuration);
    }

    private void ScaleModToDefault(int modIndex)
    {
        contentModTransform.GetChild(modIndex).DOScale(new Vector3(notSelectedScale, notSelectedScale, notSelectedScale), animationDuration);
    }


    #region LOAD MOD ICON & TITLE
    private void SetModTitle(GroupData selectedGroupData)
    {
        //First, reset all
        for (int i = 0; i < contentModTransform.childCount; i++)
        {
            if(i == 0) continue; //None Button
            Transform modTransform = contentModTransform.GetChild(i);
            TMP_InputField titleInputField = modTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
            titleInputField.text = "Empty";
        }

        for (int i = 0; i < selectedGroupData.modPaths.Count; i++)
        {
            if(i == 0) continue; //None Button
            Transform modTransform = contentModTransform.GetChild(i);
            TMP_InputField titleInputField = modTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
            titleInputField.text = Path.GetFileName(selectedGroupData.modPaths[i]);
        }
    }
    private async void SetModIcon(GroupData selectedGroupData)
    {
        //First, reset all
        for (int i = 0; i < contentModTransform.childCount; i++)
        {
            if(i == 0) continue; //None Button
            Transform modTransform = contentModTransform.GetChild(i);
            RawImage imageIcon = modTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();
            imageIcon.gameObject.SetActive(false);
        }

        for (int i = 0; i < selectedGroupData.modPaths.Count; i++)
        {
            if(i == 0) continue; //None Button
            string iconPath = Path.Combine(selectedGroupData.modPaths[i], ConstantVar.ModData_Icon_File);
            Transform modTransform = contentModTransform.GetChild(i);
            RawImage imageIcon = modTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();
            imageIcon.gameObject.SetActive(true);
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
    }
    private void LoadDefaultImageIcon(RawImage imageIcon)
    {
        imageIcon.texture = modDefaultIcon;
        imageIcon.rectTransform.sizeDelta = new Vector2(ModImageIconDefaultWidth, ModImageIconDefaultHeight);
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