using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using UnityEngine;

public class UDButton : Button
{
    [SerializeField]
    ButtonDownEvent onDown = new ButtonDownEvent();

    [SerializeField]
    ButtonDownEvent onUp = new ButtonDownEvent();

    protected UDButton() { }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        OnDown?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        OnUp?.Invoke();
    }

    public ButtonDownEvent OnDown
    {
        get { return onDown; }
        set { onDown = value; }
    }

    public ButtonDownEvent OnUp
    {
        get { return onUp; }
        set { onUp = value; }
    }

    [Serializable]
    public class ButtonDownEvent : UnityEvent { }

    [Serializable]
    public class ButtonUpEvent : UnityEvent { }
}