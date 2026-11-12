using System;
using UnityEngine;

[RequireComponent(typeof(ScreenSystem))]
public class ContractView : MonoBehaviour
{
    public static int Id => (int)ScreenId.Contracts;
    public ScreenView Screen;
}

public struct ContractEntry
{
    public SelectableObjectView view;
    public LuckyCharmInfoView infoView;

    public bool isAvailable;
}

public sealed class ContractController
{
    private readonly ContractView _view;
    private readonly SpawnAreaReferences _contractArea;

    private readonly ContractEntry[] _entries;

    private readonly Action<int> _sealClicked;

    public ContractController(GameState state, ContractView view, SpawnAreaReferences contractArea, Action<int> sealClicked = null)
    {
        _contractArea = contractArea;
        _sealClicked = sealClicked;

        _entries = new ContractEntry[state.ShopOffers.Length];

        _view = view;
    }

    public void SetContracts(ContractDefData[] contracts)
    {
        for (int i = 0; i < contracts.Length; i++)
        {
            SpawnData spawnPose = SpawnUtility.GetPose(_contractArea, contracts.Length, i);
            GameObject luckyCharmView = UnityEngine.Object.Instantiate(contracts[i].viewPrefab, spawnPose.parent);
            SelectableObjectView contractView = luckyCharmView.GetComponent<SelectableObjectView>();
            // Vector3 worldPosition = modifierItem.transform.position;
            // Vector2 screenPoint = _worldCamera.WorldToScreenPoint(worldPosition);
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //     _view.transform as RectTransform,
            //     screenPoint,
            //     _worldCamera, //view.canvas.camera
            //     out Vector2 localPoint
            // );
            // modifierInfoView.RectTransform.anchoredPosition = localPoint;
            contractView.Clicked += HandleSealClicked;
            _entries[i] = new ContractEntry
            {
                view = contractView
            };
        }
    }

    private void CleatContracts()
    {
        for (int i = _entries.Length - 1; i >= 0; i--)
        {
            if (_entries[i].view != null)
            {
                UnityEngine.Object.Destroy(_entries[i].view.gameObject);
            }

            // if (_entries[i].infoView != null)
            // {
            //     Object.Destroy(_entries[i].infoView.gameObject);
            // }

            _entries[i] = default;
        }
    }

    private void HandleSealClicked(SelectableObjectView selectableObjectView)
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

        CleatContracts();
        _sealClicked?.Invoke(index);
    }
}
