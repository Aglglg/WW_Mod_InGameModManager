using System.IO;
using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GroupScrollHandler : MonoBehaviour
{
    private const int TitleTextChildIndex = 0;
    private const int ImageMaskIconChildIndex = 1;
    private const int ImageIconChildIndexInMaskTransform = 0;

    [SerializeField] private Sprite defaultIcon;
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

    private void SetGroupTitle(int groupIndex)
    {
        Transform groupTransform = contentGroupTransform.GetChild(groupIndex);
        TMP_InputField titleInputField = groupTransform.GetChild(TitleTextChildIndex).GetComponent<TMP_InputField>();
        titleInputField.text = Path.GetFileName(TabModManager.modData.groupDatas[groupIndex].groupPath);
    }

    private void SetGroupIcon(int groupIndex)
    {
        string iconPath = Path.Combine(TabModManager.modData.groupDatas[groupIndex].groupPath, ConstantVar.MODDATA_ICON_FILE);
        if(File.Exists(iconPath))
        {
            Transform groupTransform = contentGroupTransform.GetChild(groupIndex);
            Image imageIcon = groupTransform.GetChild(ImageMaskIconChildIndex).GetChild(ImageIconChildIndexInMaskTransform).GetComponent<Image>();

            byte[] fileData = File.ReadAllBytes(iconPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            Texture2D croppedTexture = CropToAspectRatio(texture, 1, 1);
            Sprite sprite = Sprite.Create(croppedTexture, new Rect(0, 0, croppedTexture.width, croppedTexture.height), new Vector2(0.5f, 0.5f));
            imageIcon.sprite = sprite;
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