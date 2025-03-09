using System.IO;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GroupItemHandler : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public string groupPath;
    [HideInInspector] public int groupIndex;
    [SerializeField] private Image groupIcon;
    [SerializeField] private TMP_InputField titleGroup;
    [SerializeField] private GameObject contextMenu;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            contextMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(contextMenu);
            groupIcon.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        titleGroup.text = Path.GetFileName(groupPath);
        RefreshIcon();
    }

    private void RefreshIcon()
    {
        string modIconPath = Path.Combine(groupPath, ConstantVar.MANAGED_ICON_FILE);
        if(File.Exists(modIconPath))
        {
            byte[] imageBytes = File.ReadAllBytes(modIconPath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageBytes))
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                groupIcon.sprite = sprite;
                Color imageColor = Color.white;
                imageColor.a = 1;
                groupIcon.color = imageColor;
                return;
            }
        }

        groupIcon.sprite = null;
        Color color = Color.white;
        color.a = 0;
        groupIcon.color = color;
    }

    public void ButtonRename()
    {
        titleGroup.interactable = true;
        EventSystem.current.SetSelectedGameObject(titleGroup.gameObject);
    }
    public void OnEndEditGroupName()
    {
        if(!string.IsNullOrWhiteSpace(titleGroup.text.Trim()))
        {
            try
            {
                Directory.Move(groupPath, Path.Combine(Path.GetDirectoryName(groupPath), titleGroup.text.Trim()));
                groupPath = Path.Combine(Path.GetDirectoryName(groupPath), titleGroup.text.Trim());
                titleGroup.interactable = false;
                return;
            }
            catch
            {

            }
        }

        Debug.Log("Cannot rename");
        titleGroup.text = Path.GetFileName(groupPath);
        titleGroup.interactable = false;
    }

    public void ButtonChangeIcon()
    {
        var extensions = new []
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" )
        };

        string[] iconPath = OpenFileExplorer.OpenFile("Select image (1:1) to be Group Icon", extensions);
        if(iconPath.Length > 0)
        {
            try
            {
                File.Copy(iconPath[0], Path.Combine(groupPath, ConstantVar.MANAGED_ICON_FILE), true);
                RefreshIcon();
            }
            catch
            {

            }
        }
    }

    public void ButtonRemove()
    {
        string removedPath = Path.Combine(PlayerPrefs.GetString(ConstantVar.SUFFIX_PLAYERPREFKEY_MODPATH + Initialization.gameName), ConstantVar.MANAGED_REMOVED_PATH);
        if(!Directory.Exists(removedPath))
        {
            Directory.CreateDirectory(removedPath);
        }
        try
        {
            Directory.Move(groupPath, Path.Combine(removedPath, Path.GetFileName(groupPath)));
        }
        catch
        {

        }
    }
}