using System;
using TMPro;
using UnityEngine;

public class ShopView : MonoBehaviour
{
    public TextMeshProUGUI CurrencyText;
    public UIButtonView LeaveButton;

    public UIButtonView ItemPrefab;
    public RectTransform ItemContainer;
    public readonly UIButtonView[] BuyButtons = new UIButtonView[3];
}

public sealed class ShopController
{
    private readonly ShopView _view;

    private Action _leaveClicked;
    private Action<int> _buyClicked;
    
    public ShopController(ShopView view)
    {
        _view = view;
        _view.gameObject.SetActive(false);

        for (int i = 0; i < _view.BuyButtons.Length; i++)
        {
            _view.BuyButtons[i] = UnityEngine.Object.Instantiate(_view.ItemPrefab, _view.ItemContainer);
            _view.BuyButtons[i].Clicked += HandleBuyClicked;
            _view.BuyButtons[i].gameObject.SetActive(true);
        }

        _view.LeaveButton.Clicked += HandleLeaveClicked;
    }

    public void Hydrate(GameState gameState, Action leaveClicked = null, Action<int> buyClicked = null)
    {
        _view.CurrencyText.text = $"Currency: {gameState.Currency}";
        _leaveClicked ??= leaveClicked;
        _buyClicked ??= buyClicked;
        _view.gameObject.SetActive(true);
    }

    private void HandleLeaveClicked(UIButtonView button)
    {
        _leaveClicked?.Invoke();
        _leaveClicked = null;
        _view.gameObject.SetActive(false);
    }

    private void HandleBuyClicked(UIButtonView button)
    {
        int index = -1;
        for (int i = 0; i < _view.BuyButtons.Length; i++)
        {
            if (_view.BuyButtons[i] == button)
            {
                index = i;
                break;
            }
        }

        _buyClicked?.Invoke(index);
        _buyClicked = null;
    }
}