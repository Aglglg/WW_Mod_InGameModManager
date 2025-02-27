using UnityEngine;
using UnityEngine.UI;

public class ScrollviewInfiniteScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup horizontalLayoutGroup;
    public RectTransform[] itemList;

    private Vector2 oldVelocity;
    private bool isUpdated;
    void Start()
    {
        isUpdated = false;
        oldVelocity = Vector2.zero;

        int itemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.width / (itemList[0].rect.width + horizontalLayoutGroup.spacing));
        for (int i = 0; i < itemsToAdd; i++)
        {
            RectTransform RT = Instantiate(itemList[i%itemList.Length], contentPanelTransform);
            RT.SetAsLastSibling();
        }
        for (int i = 0; i < itemsToAdd; i++)
        {
            int num = itemList.Length - i - 1;
            while(num < 0)
            {
                num += itemList.Length;
            }
            RectTransform RT = Instantiate(itemList[num], contentPanelTransform);
            RT.SetAsFirstSibling();
        }
    }

    void Update()
    {
        if(isUpdated)
        {
            isUpdated = false;
            scrollRect.velocity = oldVelocity;
        }

        if(contentPanelTransform.localPosition.x + horizontalLayoutGroup.padding.left > 0)
        {
            Canvas.ForceUpdateCanvases();
            oldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition -= new Vector3(itemList.Length*(itemList[0].rect.width+horizontalLayoutGroup.spacing),0,0);
            isUpdated = true;
        }

        if(contentPanelTransform.localPosition.x + horizontalLayoutGroup.padding.left < 0 - (itemList.Length * (itemList[0].rect.width+horizontalLayoutGroup.spacing)))
        {
            Canvas.ForceUpdateCanvases();
            oldVelocity = scrollRect.velocity;
            contentPanelTransform.localPosition += new Vector3(itemList.Length * (itemList[0].rect.width+horizontalLayoutGroup.spacing),0,0);
            isUpdated = true;
        }
    }
}
