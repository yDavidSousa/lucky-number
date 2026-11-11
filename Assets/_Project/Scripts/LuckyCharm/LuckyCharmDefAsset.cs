using UnityEngine;

public enum ModifierType : byte
{
    None,
    FlipBestDie,
    CopyBestDie,
    IncreaseLowestDie,
    DoubleSumScore,
    TripleDoubleScore,
    LastRollMultiplier
}

[CreateAssetMenu(menuName = AssetMenuEditor.LUCKY_CHARM_DEF)]
[Icon("Assets/Editor/Gizmos/luckycharm.png")]
public class LuckyCharmDefAsset : IdentifiedScriptableObject
{
    public ModifierType Type;
    public int Price;

    [Space]
    public string Name;
    public string Effect;

    [Header("Visual")]
    public GameObject Prefab;
}