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
        if(PlayerPrefs.HasKey(ConstantVar.PLAYERPERFKEY_MODPATH))
        {
            textInfo.SetActive(false);
            foreach (GameObject gameObject in objects)
            {
                gameObject.SetActive(true);
            }
        }
        else
        {
            textInfo.SetActive(true);
            foreach (GameObject gameObject in objects)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
