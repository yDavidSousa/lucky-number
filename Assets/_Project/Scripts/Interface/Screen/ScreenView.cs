using UnityEngine;

public sealed class ScreenView : MonoBehaviour
{
    public CanvasGroup CanvasGroup;

    [Header("Animation")]
    public float Duration = 0.25f;
    public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}