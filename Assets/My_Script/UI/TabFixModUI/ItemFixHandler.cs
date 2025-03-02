using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemFixHandler : MonoBehaviour
{
    public ModFixData modFixData;
    public TabModFixManager modFixManager;
    [SerializeField] private TextMeshProUGUI textFixInfo;

    //Called from TabModFixManager when instantiated
    public void LoadModFixData()
    {
        textFixInfo.text = $"<align=center>{modFixData.modifiedDate}\n</align>" + modFixData.note;
    }

    //Called from child text if user click FIX
    public void FixMod()
    {
        if(Directory.Exists(modFixManager.modPathField.text))
        {
            //Call fix tool
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(modFixManager.modPathField.gameObject);
        }
    }
}
