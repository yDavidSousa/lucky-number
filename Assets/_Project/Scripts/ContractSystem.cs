using UnityEngine;

public enum ContractType : byte
{
    None,
    FewerRolls,
    HigherTarget,
    ScorePenalty
}

public struct ContractData
{
    public ContractType Type;
    public float Value;
    public int Reward;
}

public static class ContractSystem
{
    private static readonly ContractData[] Pool =
    {
        new()
        {
            Type = ContractType.FewerRolls,
            Value = 1,
            Reward = 8
        },
        new()
        {
            Type = ContractType.HigherTarget,
            Value = 0.25f,
            Reward = 10
        },
        new()
        {
            Type = ContractType.ScorePenalty,
            Value = 100,
            Reward = 12
        }
    };

    public static void GenerateOffers(ContractData[] offers)
    {
        // Fisher-Yates parcial: três ofertas distintas.
        int[] indices = new int[Pool.Length];

        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;

        for (int i = 0; i < offers.Length; i++)
        {
            int random = Random.Range(i, indices.Length);

            (indices[i], indices[random]) = (indices[random], indices[i]);

            offers[i] = Pool[indices[i]];
        }
    }

    public static void ApplyStageRules(ContractData contract, ref int targetScore, ref int maxRolls)
    {
        switch (contract.Type)
        {
            case ContractType.FewerRolls:
                maxRolls -= Mathf.RoundToInt(contract.Value);
                break;

            case ContractType.HigherTarget:
                targetScore = Mathf.CeilToInt(targetScore * (1f + contract.Value));
                break;
        }

        maxRolls = Mathf.Max(1, maxRolls);
    }

    public static int ApplyScorePenalty(ContractData contract, int score)
    {
        if (contract.Type != ContractType.ScorePenalty)
            return score;

        if (score > 0)
            return score;

        return -Mathf.RoundToInt(contract.Value);
    }
}