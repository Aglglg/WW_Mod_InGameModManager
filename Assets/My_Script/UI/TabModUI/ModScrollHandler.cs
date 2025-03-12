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
    
    [SerializeField] private Sprite modDefaultIcon;
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
    public async void UpdateModItems(int selectedGroupIndex, GroupData selectedGroupData)
    {
        bool isAddButtonGroup = selectedGroupIndex == 0;

        SetModItemsActive(!isAddButtonGroup); //If add button, disable
        if(!isAddButtonGroup)
        {
            SetModTitle(selectedGroupData);
            await SetModIconAsync(selectedGroupData);
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
    private async Task SetModIconAsync(GroupData selectedGroupData)
    {
        //First, reset all
        for (int i = 0; i < contentModTransform.childCount; i++)
        {
            if(i == 0) continue; //None Button
            Transform modTransform = contentModTransform.GetChild(i);
            Image imageIcon = modTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<Image>();
            imageIcon.gameObject.SetActive(false);
        }

        //TODO: Cause lag, need to be fixed-------Fixed, instead of texture.LoadImage, now use unitywebreq, BUT still a bit lag
        //SFB plugin also have example of loading image using unitywebreq, learn from it
        for (int i = 0; i < selectedGroupData.modPaths.Count; i++)
        {
            if(i == 0) continue; //None Button
            string iconPath = Path.Combine(selectedGroupData.modPaths[i], ConstantVar.MODDATA_ICON_FILE);
            Transform modTransform = contentModTransform.GetChild(i);
            Image imageIcon = modTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<Image>();
            imageIcon.gameObject.SetActive(true);
            if(File.Exists(iconPath))
            {
                Texture2D texture = await LoadTextureAsync(iconPath);
                if (texture != null)
                {
                    Texture2D croppedTexture = CropToAspectRatio(texture, 2, 3);
                    Sprite sprite = Sprite.Create(croppedTexture, new Rect(0, 0, croppedTexture.width, croppedTexture.height), new Vector2(0.5f, 0.5f));
                    imageIcon.sprite = sprite;
                }
                else
                {
                    imageIcon.sprite = modDefaultIcon;
                }
            }
            else
            {
                imageIcon.sprite = modDefaultIcon;
            }
        }
    }

    private void ScaleModToSelected(int modIndex)
    {
        contentModTransform.GetChild(modIndex).DOScale(new Vector3(selectedScale, selectedScale, selectedScale), animationDuration);
    }

    private void ScaleModToDefault(int modIndex)
    {
        contentModTransform.GetChild(modIndex).DOScale(new Vector3(notSelectedScale, notSelectedScale, notSelectedScale), animationDuration);
    }


    private async Task<Texture2D> LoadTextureAsync(string path)
    {
        try
        {
            // Convert the file path to a URI
            string fileUri = "file://" + path;

            // Use UnityWebRequestTexture to load the texture asynchronously
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(fileUri))
            {
                // Send the request and wait for it to complete
                var asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    await Task.Yield(); // Wait without blocking the main thread
                }

                // Check for errors
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Failed to load texture: {request.error}");
                    return null;
                }

                // Get the downloaded texture
                return DownloadHandlerTexture.GetContent(request);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load texture: {ex.Message}");
            return null;
        }
    }
    private Texture2D CropToAspectRatio(Texture2D texture, int widthRatio, int heightRatio)
    {
        float targetAspect = (float)widthRatio / heightRatio; // Desired aspect ratio (2:3)
        float originalAspect = (float)texture.width / texture.height; // Original aspect ratio

        int newWidth, newHeight;
        int x = 0, y = 0;

        if (originalAspect > targetAspect)
        {
            // Image is wider than the target aspect ratio
            newHeight = texture.height;
            newWidth = Mathf.RoundToInt(newHeight * targetAspect);
            x = (texture.width - newWidth) / 2; // Center the crop horizontally
        }
        else
        {
            // Image is taller than the target aspect ratio
            newWidth = texture.width;
            newHeight = Mathf.RoundToInt(newWidth / targetAspect);
            y = (texture.height - newHeight) / 2; // Center the crop vertically
        }

        // Create a new Texture2D for the cropped image
        Texture2D croppedTexture = new Texture2D(newWidth, newHeight);

        // Get the pixels from the original texture within the cropping area
        Color[] pixels = texture.GetPixels(x, y, newWidth, newHeight);

        // Set the pixels to the new cropped texture
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply(); // Apply the changes

        return croppedTexture;
    }
}