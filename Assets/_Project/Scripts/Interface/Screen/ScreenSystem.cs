using System;
using UnityEngine;

public enum ScreenPhase : byte
{
    Hidden,
    Opening,
    Visible,
    Closing
}

public struct ScreenData
{
    public ScreenView view;
    public ScreenPhase phase;

    public float elapsed;
    public float fromAlpha;
    public float toAlpha;

    public Action onHidden;
}

public sealed class ScreenSystem
{
    private readonly ScreenData[] _screens;

    public bool IsTransitioning { get; private set; }

    public ScreenSystem(ScreenView[] views)
    {
        _screens = new ScreenData[views.Length];

        for (int i = 0; i < views.Length; i++)
        {
            ScreenView view = views[i];

            view.gameObject.SetActive(true);
            view.CanvasGroup.alpha = 0f;
            view.CanvasGroup.interactable = false;
            view.CanvasGroup.blocksRaycasts = false;

            _screens[i] = new ScreenData
            {
                view = view,
                phase = ScreenPhase.Hidden
            };
        }
    }

    public void Show(int index)
    {
        ref ScreenData screen = ref _screens[index];

        if (screen.phase == ScreenPhase.Visible || screen.phase == ScreenPhase.Opening)
            return;

        screen.fromAlpha = screen.view.CanvasGroup.alpha;
        screen.toAlpha = 1f;
        screen.elapsed = 0f;
        screen.phase = ScreenPhase.Opening;
        screen.onHidden = null;
        IsTransitioning = true;

        SetInteraction(ref screen, false);
    }

    public void Hide(int index, Action onComplete = null)
    {
        ref ScreenData screen = ref _screens[index];

        if (screen.phase == ScreenPhase.Hidden)
        {
            onComplete?.Invoke();
            return;
        }

        if (screen.phase == ScreenPhase.Closing)
            return;

        screen.fromAlpha = screen.view.CanvasGroup.alpha;
        screen.toAlpha = 0f;
        screen.elapsed = 0f;
        screen.phase = ScreenPhase.Closing;
        screen.onHidden = onComplete;
        IsTransitioning = true;

        SetInteraction(ref screen, false);
    }

    public void Tick(float deltaTime)
    {
        for (int i = 0; i < _screens.Length; i++)
        {
            ref ScreenData screen = ref _screens[i];

            if (screen.phase != ScreenPhase.Opening && screen.phase != ScreenPhase.Closing)
                continue;

            ScreenView view = screen.view;

            float duration = Mathf.Max(0.0001f, view.Duration);

            screen.elapsed += deltaTime;

            float t = Mathf.Clamp01(screen.elapsed / duration);
            float eased = view.Curve.Evaluate(t);

            view.CanvasGroup.alpha = Mathf.LerpUnclamped(screen.fromAlpha, screen.toAlpha, eased);

            if (t < 1f)
                continue;

            bool opened = screen.phase == ScreenPhase.Opening;

            screen.phase = opened ? ScreenPhase.Visible : ScreenPhase.Hidden;

            view.CanvasGroup.alpha = opened ? 1f : 0f;
            IsTransitioning = false;

            SetInteraction(ref screen, opened);

            if (!opened)
            {
                Action callback = screen.onHidden;
                screen.onHidden = null;
                callback?.Invoke();
            }
        }
    }

    public bool CanInteract(int index)
    {
        return _screens[index].phase == ScreenPhase.Visible;
    }

    public ScreenPhase GetPhase(int index)
    {
        return _screens[index].phase;
    }

    private static void SetInteraction(ref ScreenData screen, bool enabled)
    {
        screen.view.CanvasGroup.interactable = enabled;
        screen.view.CanvasGroup.blocksRaycasts = enabled;
    }
}