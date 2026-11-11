using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSelection : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask selectableLayer;
    [SerializeField] private float maxDistance = 100f;

    private SelectableObjectView pressedObject;
    private SelectableObjectView currentHovered;

    private void Update()
    {
        UpdateHover();
        HandleClick();
    }

    private void UpdateHover()
    {
        if (Mouse.current == null)
            return;

        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        SelectableObjectView hovered = null;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, selectableLayer))
        {
            hovered = hit.collider.GetComponentInParent<SelectableObjectView>();
        }

        if (currentHovered == hovered)
            return;

        if (currentHovered != null)
            currentHovered.SetHovered(false);

        currentHovered = hovered;

        if (currentHovered != null)
            currentHovered.SetHovered(true);
    }

    private void HandleClick()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            pressedObject = currentHovered;

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (pressedObject != null && pressedObject == currentHovered)
                pressedObject.OnSelected();

            pressedObject = null;
        }
    }
}