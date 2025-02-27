using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static TabState TabState = TabState.Mod;
    [SerializeField] private RectTransform selectedTab;
    [SerializeField] private float[] selectedTabPositions;
    [SerializeField] private float animationDuration;

    [SerializeField] private GameObject[] tabContents;

    private void Start()
    {
        ChangeTab(1);
    }

    public void ChangeTab(int tabState)
    {
        Debug.Log(tabState);
        TabState = (TabState)tabState;
        selectedTab.DOLocalMoveX(selectedTabPositions[tabState], animationDuration);
        foreach (GameObject tabContent in tabContents)
        {
            tabContent.SetActive(false);
        }
        tabContents[tabState].SetActive(true);
    }
}
