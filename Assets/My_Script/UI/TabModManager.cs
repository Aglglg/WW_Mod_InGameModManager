using System.IO;
using UnityEngine;

public class TabModManager : MonoBehaviour
{
    [SerializeField] private UIManager uIManager;
    [SerializeField] private GameObject textInfo;
    [SerializeField] private GameObject[] objects;
    private int selectedModSlot = 0;

    //Called from button
    public void GoToSetting()
    {
        uIManager.ChangeTab((int)TabState.Setting);
    }
    private void SelectMod(int modSlot)
    {
        selectedModSlot = modSlot;
    }

    private void OnEnable()
    {
        InitializeTabMod();
    }

    private void InitializeTabMod()
    {
        if(PlayerPrefs.HasKey(ConstantVar.PLAYERPERFKEY_MODPATH)
            && Directory.Exists(PlayerPrefs.GetString(ConstantVar.PLAYERPERFKEY_MODPATH))
            && (PlayerPrefs.GetString(ConstantVar.PLAYERPERFKEY_MODPATH).ToLower().EndsWith(ConstantVar.MODFOLDER_VALIDSUFFIX1)
               || PlayerPrefs.GetString(ConstantVar.PLAYERPERFKEY_MODPATH).ToLower().EndsWith(ConstantVar.MODFOLDER_VALIDSUFFIX2)))
        {
            ValidModPath();
            Debug.Log("VALID: " + PlayerPrefs.GetString(ConstantVar.PLAYERPERFKEY_MODPATH));
            Debug.Log(Directory.Exists(PlayerPrefs.GetString(ConstantVar.PLAYERPERFKEY_MODPATH)));
        }
        else
        {
            InvalidModPath();
            Debug.Log("INVALID");
        }
    }
    private void ValidModPath()
    {
        textInfo.SetActive(false);
        foreach (GameObject gameObject in objects)
        {
            gameObject.SetActive(true);
        }
    }
    private void InvalidModPath()
    {
        textInfo.SetActive(true);
        foreach (GameObject gameObject in objects)
        {
            gameObject.SetActive(false);
        }
    }
}
