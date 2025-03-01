using TMPro;
using UnityEngine;

public class ItemFixHandler : MonoBehaviour
{
    public ModFixData modFixData;
    [SerializeField] private TextMeshProUGUI textFixInfo;
    public void LoadModFixData()
    {
        textFixInfo.text = modFixData.note;
    }
    public void FixMod()
    {
        Debug.Log("FIX MOD");
    }
}
