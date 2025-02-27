using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContentObjectController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public ScrollviewSnapZoom scrollviewSnapZoom;

    public void OnDeselect(BaseEventData eventData)
    {
        StartCoroutine(CheckDeselect());
    }

    IEnumerator CheckDeselect()
    {
        yield return null;
        if(EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(Input.GetMouseButtonDown(0)) return;
        scrollviewSnapZoom.ScrollToItem(gameObject);
    }
}
