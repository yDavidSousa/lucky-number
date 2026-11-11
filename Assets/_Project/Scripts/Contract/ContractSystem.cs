using UnityEngine;

public sealed class ContractSystem
{
    private readonly ContractDefRegistry _contractDefRegistry;

    public ContractSystem(ContractDefRegistry contractDefRegistry)
    {
        _contractDefRegistry = contractDefRegistry;
    }

    public void GenerateOffers(ContractDefData[] offers)
    {
        // Fisher-Yates parcial: três ofertas distintas.
        int[] indices = new int[_contractDefRegistry.Count];

        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        for (int i = 0; i < offers.Length; i++)
        {
            int random = Random.Range(i, indices.Length);

            (indices[i], indices[random]) = (indices[random], indices[i]);

            offers[i] = _contractDefRegistry.GetByIndex(indices[i]);
        }
    }

    public void ApplyStageRules(ContractDefData contract, ref int targetScore, ref int maxRolls)
    {
        switch (contract.type)
        {
            case ContractType.FewerRolls:
                maxRolls -= Mathf.RoundToInt(contract.value);
                break;

            case ContractType.HigherTarget:
                targetScore = Mathf.CeilToInt(targetScore * (1f + contract.value));
                break;
        }

        maxRolls = Mathf.Max(1, maxRolls);
    }

    public int ApplyScorePenalty(ContractDefData contract, int score)
    {
        if (contract.type != ContractType.ScorePenalty)
            return score;

        if (score > 0)
            return score;

        return -Mathf.RoundToInt(contract.value);
    }
}