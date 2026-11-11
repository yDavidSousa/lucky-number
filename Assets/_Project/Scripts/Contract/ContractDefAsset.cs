using UnityEngine;

public enum ContractType : byte
{
    None,
    FewerRolls,
    HigherTarget,
    ScorePenalty
}

[CreateAssetMenu(menuName = AssetMenuEditor.CONTRACT_DEF)]
[Icon("Assets/Editor/Gizmos/contract.png")]
public class ContractDefAsset : IdentifiedScriptableObject
{
    public ContractType Type;
    public float Value;
    public int Reward;

    [Space]
    public string Name;
    public string Flavor;
    public string Effect;

    [Header("Visual")]
    public GameObject Prefab;
}