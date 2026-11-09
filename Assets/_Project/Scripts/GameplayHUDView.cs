using TMPro;
using UnityEngine;

public class GameplayHUDView : MonoBehaviour
{
    public TextMeshProUGUI LuckyNumberText;
    public TextMeshProUGUI TargetScoreText;
    public TextMeshProUGUI StageScoreText;
    public TextMeshProUGUI RollCountText;
    public UIButtonView ThrowButton;
}

public sealed class GameplayHUDController
{
    private readonly GameplayHUDView _view;
    private readonly GameState _gameState;

    public GameplayHUDController(GameplayHUDView view, GameState gameState)
    {
        _view = view;
        _gameState = gameState;
        view.ThrowButton.Clicked += HandleThrowClicked;
    }

    public void LateTick()
    {
        bool visible = _gameState.Phase == GamePhase.Preparation;
        if (_view.ThrowButton.gameObject.activeSelf != visible)
        {
            _view.ThrowButton.gameObject.SetActive(visible);
        }
    }

    public void Hydrate(GameState gameState)
    {
        _view.LuckyNumberText.text = gameState.LuckyNumber.ToString();
        _view.TargetScoreText.text = gameState.StageSettings.TargetScore.ToString();
        _view.StageScoreText.text = gameState.StageScore.ToString();
        _view.RollCountText.text = $"{gameState.StageSettings.MaxRolls - gameState.RollsRemaining} / {gameState.StageSettings.MaxRolls}";
    }

    private void HandleThrowClicked(UIButtonView button)
    {
        _gameState.PendingPhase = GamePhase.Rolling;
    }
}