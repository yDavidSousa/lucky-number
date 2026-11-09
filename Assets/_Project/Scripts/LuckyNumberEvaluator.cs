using UnityEngine;

public enum LuckyMatchType
{
    None,
    SingleDie,
    Sum,
    Dif,
    DoubleDice
}

public struct LuckyResult
{
    public LuckyMatchType MatchType;
    public int Score;
}


public static class LuckyNumberEvaluator
{
    public static LuckyResult Evaluate(int luckyNumber, int dice1, int dice2, int baseScore = 100)
    {
        if (dice1 == luckyNumber && dice2 == luckyNumber)
        {
            return new LuckyResult
            {
                MatchType = LuckyMatchType.DoubleDice,
                Score = baseScore * 5
            };
        }

        if (dice1 + dice2 == luckyNumber)
        {
            return new LuckyResult
            {
                MatchType = LuckyMatchType.Sum,
                Score = baseScore * 3
            };
        }

        if (Mathf.Abs(dice1 - dice2) == luckyNumber)
        {
            return new LuckyResult
            {
                MatchType = LuckyMatchType.Dif,
                Score = baseScore * 3
            }; 
        }

        if (dice1 == luckyNumber || dice2 == luckyNumber)
        {
            return new LuckyResult
            {
                MatchType = LuckyMatchType.SingleDie,
                Score = baseScore
            };
        }

        return new LuckyResult
        {
            MatchType = LuckyMatchType.None,
            Score = 0
        };
    }
}