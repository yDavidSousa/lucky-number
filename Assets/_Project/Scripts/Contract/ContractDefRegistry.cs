using System.Collections.Generic;
using UnityEngine;

public readonly struct ContractDefData
{
    public readonly int id;
    public readonly ContractType type;
    public readonly float value;
    public readonly int reward;

    public readonly GameObject viewPrefab;

    public ContractDefData(int id, ContractType type, float value, int reward, GameObject viewPrefab)
    {
        this.id = id;
        this.type = type;
        this.value = value;
        this.reward = reward;
        this.viewPrefab = viewPrefab;
    }
}

public class ContractDefRegistry : System.IDisposable
{
    private ContractDefData[] _definitions = System.Array.Empty<ContractDefData>();
    private readonly Dictionary<int, int> _indexById = new();

    public int Count => _definitions.Length;

    public void Load(string path)
    {
        _indexById.Clear();

        ContractDefAsset[] assets = Resources.LoadAll<ContractDefAsset>(path);
        List<ContractDefData> definitions = new List<ContractDefData>(assets.Length);

        foreach (ContractDefAsset asset in assets)
        {
            int id = asset.Id;
            if (!_indexById.TryAdd(id, definitions.Count))
            {
                Debug.LogError($"Duplicate Contract ID: {asset.name}");
                continue;
            }

            definitions.Add(
                new ContractDefData(
                    asset.Id,
                    asset.Type,
                    asset.Value,
                    asset.Reward,
                    asset.Prefab
                )
            );
        }

        _definitions = definitions.ToArray();
    }

    public void Dispose()
    {
        _definitions = System.Array.Empty<ContractDefData>();
        _indexById.Clear();
    }

    public ContractDefData Get(int id)
    {
        return _definitions[_indexById[id]];
    }

    public ContractDefData GetByIndex(int index)
    {
        return _definitions[index];
    }

    public bool TryGet(int id, out ContractDefData definition)
    {
        if (_indexById.TryGetValue(id, out int index))
        {
            definition = _definitions[index];
            return true;
        }

        definition = default;
        return false;
    }
}