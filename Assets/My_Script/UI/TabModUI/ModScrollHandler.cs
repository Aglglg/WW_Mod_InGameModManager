using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using SFB;
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
    
    [SerializeField] private GameObject reloadInfo;
    [SerializeField] private GameObject operationInfo;

    [SerializeField] private Button contextMenuAddButton;
    [SerializeField] private Button contextMenuChangeIconButton;
    [SerializeField] private Button contextMenuRenameButton;
    [SerializeField] private Button contextMenuKeybindButton;
    [SerializeField] private Button contextMenuFixModButton;
    [SerializeField] private Button contextMenuRemoveButton;

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
    private GroupData _currentSelectedGroup;

    private void Start()
    {
        InitializeModItems();
    }

    //Called from ModItem if clicked
    public void GoToSelectedMod(Transform modItem)
    {
        simpleScrollSnap.GoToPanel(modItem.GetSiblingIndex());
    }

    // Called from GroupScrollHandler
    public void UpdateModItems(int selectedGroupIndex, GroupData selectedGroupData)
    {
        bool isAddButtonGroup = selectedGroupIndex == 0;
        _currentSelectedGroup = selectedGroupData;

        SetModItemsActive(isAddButtonGroup); //If add button, disable
    }

    // Called from PlayerInput
    public void OnModNavigate(InputAction.CallbackContext context)
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
            child.GetComponent<ButtonRightClick>().modScrollHandler = this;
        }

        ScaleModToSelected(0); // Scale the first mod to selected state
    }

    private void SetModItemsActive(bool isAddButtonGroup)
    {
        //if group == add group button, first show it(1), then disable it(0), if not, disable it then show it(like refresh animation)
        GetComponent<CanvasGroup>().DOFade(isAddButtonGroup ? 1 : 0, animationDuration/2).OnComplete(
            () =>
            {
                if(!isAddButtonGroup)
                {
                    SetModTitle();
                    SetModIcon();
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
    private void SetModTitle()
    {
        //First, reset all
        for (int i = 0; i < contentModTransform.childCount; i++)
        {
            if(i == 0) continue; //None Button
            Transform modTransform = contentModTransform.GetChild(i);
            TMP_InputField titleInputField = modTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
            titleInputField.text = "Empty";
        }

        for (int i = 0; i < _currentSelectedGroup.modNames.Length; i++)
        {
            if(i == 0) continue; //None Button
            if(_currentSelectedGroup.modNames[i] == "Empty") continue; //Empty
            Transform modTransform = contentModTransform.GetChild(i);
            TMP_InputField titleInputField = modTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
            titleInputField.text = _currentSelectedGroup.modNames[i].TrimEnd('_');
        }
    }
    private async void SetModIcon()
    {
        //First, reset all
        for (int i = 0; i < contentModTransform.childCount; i++)
        {
            if(i == 0) continue; //None Button
            Transform modTransform = contentModTransform.GetChild(i);
            RawImage imageIcon = modTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();
            imageIcon.gameObject.SetActive(false);
        }

        for (int i = 0; i < _currentSelectedGroup.modNames.Length; i++)
        {
            if(i == 0) continue; //None Button
            if(_currentSelectedGroup.modNames[i] == "Empty") continue; //Empty
            string iconPath = Path.Combine(_currentSelectedGroup.groupPath, _currentSelectedGroup.modNames[i], ConstantVar.ModData_Icon_File);
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
    private async void SetModIconIndividual(int selectedIndex)
    {
        string iconPath = Path.Combine(_currentSelectedGroup.groupPath, _currentSelectedGroup.modNames[selectedIndex], ConstantVar.ModData_Icon_File);
        Transform modTransform = contentModTransform.GetChild(selectedIndex);
        RawImage imageIcon = modTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();
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

    #region CONTEXT MENU
    public void ShowContextMenu(Transform modItem)
    {
        if(modItem.GetSiblingIndex() == _currentTargetIndex && _currentTargetIndex != 0)
        {
            SetContextMenuButtonInteractible();
            modContextMenu.GetComponent<CanvasGroup>().alpha = 0;
            modContextMenu.GetComponent<CanvasGroup>().DOFade(1, animationDuration);
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
    private void SetContextMenuButtonInteractible()
    {
        Color whiteColorInteractible = Color.white;
        Color whiteColorNotInteractible = Color.white;
        whiteColorNotInteractible.a = 0.125f;

        contextMenuAddButton.interactable = true;
        contextMenuAddButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorInteractible;

        contextMenuChangeIconButton.interactable = true;
        contextMenuChangeIconButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorInteractible;

        contextMenuRenameButton.interactable = true;
        contextMenuRenameButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorInteractible;

        contextMenuKeybindButton.interactable = true;
        contextMenuKeybindButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorInteractible;

        contextMenuFixModButton.interactable = true;
        contextMenuFixModButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorInteractible;

        contextMenuRemoveButton.interactable = true;
        contextMenuRemoveButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorInteractible;

        if(_currentSelectedGroup.modNames[_currentTargetIndex] == "Empty")
        {
            contextMenuChangeIconButton.interactable = false;
            contextMenuChangeIconButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorNotInteractible;

            contextMenuRenameButton.interactable = false;
            contextMenuRenameButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorNotInteractible;

            contextMenuKeybindButton.interactable = false;
            contextMenuKeybindButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorNotInteractible;

            contextMenuFixModButton.interactable = false;
            contextMenuFixModButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorNotInteractible;

            contextMenuRemoveButton.interactable = false;
            contextMenuRemoveButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorNotInteractible;
        }
        else
        {
            contextMenuAddButton.interactable = false;
            contextMenuAddButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = whiteColorNotInteractible;
        }
    }

    public void AddGroupButton()
    {
        reloadInfo.SetActive(true);
    }

    public void ChangeIconModButton()
    {
        var extensions = new []
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
        };
        string[] inputPath = StandaloneFileBrowser.OpenFilePanel($"Select image ({ConstantVar.Width_ModIcon}px:{ConstantVar.Height_ModIcon}px)", "", extensions, false);
        if(inputPath.Length > 0)
        {
            ModManagerUtils.CreateIcon(inputPath[0], Path.Combine(_currentSelectedGroup.groupPath, _currentSelectedGroup.modNames[_currentTargetIndex], "icon.png"), false);
        }
        SetModIconIndividual(_currentTargetIndex);
    }

    public void RenameModButton()
    {
        Transform modTransform = contentModTransform.GetChild(_currentTargetIndex);
        TMP_InputField titleInputField = modTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
        titleInputField.interactable = true;
        EventSystem.current.SetSelectedGameObject(titleInputField.gameObject);
    }

    public void RemoveModButton()
    {
        string removedPath = Path.Combine(PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName), ConstantVar.Removed_Path);
        string targetModPath = Path.Combine(_currentSelectedGroup.groupPath, _currentSelectedGroup.modNames[_currentTargetIndex]);
        string targetModPathRemoved = Path.Combine(removedPath, Path.GetFileName(targetModPath));
        
        try
        {
            if(!Directory.Exists(removedPath))
            {
                Directory.CreateDirectory(removedPath);
            }
            while(Directory.Exists(targetModPathRemoved))
            {
                targetModPathRemoved += '_';
            }

            Directory.Move(targetModPath, targetModPathRemoved);

            _currentSelectedGroup.modNames[_currentTargetIndex] = "Empty";
            ModManagerUtils.SaveManagedModData();

            Transform modTransform = contentModTransform.GetChild(_currentTargetIndex);
            TMP_InputField titleInputField = modTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
            RawImage imageIcon = modTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();

            imageIcon.gameObject.SetActive(false);
            titleInputField.text = _currentSelectedGroup.modNames[_currentTargetIndex];

            if(ModManagerUtils.RevertManagedMod(targetModPathRemoved))
            {
                reloadInfo.SetActive(true);
                ToggleOperationInfo($"Mod removed and moved to <color=yellow>{ConstantVar.Removed_Path}</color>");
            }
            else
            {
                ToggleOperationInfo($"Mod removed.");
            }
        }
        catch(IOException ex)
        {
            string errorMessage;
            if(ex.Message.Contains("denied"))
            {
                errorMessage = "Access denied. Close File Explorer or another apps. Or run with admin privillege(or XXMI Launcher).";
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

    //Called from mod item inputfield
    public void OnDoneRename(string text)
    {
        string oldModName = _currentSelectedGroup.modNames[_currentTargetIndex];
        string newModName = text + '_';

        string oldModPath = Path.Combine(_currentSelectedGroup.groupPath, oldModName);
        string newModPath = Path.Combine(_currentSelectedGroup.groupPath, newModName);

        Transform groupTransform = contentModTransform.GetChild(_currentTargetIndex);
        TMP_InputField titleInputField = groupTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();

        if(oldModName != newModName)
        {
            try
            {
                Directory.Move(oldModPath, newModPath);
                _currentSelectedGroup.modNames[_currentTargetIndex] = newModName;
                ModManagerUtils.SaveManagedModData();
                titleInputField.text = newModName.TrimEnd('_');
            }
            catch (IOException ex)
            {
                titleInputField.text = oldModName.TrimEnd('_');
                string errorMessage;

                if(ex.Message.Contains("exist"))
                {
                    errorMessage = "Group name or folder name already exists.";
                }
                else if(ex.Message.Contains("denied"))
                {
                    errorMessage = "Access denied. Close File Explorer or another apps. Or run with admin privillege(or XXMI Launcher).";
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
        titleInputField.interactable = false;
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