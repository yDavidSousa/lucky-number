using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InteractionSystem
{
    private readonly Camera _camera;
    private readonly LayerMask _layerMask;

    public SelectableObjectView Hovered { get; private set; }
    public SelectableObjectView Clicked { get; private set; }

    public InteractionSystem(Camera camera, LayerMask layerMask)
    {
        _camera = camera;
        _layerMask = layerMask;
    }

    public void Tick()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        Hovered = null;
        Clicked = null;

        Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, _layerMask))
            return;

        Hovered = hit.collider.GetComponentInParent<SelectableObjectView>();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            Clicked = Hovered;

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // if (pressedObject != null && pressedObject == currentHovered)
            //     pressedObject.OnSelected();
            //
            // pressedObject = null;
        }
    }
}