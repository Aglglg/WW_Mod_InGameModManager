using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabSettingManager : MonoBehaviour
{
    #region VARIABLES
    [Header("SELECTION")]
    [SerializeField] private InputAction actionAnyMouse;
    [SerializeField] private InputAction actionAnyKeyboard;
    [SerializeField] private InputAction actionAnyGamepad;
    [SerializeField] private Image[] selectedMarkers;
    [SerializeField] private GameObject tabTitle;

    [Header("")]
    [Header("SETTINGS")]
    [SerializeField] private Transform backgroundPanel;
    [SerializeField] private Image backgroundFillImage;
    [SerializeField] private TMP_InputField modPathField;
    [SerializeField] private Slider opacitySlider;
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private InputAction actionResetTransform;

    [Header("")]
    [Header("SUPPORT")]
    [SerializeField] private Image supportImageIcon;
    private string supportLink;
    #endregion
    
    private void Start()
    {
        StartCoroutine(LoadSupportImage());
        StartCoroutine(LoadSupportLink());
    }

    private void OnEnable()
    {
        modPathField.onSubmit.AddListener(ModPathOnSubmit);
        modPathField.onEndEdit.AddListener(ModPathOnSubmit);

        actionResetTransform.Enable();
        actionResetTransform.performed += ResetPanelTransform;

        actionAnyMouse.Enable();
        actionAnyKeyboard.Enable();
        actionAnyGamepad.Enable();
        actionAnyMouse.performed += OnAnyMouse;
        actionAnyKeyboard.performed += OnAnyKeyboard;
        actionAnyGamepad.performed += OnAnyGamepad;

        SetSelectedObject(tabTitle);
    }
    private void OnDisable()
    {
        modPathField.onSubmit.RemoveListener(ModPathOnSubmit);
        modPathField.onEndEdit.RemoveListener(ModPathOnSubmit);

        actionResetTransform.Disable();
        actionResetTransform.performed -= ResetPanelTransform;

        actionAnyMouse.Disable();
        actionAnyKeyboard.Disable();
        actionAnyGamepad.Disable();
        actionAnyMouse.performed -= OnAnyMouse;
        actionAnyKeyboard.performed -= OnAnyKeyboard;
        actionAnyGamepad.performed -= OnAnyGamepad;

        foreach (Image image in selectedMarkers)
        {
            image.gameObject.SetActive(false);
        }
    }

    private void SetSelectedObject(GameObject selectedObject)
    {
        EventSystem.current.SetSelectedGameObject(selectedObject);
    }

    private void ResetPanelTransform(InputAction.CallbackContext context)
    {
        Debug.Log("RESET SCALE & POS");
        float panelScale = ConstantVar.DEFAULT_SCALE;
        backgroundPanel.localScale = new Vector3(panelScale, panelScale, panelScale);
        PlayerPrefs.SetFloat(ConstantVar.PLAYERPERFKEY_SCALE, panelScale);

        backgroundPanel.localPosition = Vector3.zero;
    }

    private void OnAnyMouse(InputAction.CallbackContext context)
    {
        foreach (Image image in selectedMarkers)
        {
            Color color = image.color;
            color.a = 0;
            image.color = color;
        }
    }

    private void OnAnyKeyboard(InputAction.CallbackContext context)
    {
        if(EventSystem.current.currentSelectedGameObject == null) SetSelectedObject(tabTitle);
        foreach (Image image in selectedMarkers)
        {
            Color color = image.color;
            color.a = 1;
            image.color = color;
        }
    }

    private void OnAnyGamepad(InputAction.CallbackContext context)
    {
        if(EventSystem.current.currentSelectedGameObject == null) SetSelectedObject(tabTitle);
        foreach (Image image in selectedMarkers)
        {
            Color color = image.color;
            color.a = 1;
            image.color = color;
        }
    }

    private void ModPathOnSubmit(string arg0)
    {
        Debug.Log("SUBMIT");
        SaveModPath();
    }

    //Called from UIManager
    public void LoadSetting()
    {
        if(PlayerPrefs.HasKey(ConstantVar.PLAYERPERFKEY_MODPATH))
        {
            modPathField.text = PlayerPrefs.GetString(ConstantVar.PLAYERPERFKEY_MODPATH);
            SaveModPath();
        }

        if(PlayerPrefs.HasKey(ConstantVar.PLAYERPERFKEY_OPACITY))
        {
            Color fillColor = backgroundFillImage.color;
            fillColor.a = PlayerPrefs.GetFloat(ConstantVar.PLAYERPERFKEY_OPACITY);
            backgroundFillImage.color = fillColor;
            opacitySlider.value = fillColor.a;
        }
        else
        {
            Color fillColor = backgroundFillImage.color;
            fillColor.a = ConstantVar.DEFAULT_OPACITY;
            backgroundFillImage.color = fillColor;
            opacitySlider.value = fillColor.a;
            PlayerPrefs.SetFloat(ConstantVar.PLAYERPERFKEY_OPACITY, fillColor.a);
        }

        if(PlayerPrefs.HasKey(ConstantVar.PLAYERPERFKEY_SCALE))
        {
            float panelScale = PlayerPrefs.GetFloat(ConstantVar.PLAYERPERFKEY_SCALE);
            backgroundPanel.localScale = new Vector3(panelScale, panelScale, panelScale);
            scaleSlider.value = panelScale;
        }
        else
        {
            float panelScale = ConstantVar.DEFAULT_SCALE;
            backgroundPanel.localScale = new Vector3(panelScale, panelScale, panelScale);
            scaleSlider.value = panelScale;
            PlayerPrefs.SetFloat(ConstantVar.PLAYERPERFKEY_SCALE, panelScale);
        }
    }

    #region MODS PATH
    //Called from Button
    public void SelectModPath()
    {
        string[] folder = OpenFileExplorer.OpenFolder("Select Mods folder (ex: D:\\WWMI\\Mods)");
        if(folder.Length > 0)
        {
            modPathField.text = folder[0];
            SaveModPath();
        }
    }

    private void SaveModPath()
    {
        PlayerPrefs.SetString(ConstantVar.PLAYERPERFKEY_MODPATH, modPathField.text);
    }
    #endregion

    #region SLIDER SETTING
    //Called from Slider OnValueChanged
    public void ValueChangedOpacitySlider(float value)
    {
        Color fillColor = backgroundFillImage.color;
        fillColor.a = value;
        backgroundFillImage.color = fillColor;
        PlayerPrefs.SetFloat(ConstantVar.PLAYERPERFKEY_OPACITY, value);
    }
    public void ValueChangedScaleSlider(float value)
    {
        backgroundPanel.localScale = new Vector3(value, value, value);
        PlayerPrefs.SetFloat(ConstantVar.PLAYERPERFKEY_SCALE, value);
    }
    #endregion

    #region SUPPORT
    //Called from Button
    public void SupportButton()
    {
        Application.OpenURL(supportLink);
    }
    public void ProfileButton()
    {
        Application.OpenURL(ConstantVar.LINK_GAMEBANANA);
    }

    IEnumerator LoadSupportImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(ConstantVar.LINK_GETSUPPORTICON);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            supportImageIcon.sprite = sprite;
        }
        else
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
    }
    IEnumerator LoadSupportLink()
    {
        UnityWebRequest request = UnityWebRequest.Get(ConstantVar.LINK_GETSUPPORTLINK);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            supportLink = request.downloadHandler.text;
        }
        else
        {
            Debug.LogError("Failed to load link: " + request.error);
        }
    }
    #endregion
}
