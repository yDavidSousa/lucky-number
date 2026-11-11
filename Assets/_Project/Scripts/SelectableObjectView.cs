using System;
using UnityEngine;

public sealed class SelectableObjectView : MonoBehaviour
{
    [Header("Hover")]
    [SerializeField] private float hoverHeight = 0.3f;
    [SerializeField] private float hoverSpeed = 8f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private GameObject outlineObject;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Quaternion initialRotation;

    public bool isHovered;
    public event Action<SelectableObjectView> Clicked;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        initialScale = transform.localScale;
        outlineObject.gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector3 targetPosition = initialPosition;
        Vector3 targetScale = initialScale;

        if (isHovered)
        {
            float bob = Mathf.Sin(Time.time * 3f) * 0.05f;
            targetPosition += Vector3.up * (hoverHeight + bob);
            targetScale = initialScale * 1.2f;
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * hoverSpeed
        );

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * hoverSpeed
        );

        if (isHovered)
        {
            transform.Rotate(
                Vector3.up,
                rotationSpeed * Time.deltaTime,
                Space.Self
            );
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                initialRotation,
                Time.deltaTime * hoverSpeed
            );
        }
    }

    public void SetHovered(bool hovered)
    {
        if (isHovered == hovered)
            return;

        isHovered = hovered;

        if (outlineObject != null)
            outlineObject.SetActive(hovered);
    }

    public void OnSelected()
    {
        Clicked?.Invoke(this);
    }
}