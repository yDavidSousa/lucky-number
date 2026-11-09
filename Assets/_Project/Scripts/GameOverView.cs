using UnityEngine;

public class GameOverView : MonoBehaviour
{
    public UIButtonView RestartButton;
}

public sealed class GameOverController
{
    private readonly GameOverView _view;

    public GameOverController(GameOverView view)
    {
        _view = view;
        _view.RestartButton.Clicked += HandleRestartClicked;
        _view.gameObject.SetActive(false);
    }

    public void Hydrate()
    {
        _view.gameObject.SetActive(true);
    }

    private void HandleRestartClicked(UIButtonView button)
    {
        //_gameState.PendingPhase = GamePhase.Rolling;
        
        _view.gameObject.SetActive(false);
    }
}