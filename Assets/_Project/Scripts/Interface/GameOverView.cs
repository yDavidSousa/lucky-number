using System;
using UnityEngine;

[RequireComponent(typeof(ScreenSystem))]
public class GameOverView : MonoBehaviour
{
    public static int Id => (int)ScreenId.GameOver;
    public ScreenView Screen;

    [Space]
    public UIButtonView RestartButton;
}

public sealed class GameOverController
{
    private readonly GameOverView _view;

    private readonly Action _leaveClicked;

    public GameOverController(GameOverView view, Action leaveClicked = null)
    {
        _view = view;
        _view.RestartButton.Clicked += HandleRestartClicked;

        _leaveClicked = leaveClicked;
    }

    private void HandleRestartClicked(UIButtonView button)
    {
        _leaveClicked?.Invoke();
    }
}