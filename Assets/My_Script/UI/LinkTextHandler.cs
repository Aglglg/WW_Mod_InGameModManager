using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LinkTextHandler : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textMeshProUGUI;
    private int selectedLinkIndex;
    private void Start()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshProUGUI, new Vector3(eventData.position.x, eventData.position.y, 0f), null);
        if(linkIndex == -1) return;
        TMP_LinkInfo linkInfo = textMeshProUGUI.textInfo.linkInfo[linkIndex];
        Application.OpenURL(linkInfo.GetLinkID());
    }
}
