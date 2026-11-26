using UnityEngine;

[System.Serializable]
public struct DiceWeights
{
    public float One;
    public float Two;
    public float Three;
    public float Four;
    public float Five;
    public float Six;

    public static DiceWeights Fair => new()
    {
        One = 1f,
        Two = 1f,
        Three = 1f,
        Four = 1f,
        Five = 1f,
        Six = 1f
    };

    public float Get(int face)
    {
        return face switch
        {
            1 => One,
            2 => Two,
            3 => Three,
            4 => Four,
            5 => Five,
            6 => Six,
            _ => 0f
        };
    }

    public void Set(int face, float weight)
    {
        weight = Mathf.Max(0f, weight);

        switch (face)
        {
            case 1: One = weight; break;
            case 2: Two = weight; break;
            case 3: Three = weight; break;
            case 4: Four = weight; break;
            case 5: Five = weight; break;
            case 6: Six = weight; break;
        }
    }
}

public static class DiceRollSystem
{
    public static int Roll(in DiceWeights weights)
    {
        float total = 0f;

        for (int face = 1; face <= 6; face++)
            total += Mathf.Max(0f, weights.Get(face));

        if (total <= 0f)
        {
            Debug.LogWarning("Invalid dice weights.");
            return Random.Range(1, 7);
        }

        float random = Random.value * total;

        for (int face = 1; face <= 6; face++)
        {
            random -= Mathf.Max(0f, weights.Get(face));

            if (random < 0f)
                return face;
        }

        return 6;
    }

    public static void RollPair(in DiceWeights weightsA, in DiceWeights weightsB, out int dieA, out int dieB)
    {
        dieA = Roll(weightsA);
        dieB = Roll(weightsB);
    }
}