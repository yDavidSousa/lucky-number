public sealed class LuckyCharmSystem
{
    public void ApplyDiceModifiers(ModifierType[] modifiers, int luckyNumber, ref int dieA, ref int dieB)
    {
        foreach (ModifierType modifier in modifiers)
        {
            switch (modifier)
            {
                case ModifierType.FlipBestDie:
                {
                    FlipBestDie(luckyNumber, ref dieA, ref dieB);
                    break;   
                }
                case ModifierType.CopyBestDie:
                {
                    CopyBestDie(luckyNumber, ref dieA, ref dieB);
                    break;   
                }
                case ModifierType.IncreaseLowestDie:
                {
                    if (dieA <= dieB)
                        dieA = System.Math.Min(6, dieA + 1);
                    else
                        dieB = System.Math.Min(6, dieB + 1);
                    break;   
                }
            }
        }
    }

    private int Evaluate(int luckyNumber, int a, int b)
    {
        return LuckyNumberEvaluator.Evaluate(luckyNumber, a, b).Score;
    }

    private void FlipBestDie(int luckyNumber, ref int a, ref int b)
    {
        int originalA = a;
        int originalB = b;

        int bestScore = Evaluate(luckyNumber, a, b);

        int flippedA = 7 - originalA;
        int scoreA = Evaluate(luckyNumber, flippedA, originalB);

        if (scoreA > bestScore)
        {
            bestScore = scoreA;
            a = flippedA;
            b = originalB;
        }

        int flippedB = 7 - originalB;
        int scoreB = Evaluate(luckyNumber, originalA, flippedB);

        if (scoreB > bestScore)
        {
            a = originalA;
            b = flippedB;
        }
    }

    private void CopyBestDie(int luckyNumber, ref int a, ref int b)
    {
        int originalA = a;
        int originalB = b;

        int bestScore = Evaluate(luckyNumber, a, b);

        int scoreA = Evaluate(luckyNumber, originalB, originalB);

        if (scoreA > bestScore)
        {
            bestScore = scoreA;
            a = originalB;
            b = originalB;
        }

        int scoreB = Evaluate(luckyNumber, originalA, originalA);

        if (scoreB > bestScore)
        {
            a = originalA;
            b = originalA;
        }
    }

    public int ApplyScoreModifiers(ModifierType[] modifiers, LuckyMatchType matchType, int score, int rollsRemaining)
    {
        float multiplier = 1f;

        foreach (ModifierType modifier in modifiers)
        {
            switch (modifier)
            {
                case ModifierType.DoubleSumScore:
                {
                    if (matchType == LuckyMatchType.Sum)
                        multiplier *= 2f;
                    break;
                }
                case ModifierType.TripleDoubleScore:
                {
                    if (matchType == LuckyMatchType.DoubleDice)
                        multiplier *= 3f;
                    break;
                }
                case ModifierType.LastRollMultiplier:
                {
                    if (rollsRemaining == 1)
                        multiplier *= 4f;
                    break;
                }
            }
        }

        return UnityEngine.Mathf.RoundToInt(score * multiplier);
    }
}
