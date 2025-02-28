using System;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public static TabState CurrentTabState = TabState.Mod;
    [SerializeField] private TabSettingManager tabSettingManager;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private RectTransform selectedTab;
    [SerializeField] private float[] selectedTabPositions;
    [SerializeField] private float animationDuration;
    [SerializeField] private GameObject[] tabContents;
    [SerializeField] private GameObject[] buttonKeyIconsGamepad;
    [SerializeField] private GameObject[] buttonKeyIconsKeyboard;

    private Vector2 initialMousePos;
    private Vector2 initialPanelPos;

    private void Start()
    {
        tabSettingManager.LoadSetting();
        ChangeTab((int)TabState.Mod);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos = eventData.position - (initialMousePos - initialPanelPos);
        GetComponent<RectTransform>().position = pos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        initialMousePos = eventData.position;
        initialPanelPos = GetComponent<RectTransform>().position;
    }

    public void OnControlChanged()
    {
        if(playerInput.currentControlScheme == "Keyboard")
        {
            foreach (var item in buttonKeyIconsGamepad)
            {
                item.SetActive(false);
            }
            foreach (var item in buttonKeyIconsKeyboard)
            {
                item.SetActive(true);
            }
        }
        else if(playerInput.currentControlScheme == "Gamepad")
        {
            foreach (var item in buttonKeyIconsGamepad)
            {
                item.SetActive(true);
            }
            foreach (var item in buttonKeyIconsKeyboard)
            {
                item.SetActive(false);
            }
        }
    }

    //Called from tab buttons, and other scripts
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

    //Called from PlayerInput component on this GameObject
    public void OnTabNavigate(InputAction.CallbackContext context)
    {
        if(context.phase != InputActionPhase.Performed) return;
        ChangeTab(Mathf.Clamp((int)CurrentTabState + Mathf.RoundToInt(context.ReadValue<float>()), 0, Enum.GetValues(typeof(TabState)).Length - 1));// - 1, index start with 0, length 4 (0 - 3)
    }

    #region KEYBIND Tab

    //Called from context menu
    public void ShowKeybind(string mainFolder)
    {
        string[] iniFiles = FindIniFiles.FindIniFilesRecursive(mainFolder);
    }

    //Called from button
    public void GoToLinkValidKey()
    {
        Application.OpenURL(ConstantVar.LINK_VALIDKEYS);
    }

    #endregion
}