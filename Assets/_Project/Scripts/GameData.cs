public static class GameData
{
    public static readonly LuckyCharmDefRegistry LuckyCharmDefRegistry = new();
    public static readonly ContractDefRegistry ContractDefRegistry = new();

    public static void Initialize()
    {
        LuckyCharmDefRegistry.Load("LuckyCharms");
        ContractDefRegistry.Load("Contracts");
    }

    public static void Dispose()
    {
        LuckyCharmDefRegistry.Dispose();
        ContractDefRegistry.Dispose();
    }
}