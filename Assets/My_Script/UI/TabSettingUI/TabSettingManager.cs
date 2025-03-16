using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TabSettingManager : MonoBehaviour
{
    #region VARIABLES
    [Header("\n\nSETTINGS")]
    [SerializeField] private Transform backgroundPanel;
    [SerializeField] private Image backgroundFillImage;
    [SerializeField] private TMP_InputField modPathField;
    [SerializeField] private Slider opacitySlider;
    [SerializeField] private Slider scaleSlider;


    [Header("\n\nSUPPORT")]
    [SerializeField] private RawImage supportImageIcon;
    private string supportLink;
    #endregion
    
    private void Start()
    {
        
    }

    private void OnEnable()
    {
        StartCoroutine(LoadSupportImage());
        StartCoroutine(LoadSupportLink());

        modPathField.onSubmit.AddListener(ModPathOnSubmit);
        modPathField.onEndEdit.AddListener(ModPathOnSubmit);
    }
    private void OnDisable()
    {
        modPathField.onSubmit.RemoveListener(ModPathOnSubmit);
        modPathField.onEndEdit.RemoveListener(ModPathOnSubmit);
    }

    private void ModPathOnSubmit(string arg0)
    {
        Debug.Log("SUBMIT");
        SaveModPath();
    }

    //Called from UIManager
    public void LoadSetting()
    {
        if(PlayerPrefs.HasKey(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName))
        {
            modPathField.text = PlayerPrefs.GetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName);
            SaveModPath();
        }

        if(PlayerPrefs.HasKey(ConstantVar.PlayerPrefKey_Opacity))
        {
            Color fillColor = backgroundFillImage.color;
            fillColor.a = PlayerPrefs.GetFloat(ConstantVar.PlayerPrefKey_Opacity);
            backgroundFillImage.color = fillColor;
            opacitySlider.value = fillColor.a;
        }
        else
        {
            Color fillColor = backgroundFillImage.color;
            fillColor.a = ConstantVar.Default_Opacity;
            backgroundFillImage.color = fillColor;
            opacitySlider.value = fillColor.a;
            PlayerPrefs.SetFloat(ConstantVar.PlayerPrefKey_Opacity, fillColor.a);
        }

        if(PlayerPrefs.HasKey(ConstantVar.PlayerPrefKey_Scale))
        {
            float panelScale = PlayerPrefs.GetFloat(ConstantVar.PlayerPrefKey_Scale);
            backgroundPanel.localScale = new Vector3(panelScale, panelScale, panelScale);
            scaleSlider.value = panelScale;
        }
        else
        {
            float panelScale = ConstantVar.Default_Scale;
            backgroundPanel.localScale = new Vector3(panelScale, panelScale, panelScale);
            scaleSlider.value = panelScale;
            PlayerPrefs.SetFloat(ConstantVar.PlayerPrefKey_Scale, panelScale);
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
        PlayerPrefs.SetString(ConstantVar.Prefix_PlayerPrefKey_ModPath + Initialization.gameName, modPathField.text);
    }
    #endregion

    #region SLIDER SETTING
    //Called from Slider OnValueChanged
    public void ValueChangedOpacitySlider(float value)
    {
        Color fillColor = backgroundFillImage.color;
        fillColor.a = value;
        backgroundFillImage.color = fillColor;
        PlayerPrefs.SetFloat(ConstantVar.PlayerPrefKey_Opacity, value);
    }
    public void ValueChangedScaleSlider(float value)
    {
        backgroundPanel.localScale = new Vector3(value, value, value);
        PlayerPrefs.SetFloat(ConstantVar.PlayerPrefKey_Scale, value);
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
        Application.OpenURL(ConstantVar.Link_GameBanana);
    }

    private IEnumerator LoadSupportImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(ConstantVar.Link_GetSupportIcon);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            supportImageIcon.texture = texture;
        }
        else
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
    }
    IEnumerator LoadSupportLink()
    {
        UnityWebRequest request = UnityWebRequest.Get(ConstantVar.Link_GetSupportLink);
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

    #region Called from PlayerInput
    //Called from PlayerInput in PanelRoot&Background GameObject
    public void OnResetPanelTransform(InputAction.CallbackContext context)
    {
        //Make sure it's only when performed not started/ended(on hold 1 sec only)
        if(context.phase != InputActionPhase.Performed) return;
        Debug.Log("RESET SCALE & POS");
        float panelScale = ConstantVar.Default_Scale;
        scaleSlider.value = panelScale;
        

        backgroundPanel.localPosition = Vector3.zero;
    }
    #endregion
}
