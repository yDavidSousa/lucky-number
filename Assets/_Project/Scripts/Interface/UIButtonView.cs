using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum UIButtonState : byte
{
    Normal,
    Hovered,
    Pressed,
    Disabled
}

[Serializable]
public struct UIButtonStyle
{
    public Vector3 NormalScale;
    public Vector3 HoveredScale;
    public Vector3 PressedScale;
    public Vector3 DisabledScale;

    public float TransitionSpeed;
}

public struct UIButtonPresentation
{
    public UIButtonState State;

    public bool Interactable;
}

public class UIButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private RectTransform target;

    [SerializeField] private UIButtonStyle style = new()
    {
        NormalScale = Vector3.one,
        HoveredScale = Vector3.one,
        PressedScale = Vector3.one,
        DisabledScale = Vector3.one,
        TransitionSpeed = 12f
    };

    private UIButtonPresentation _presentation;

    private bool _hovered;
    private bool _pressed;

    public event Action<UIButtonView> Clicked;
    public UIButtonState State => _presentation.State;

    public bool Interactable
    {
        set
        {
            _presentation.Interactable = value;
            RefreshState();
        }
    }

    private void Reset()
    {
        target = (RectTransform)transform;
    }

    private void Awake()
    {
        if (target == null)
        {
            target = (RectTransform)transform;
        }

        _presentation.Interactable = true;
        RefreshState();
    }

    private void Update()
    {
        UpdateVisuals(Time.deltaTime);
    }

    public void SetInteractable(bool value)
    {
        Interactable = value;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;

        RefreshState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        _pressed = false;

        RefreshState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_presentation.Interactable) return;

        _pressed = true;

        RefreshState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;

        RefreshState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_presentation.Interactable) return;

        Clicked?.Invoke(this);
    }

    private void RefreshState()
    {
        if (!_presentation.Interactable)
        {
            _presentation.State = UIButtonState.Disabled;
            return;
        }

        if (_pressed)
        {
            _presentation.State = UIButtonState.Pressed;
            return;
        }

        if (_hovered)
        {
            _presentation.State = UIButtonState.Hovered;
            return;
        }

        _presentation.State = UIButtonState.Normal;
    }

    private void UpdateVisuals(float dt)
    {
        Vector3 targetScale = GetTargetScale(_presentation.State);
        target.localScale = Vector3.Lerp(target.localScale, targetScale, dt * style.TransitionSpeed);
    }

    private Vector3 GetTargetScale(UIButtonState state)
    {
        switch (state)
        {
            case UIButtonState.Hovered:
                return style.HoveredScale;
            case UIButtonState.Pressed:
                return style.PressedScale;
            case UIButtonState.Disabled:
                return style.DisabledScale;
            default:
                return style.NormalScale;
        }
    }
}