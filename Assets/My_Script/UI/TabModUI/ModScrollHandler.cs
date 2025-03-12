using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using TMPro;
using UnityEngine;
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
    
    [SerializeField] private Texture2D modDefaultIcon;
    [SerializeField] private float animationDuration;
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;
    [SerializeField] private Transform contentModTransform;
    [SerializeField] private GameObject keyIcon;
    [SerializeField] private GameObject selectButton;
    [SerializeField] private float selectedScale;
    [SerializeField] private float notSelectedScale;

    private int _selectedModIndex = 0;
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

    // Called from GroupScrollHandler
    public void UpdateModItems(int selectedGroupIndex, GroupData selectedGroupData)
    {
        bool isAddButtonGroup = selectedGroupIndex == 0;

        SetModItemsActive(!isAddButtonGroup); //If add button, disable
        if(!isAddButtonGroup)
        {
            SetModTitle(selectedGroupData);
            SetModIcon(selectedGroupData);
        }
    }

    // Called from PlayerInput
    public void OnModNavigate(InputAction.CallbackContext context)
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

        ScaleModToSelected(targetIndex);
        ScaleModToDefault(_previousTargetIndex);

        _previousTargetIndex = targetIndex;
    }

    // Called from SimpleScrollSnap when a panel is centered
    public void OnPanelCentered(int targetIndex, int previousIndex)
    {
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

    private void SetModItemsActive(bool isActive)
    {
        GetComponent<CanvasGroup>().DOFade(isActive ? 1 : 0, animationDuration/2);
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
    private void SetModIcon(GroupData selectedGroupData)
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
            string iconPath = Path.Combine(selectedGroupData.modPaths[i], ConstantVar.MODDATA_ICON_FILE);
            Transform modTransform = contentModTransform.GetChild(i);
            RawImage imageIcon = modTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<RawImage>();
            imageIcon.gameObject.SetActive(true);
            if(File.Exists(iconPath))
            {
                StartCoroutine(LoadModImageIcon(new System.Uri(iconPath).AbsoluteUri, imageIcon));
            }
            else
            {
                LoadDefaultImageIcon(imageIcon);
            }
        }
    }
    private IEnumerator LoadModImageIcon(string uri, RawImage imageIcon)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            imageIcon.texture = texture;
            SetImageIconWidthHeight(imageIcon.rectTransform, texture);
        }
        else
        {
            LoadDefaultImageIcon(imageIcon);
        }
    }
    private void SetImageIconWidthHeight(RectTransform rectTransform, Texture2D texture)
    {
        bool isImageLandscape = texture.width > texture.height;
        float width;
        float height;
        if(isImageLandscape) //Maintain default height, change width
        {
            height = ModImageIconDefaultHeight;
            float divideBy = texture.height / ModImageIconDefaultHeight;
            width = texture.width / divideBy;
        }
        else //Maintain default width, change height
        {
            width = ModImageIconDefaultWidth;
            float divideBy = texture.width / ModImageIconDefaultWidth;
            height = texture.height / divideBy;
        }
        rectTransform.sizeDelta = new Vector2(width, height);
    }
    private void LoadDefaultImageIcon(RawImage imageIcon)
    {
        imageIcon.texture = modDefaultIcon;
        imageIcon.rectTransform.sizeDelta = new Vector2(ModImageIconDefaultWidth, ModImageIconDefaultHeight);
    }
    #endregion
}