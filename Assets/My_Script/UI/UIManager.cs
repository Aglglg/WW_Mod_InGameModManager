using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static TabState CurrentTabState = TabState.Mod;
    [SerializeField] private RectTransform selectedTab;
    [SerializeField] private float[] selectedTabPositions;
    [SerializeField] private float animationDuration;
    [SerializeField] private GameObject[] tabContents;

    private void Start()
    {
        ChangeTab((int)TabState.Mod);
    }

    //Called from tab buttons
    public void ChangeTab(int tabState)
    {
        if(CurrentTabState == (TabState)tabState) return;
        TabState prevTabState = CurrentTabState;
        CurrentTabState = (TabState)tabState;

        selectedTab.DOLocalMoveX(selectedTabPositions[(int)CurrentTabState], animationDuration);
        tabContents[(int)prevTabState].GetComponent<CanvasGroup>().DOFade(0f, animationDuration/2)
        .OnComplete
        (() =>
            {
                tabContents[(int)prevTabState].SetActive(false);
                tabContents[(int)CurrentTabState].GetComponent<CanvasGroup>().alpha = 0;
                tabContents[(int)CurrentTabState].SetActive(true);
                tabContents[(int)CurrentTabState].GetComponent<CanvasGroup>().DOFade(1f, animationDuration/2);
            }
        );
    }

    //Called InputSystem on this GameObject
    public void OnTabNavigate(InputValue value)
    {
        ChangeTab(Mathf.Clamp((int)CurrentTabState + Mathf.RoundToInt(value.Get<float>()), 0, Enum.GetValues(typeof(TabState)).Length - 1));// - 1, index start with 0, length 4 (0 - 3)
    }

    #region MOD Tab
    private int selectedModSlot = 0;
        #region ModSelection
        private void SelectMod(int modSlot)
        {
            selectedModSlot = modSlot;
        }
        #endregion
    #endregion


    #region KEYBIND Tab
    [Header("KEYBIND Tab")]
    [SerializeField] private Button validKeyButton;
    private const string LINK_VALIDKEYS = "https://forums.frontier.co.uk/attachments/edhm-hotkeys-pdf.343006/";

    //Called from context menu
    public void ShowKeybind(string mainFolder)
    {
        string[] iniFiles = FindIniFiles.FindIniFilesRecursive(mainFolder);
    }

    public void GoToLinkValidKey()
    {
        Application.OpenURL(LINK_VALIDKEYS);
    }
    #endregion
}