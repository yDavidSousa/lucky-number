using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(ScreenSystem))]
public class GameplayHUDView : MonoBehaviour
{
    public static int Id => (int)ScreenId.HUD;
    public ScreenView Screen;

    [Space]
    public LuckyNumberView LuckyNumberView;
    public TextMeshProUGUI TargetScoreText;
    public TextMeshProUGUI StageScoreText;
    public TextMeshProUGUI RollCountText;
    public UIButtonView ThrowButton;
}

public sealed class GameplayHUDController
{
    private readonly GameplayHUDView _view;

    private readonly Action _throwDice;

    public GameplayHUDController(GameplayHUDView view, Action throwDice = null)
    {
        _view = view;

        _throwDice = throwDice;
        _view.ThrowButton.Clicked += HandleThrowClicked;
    }

    public void Tick(float deltaTime)
    {
        _view.LuckyNumberView.Tick(deltaTime);
    }

    public void SpinLuckyNumber(int luckyNumber)
    {
        _view.LuckyNumberView.Spin(luckyNumber, ()=> _view.ThrowButton.gameObject.SetActive(true));
    }

    public void Hydrate(GameState gameState)
    {
        _view.TargetScoreText.text = gameState.StageSettings.TargetScore.ToString();
        _view.StageScoreText.text = gameState.StageScore.ToString();
        _view.RollCountText.text = $"{gameState.StageSettings.MaxRolls - gameState.RollsRemaining} / {gameState.StageSettings.MaxRolls}";
    }

    private void HandleThrowClicked(UIButtonView button)
    {
        _view.ThrowButton.gameObject.SetActive(false);
        _throwDice?.Invoke();
    }
}