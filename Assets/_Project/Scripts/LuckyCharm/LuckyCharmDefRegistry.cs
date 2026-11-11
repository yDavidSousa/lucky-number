using System.Collections.Generic;
using UnityEngine;

public readonly struct LuckyCharmDefData
{
    public readonly int id;
    public readonly ModifierType type;
    public readonly int price;

    public readonly string name;
    public readonly string effect;

    public readonly GameObject viewPrefab;

    public LuckyCharmDefData(int id, ModifierType type, int price, string name, string effect, GameObject viewPrefab)
    {
        this.id = id;
        this.type = type;
        this.price = price;
        this.name = name;
        this.effect = effect;
        this.viewPrefab = viewPrefab;
    }
}

public class LuckyCharmDefRegistry : System.IDisposable
{
    private LuckyCharmDefData[] _definitions = System.Array.Empty<LuckyCharmDefData>();
    private readonly Dictionary<int, int> _indexById = new();

    public int Count => _definitions.Length;

    public void Load(string path)
    {
        _indexById.Clear();

        LuckyCharmDefAsset[] assets = Resources.LoadAll<LuckyCharmDefAsset>(path);
        List<LuckyCharmDefData> definitions = new List<LuckyCharmDefData>(assets.Length);

        foreach (LuckyCharmDefAsset asset in assets)
        {
            int id = asset.Id;
            if (!_indexById.TryAdd(id, definitions.Count))
            {
                Debug.LogError($"Duplicate LuckyCharm ID: {asset.name}");
                continue;
            }

            definitions.Add(
                new LuckyCharmDefData(
                    asset.Id,
                    asset.Type,
                    asset.Price,
                    asset.Name,
                    asset.Effect,
                    asset.Prefab
                )
            );
        }

        _definitions = definitions.ToArray();
    }

    public void Dispose()
    {
        _definitions = System.Array.Empty<LuckyCharmDefData>();
        _indexById.Clear();
    }

    public LuckyCharmDefData Get(int id)
    {
        return _definitions[_indexById[id]];
    }

    public LuckyCharmDefData GetByIndex(int index)
    {
        return _definitions[index];
    }

    public bool TryGet(int id, out LuckyCharmDefData definition)
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