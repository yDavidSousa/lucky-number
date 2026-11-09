using System.Collections.Generic;
using UnityEngine;

public sealed class LuckyNumberGenerator
{
    private readonly List<int> history = new();

    private readonly float[] hitChances =
    {
        11f / 36f,
        12f / 36f,
        13f / 36f,
        14f / 36f,
        15f / 36f,
        16f / 36f
    };

    public int Generate(float difficulty, int lossStreak)
    {
        difficulty = Mathf.Clamp01(difficulty);

        float[] weights = new float[6];
        float totalWeight = 0f;

        for (int i = 0; i < 6; i++)
        {
            int number = i + 1;

            // Dificuldade alta favorece alvos difíceis.
            float challenge = Mathf.Lerp(
                hitChances[i],
                1f - hitChances[i],
                difficulty
            );

            float weight = challenge;

            // Reduz a repetição dos últimos números.
            for (int j = 0; j < history.Count; j++)
            {
                if (history[j] == number)
                    weight *= 0.25f;
            }

            // Após derrotas, favorece alvos fáceis.
            float recovery = Mathf.Clamp01(lossStreak * 0.15f);

            weight = Mathf.Lerp(
                weight,
                hitChances[i],
                recovery
            );

            weights[i] = weight;
            totalWeight += weight;
        }

        float roll = Random.value * totalWeight;

        for (int i = 0; i < weights.Length; i++)
        {
            roll -= weights[i];

            if (roll <= 0f)
                return Record(i + 1);
        }

        return Record(6);
    }

    private int Record(int number)
    {
        history.Add(number);

        if (history.Count > 3)
            history.RemoveAt(0);

        return number;
    }
}