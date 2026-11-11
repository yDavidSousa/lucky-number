using System;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

[RequireComponent(typeof(ScreenSystem))]
public class ShopView : MonoBehaviour
{
    public static int Id => (int)ScreenId.Shop;
    public ScreenView Screen;

    [Space]
    public TextMeshProUGUI CurrencyText;
    public UIButtonView LeaveButton;

    public LuckyCharmInfoView LuckyCharmInfoViewPrefab;
}

public struct ModifierEntry
{
    public SelectableObjectView view;
    public LuckyCharmInfoView infoView;

    public bool isAvailable;
}

public sealed class ShopController
{
    private readonly Camera _worldCamera;
    private readonly ShopView _view;
    private readonly ModifierReferences _modifierReferences;

    private readonly ModifierEntry[] _entries;

    private readonly Action _leaveClicked;
    private readonly Action<int> _purchaseClicked;

    public ShopController(GameState state, ShopView view, Camera worldCamera, ModifierReferences modifierReferences, Action leaveClicked = null, Action<int> purchaseClicked = null)
    {
        _worldCamera = worldCamera;
        _modifierReferences = modifierReferences;

        _entries = new ModifierEntry[state.ShopOffers.Length];

        _leaveClicked = leaveClicked;
        _purchaseClicked = purchaseClicked;

        _view = view;
        _view.LeaveButton.Clicked += HandleLeaveClicked;
    }

    public void Open(GameState state)
    {
        SetOffers(state.ShopOffers);
        Hydrate(state);
    }

    public void LateTick(LuckyCharmDefData[] offers)
    {
        for (int i = 0; i < _entries.Length; i++)
        {
            if (_entries[i].view == null)
            {
                continue;
            }

            _entries[i].infoView.LateTick(_entries[i].view.isHovered);
        }
    }

    private void SetOffers(LuckyCharmDefData[] offers)
    {
        for (int i = 0; i < offers.Length; i++)
        {
            SelectableObjectView modifierItem = Object.Instantiate(offers[i].viewPrefab, _modifierReferences.slots[i].transform).GetComponent<SelectableObjectView>();
            LuckyCharmInfoView modifierInfoView = Object.Instantiate(_view.LuckyCharmInfoViewPrefab, _view.gameObject.transform);

            Vector3 worldPosition = modifierItem.transform.position;
            Vector2 screenPoint = _worldCamera.WorldToScreenPoint(worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _view.transform as RectTransform,
                screenPoint,
                _worldCamera, //view.canvas.camera
                out Vector2 localPoint
            );
            modifierInfoView.RectTransform.anchoredPosition = localPoint;
            modifierItem.Clicked += HandlePurchaseClicked;
            _entries[i] = new ModifierEntry
            {
                view = modifierItem,
                infoView = modifierInfoView,
                isAvailable = true
            };
            modifierInfoView.gameObject.SetActive(true);
        }
    }

    private void CleatOffers()
    {
        for (int i = _entries.Length - 1; i >= 0; i--)
        {
            if (_entries[i].view != null)
            {
                Object.Destroy(_entries[i].view.gameObject);
            }

            if (_entries[i].infoView != null)
            {
                Object.Destroy(_entries[i].infoView.gameObject);
            }

            _entries[i] = default;
        }
    }

    public void Hydrate(GameState state)
    {
        _view.CurrencyText.text = state.Currency.ToString();

        for (int i = 0; i < _entries.Length; i++)
        {
            ModifierEntry entry =  _entries[i];
            bool purchased = state.ShopPurchased[i];
            entry.infoView.Hydrate(state.ShopOffers[i], purchased);

            if (entry.isAvailable && purchased)
            {
                LayerUtil.SetLayerRecursively(entry.view.gameObject.transform, LayerMask.NameToLayer("Default"));
                entry.isAvailable = false;
            }

            _entries[i] = entry;
        }
    }

    private void HandleLeaveClicked(UIButtonView button)
    {
        CleatOffers();
        _leaveClicked?.Invoke();
    }

    private void HandlePurchaseClicked(SelectableObjectView selectableObjectView)
    {
        int index = -1;
        for (int i = 0; i < _entries.Length; i++)
        {
            if(_entries[i].view == selectableObjectView)
            {
                index = i;
                break;
            }
        }

        _purchaseClicked?.Invoke(index);
    }
}