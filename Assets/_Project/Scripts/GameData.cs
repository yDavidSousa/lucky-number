using UnityEngine;

public static class GameData
{
    public static readonly LuckyCharmDefRegistry LuckyCharmDefRegistry = new();
    public static readonly ContractDefRegistry ContractDefRegistry = new();

    public static DialogueDefAsset[] DialogueDefAssets = System.Array.Empty<DialogueDefAsset>();

    public static void Initialize()
    {
        LuckyCharmDefRegistry.Load("LuckyCharms");
        ContractDefRegistry.Load("Contracts");
        DialogueDefAssets = Resources.LoadAll<DialogueDefAsset>("Dialogues"); //create def registry to find dialogue by params and pass like method to trigger
    }

    public static void Dispose()
    {
        LuckyCharmDefRegistry.Dispose();
        ContractDefRegistry.Dispose();
    }
}