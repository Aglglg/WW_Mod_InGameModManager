using System.IO;
using UnityEngine;

public class TabModManager : MonoBehaviour
{
    [SerializeField] private GameObject groupItem;
    [SerializeField] private Transform groupRoot;
    [SerializeField] private Transform modsRoot;
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
        string key = ConstantVar.SUFFIX_PLAYERPREFKEY_MODPATH + Initialization.gameName;
        if(PlayerPrefs.HasKey(key)
            && Directory.Exists(PlayerPrefs.GetString(key))
            && PlayerPrefs.GetString(key).TrimEnd('\\').ToLower().EndsWith(ConstantVar.MODFOLDER_VALIDSUFFIX)
               )
        {
            ValidModPath();
            ModsManager.InitializeMods();
            InstantiateMods();
        }
        else
        {
            InvalidModPath();
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

    private void InstantiateMods()
    {
        foreach (var item in ModsManager.managedModDatas.groupDatas)
        {
            GameObject instantiatedGroup = Instantiate(groupItem, groupRoot);
            instantiatedGroup.GetComponent<GroupItemHandler>().groupPath = item.groupPath;
        }
    }
}
