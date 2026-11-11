using UnityEngine;

public sealed class ShopSystem
{
    private readonly LuckyCharmDefRegistry _luckyCharmDefRegistry;

    public ShopSystem(LuckyCharmDefRegistry luckyCharmDefRegistry)
    {
        _luckyCharmDefRegistry = luckyCharmDefRegistry;
    }

    public void GenerateOffers(LuckyCharmDefData[] offers, ref bool[] purchased)
    {
        int[] indices = new int[_luckyCharmDefRegistry.Count];

        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        for (int i = 0; i < offers.Length; i++)
        {
            int random = Random.Range(i, indices.Length);

            (indices[i], indices[random]) = (indices[random], indices[i]);

            offers[i] = _luckyCharmDefRegistry.GetByIndex(indices[i]);
            purchased[i] = false;
        }
    }

    public bool TryBuy(GameState state, int offerIndex, out int slot)
    {
        slot = -1;

        if (offerIndex < 0 || offerIndex >= state.ShopOffers.Length)
            return false;

        if (state.ShopPurchased[offerIndex])
            return false;

        LuckyCharmDefData offer = state.ShopOffers[offerIndex];

        if (state.Currency < offer.price)
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

        state.Currency -= offer.price;
        state.EquippedModifiers[freeSlot] = offer.type;
        state.ShopPurchased[offerIndex] = true;

        slot = freeSlot;
        return true;
    }
}