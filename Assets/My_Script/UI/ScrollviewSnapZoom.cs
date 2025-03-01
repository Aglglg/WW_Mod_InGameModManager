using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.InputSystem;
using DG.Tweening;

public class ScrollviewSnapZoom : MonoBehaviour
{
    [SerializeField] private InputAction mouseClickAction;
    [SerializeField] private Scrollbar scrollBar;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] private RectTransform sampleContent;
    [SerializeField] private RectTransform viewportRectTransform;
    [SerializeField] private float scaleNotSelected;
    [SerializeField] private float scaleSelected;
    private float scrollPos = 0;
    private float[] pos;

    void Start()
    {
        mouseClickAction.Enable();
    }

    void Update()
    {
        //Update padding to automatically make it on center when scaled
        horizontalLayoutGroup.padding.left = Mathf.RoundToInt((viewportRectTransform.rect.width/2) - (sampleContent.rect.width/2));
        horizontalLayoutGroup.padding.right = Mathf.RoundToInt((viewportRectTransform.rect.width/2) - (sampleContent.rect.width/2));

        int itemCount = transform.childCount;
        pos = new float[itemCount];
        float distance = 1f / (itemCount - 1f);

        for (int i = 0; i < itemCount; i++)
        {
            pos[i] = distance * i;
        }
        
        if (mouseClickAction.IsInProgress())
        {
            scrollPos = scrollBar.value;
        }
        else
        {
            for (int i = 0; i < itemCount; i++)
            {
                if (scrollPos < pos[i] + (distance / 2) && scrollPos > pos[i] - (distance / 2))
                {
                    scrollBar.value = Mathf.Lerp(scrollBar.value, pos[i], 0.1f);
                }
            }
        }

        for (int i = 0; i < itemCount; i++)
        {
            if (scrollPos < pos[i] + (distance / 2) && scrollPos > pos[i] - (distance / 2))
            {
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(scaleSelected, scaleSelected), 0.1f);
                for (int j = 0; j < itemCount; j++)
                {
                    if (j != i)
                    {
                        transform.GetChild(j).localScale = Vector2.Lerp(transform.GetChild(j).localScale, new Vector2(scaleNotSelected, scaleNotSelected), 0.1f);
                    }
                }
            }
        }
    }

    //Called from when UI selected
    public void ScrollToItem(GameObject item)
    {
        int index = item.transform.GetSiblingIndex();
        float distance = 1f / (pos.Length - 1f);
        scrollPos = pos[index];

        float duration = 0.3f;
        float startPos = scrollBar.value;

        DOTween.To(() => scrollBar.value, x => scrollBar.value = x, scrollPos, duration);
    }
}
