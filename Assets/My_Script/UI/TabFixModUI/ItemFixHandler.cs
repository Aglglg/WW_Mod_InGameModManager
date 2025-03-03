using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemFixHandler : MonoBehaviour
{
    [HideInInspector] public ModFixData modFixData;
    [HideInInspector] public TabModFixManager tabModFixManager;
    [SerializeField] private TextMeshProUGUI noteText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject confirmation;
    [SerializeField] private GameObject fixButton;
    public void Start()
    {
        titleText.text = modFixData.title + $"\n<align=center>{modFixData.modifiedDate}\n</align>";
        noteText.text = "\n\n" + modFixData.note;
        noteText.alpha = 1;
    }

    public void ButtonFixShowConfirm()
    {
        noteText.text = "a\na\na";
        noteText.alpha = 0;
        confirmation.SetActive(true);
        fixButton.SetActive(false);
    }

    public void ButtonCancelConfirm()
    {
        noteText.text = "\n\n" + modFixData.note;
        noteText.alpha = 1;
        confirmation.SetActive(false);
        fixButton.SetActive(true);
    }
    public async void ButtonConfirmFix()
    {
        if(Directory.Exists(tabModFixManager.modPathField.text))
        {
            ButtonCancelConfirm();
            tabModFixManager.ShowLog();
            tabModFixManager.ToggleDoneButton(false);
            if(modFixData.modFixType == ModFixType.HashReplacement)
            {
                await Task.Delay(200);
                await ModFixer.FixModHashReplacementAsync(modFixData, tabModFixManager.modPathField.text);
            }
            else if(modFixData.modFixType == ModFixType.HashAddition)
            {
                await Task.Delay(200);
                await ModFixer.FixModHashAdditionAsync(modFixData, tabModFixManager.modPathField.text);
            }
            else
            {
                Debug.Log("UI--Mod fix type not recognized, try check for update");
            }
            tabModFixManager.ToggleDoneButton(true);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(tabModFixManager.modPathField.gameObject);
        }
    }
}
