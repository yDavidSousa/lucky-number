public struct StageSettings
{
    public int TargetScore;
    public int MaxRolls;
}

public static class StageProgression
{
    public const int STAGES_PER_ACT = 3;
    public const int TOTAL_STAGES = 9;

    private static readonly StageSettings[] Stages =
    {
        new() { TargetScore = 200, MaxRolls = 5 },
        new() { TargetScore = 350, MaxRolls = 5 },
        new() { TargetScore = 500, MaxRolls = 5 },

        new() { TargetScore = 800, MaxRolls = 5 },
        new() { TargetScore = 1200, MaxRolls = 5 },
        new() { TargetScore = 1800, MaxRolls = 5 },

        new() { TargetScore = 2500, MaxRolls = 5 },
        new() { TargetScore = 3500, MaxRolls = 5 },
        new() { TargetScore = 5000, MaxRolls = 5 }
    };

    public static StageSettings GetSettings(int stage)
    {
        return Stages[stage - 1];
    }

    public static bool IsActComplete(int stage)
    {
        return stage % STAGES_PER_ACT == 0;
    }

    public static int ActByStage(int stage)
    {
        return stage / STAGES_PER_ACT;
    }

    public static bool IsRunComplete(int stage)
    {
        return stage >= TOTAL_STAGES;
    }
}