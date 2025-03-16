using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LinkTextHandler : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textMeshProUGUI;
    private TabModFixManager tabModFixManager;

    //Only for Mod Fix, called when instantiating Item Fix
    [HideInInspector] public ModFixData modFixData;

    private void Start()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        tabModFixManager = GameObject.FindGameObjectWithTag(ConstantVar.Tag_ModFixManager).GetComponent<TabModFixManager>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshProUGUI, new Vector3(eventData.position.x, eventData.position.y, 0f), null);
        if(linkIndex == -1) return;

        TMP_LinkInfo linkInfo = textMeshProUGUI.textInfo.linkInfo[linkIndex];

        Application.OpenURL(linkInfo.GetLink());
    }
}
