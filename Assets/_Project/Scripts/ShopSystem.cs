using UnityEngine;

public static class ShopSystem
{
    private static readonly ModifierData[] Pool =
    {
        new() { Type = ModifierType.FlipBestDie, Price = 5 },
        new() { Type = ModifierType.CopyBestDie, Price = 12 },
        new() { Type = ModifierType.IncreaseLowestDie, Price = 6 },
        new() { Type = ModifierType.DoubleSumScore, Price = 8 },
        new() { Type = ModifierType.TripleDoubleScore, Price = 10 },
        new() { Type = ModifierType.LastRollMultiplier, Price = 9 }
    };

    public static void GenerateOffers(ModifierData[] offers, bool[] purchased)
    {
        int[] indices = new int[Pool.Length];

        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        for (int i = 0; i < offers.Length; i++)
        {
            int random = Random.Range(i, indices.Length);

            (indices[i], indices[random]) = (indices[random], indices[i]);

            offers[i] = Pool[indices[i]];
            purchased[i] = false;
        }
    }

    public static bool TryBuy(GameState state, int offerIndex)
    {
        if (offerIndex < 0 || offerIndex >= state.ShopOffers.Length)
            return false;

        if (state.ShopPurchased[offerIndex])
            return false;

        ModifierData offer = state.ShopOffers[offerIndex];

        if (state.Currency < offer.Price)
            return false;

        int freeSlot = -1;

        for (int i = 0; i < state.EquippedModifiers.Length; i++)
        {
            if (state.EquippedModifiers[i] == ModifierType.None)
            {
                freeSlot = i;
                break;
            }
        }

        if (freeSlot < 0)
            return false;

        state.Currency -= offer.Price;
        state.EquippedModifiers[freeSlot] = offer.Type;
        state.ShopPurchased[offerIndex] = true;

        return true;
    }
}