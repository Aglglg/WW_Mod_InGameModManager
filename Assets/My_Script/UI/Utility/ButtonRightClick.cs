using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class ButtonRightClick : Button
{
    //assigned from ModScrollHandler, if any
    public ModScrollHandler modScrollHandler;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.button = PointerEventData.InputButton.Left;
        }
        else if(eventData.button == PointerEventData.InputButton.Left)
        {
            eventData.button = PointerEventData.InputButton.Right;
        }
        base.OnPointerDown(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.button = PointerEventData.InputButton.Left;
        }
        else if(eventData.button == PointerEventData.InputButton.Left)
        {
            eventData.button = PointerEventData.InputButton.Right;
        }
        base.OnPointerUp(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        modScrollHandler?.GoToSelectedMod(transform);
        base.OnPointerClick(eventData);
    }
}
