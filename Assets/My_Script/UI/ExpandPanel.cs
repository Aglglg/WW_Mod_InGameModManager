using UnityEngine;
using UnityEngine.EventSystems;

public class ExpandPanel : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform rootTransform;
    [SerializeField] private bool isButtonRight;

    private static readonly Vector2 pivotIfExpandFromRight = Vector2.up;
    private static readonly Vector2 pivotIfExpandFromLeft = Vector2.one;
    private static readonly Vector2 pivotIfNotExpanding = new Vector2(0.5f, 0.5f);

    private Vector2 initialMousePos;
    private Vector2 initialSize;

    private void Awake()
    {
        if(PlayerPrefs.HasKey(ConstantVar.PLAYERPERFKEY_HEIGHT) && PlayerPrefs.HasKey(ConstantVar.PLAYERPERFKEY_WIDTH))
        {
            rootTransform.sizeDelta = new Vector2(PlayerPrefs.GetFloat(ConstantVar.PLAYERPERFKEY_WIDTH), PlayerPrefs.GetFloat(ConstantVar.PLAYERPERFKEY_HEIGHT));
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootTransform, eventData.position, eventData.pressEventCamera, out localMousePos);

        float widthChange;
        float heightChange = initialMousePos.y - localMousePos.y;

        if(isButtonRight)
        {
            widthChange = localMousePos.x - initialMousePos.x;
        }
        else
        {
            widthChange = initialMousePos.x - localMousePos.x;
        }

        rootTransform.sizeDelta = new Vector2(Mathf.Max(initialSize.x + widthChange, ConstantVar.DEFAULT_WIDTH), Mathf.Max(initialSize.y + heightChange, ConstantVar.DEFAULT_HEIGHT));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 pivot = isButtonRight ? pivotIfExpandFromRight : pivotIfExpandFromLeft;
        ChangePivot(pivot);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootTransform, eventData.position, eventData.pressEventCamera, out initialMousePos);
        initialSize = new Vector2(rootTransform.sizeDelta.x, rootTransform.sizeDelta.y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ChangePivot(pivotIfNotExpanding);
        initialMousePos = Vector2.zero;

        PlayerPrefs.SetFloat(ConstantVar.PLAYERPERFKEY_WIDTH, rootTransform.sizeDelta.x);
        PlayerPrefs.SetFloat(ConstantVar.PLAYERPERFKEY_HEIGHT, rootTransform.sizeDelta.y);
    }

    private void ChangePivot(Vector2 pivot)
    {
        // Calculate the position difference
        Vector2 size = rootTransform.rect.size;
        Vector2 deltaPivot = pivot - rootTransform.pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x * rootTransform.localScale.x, deltaPivot.y * size.y * rootTransform.localScale.y);

        // Apply the new pivot
        rootTransform.pivot = pivot;

        // Adjust the position to compensate
        rootTransform.anchoredPosition += (Vector2)deltaPosition;
    }
}
