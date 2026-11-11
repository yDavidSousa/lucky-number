using UnityEngine;


public enum DialogueTrigger : byte
{
    FirstEncounter,
    MISSING_ActStarted,
    LuckyNumberRevealed,
    MISSING_RollStarted,
    SingleMatch,
    SumMatch,
    DoubleMatch,
    RollMissed,
    MISSING_MissStreak,
    MISSING_StageCompleted,
    MISSING_StageFailed,
    ShopOpened,
    MISSING_CharmPurchased,
    MISSING_InsufficientCoins,
    MISSING_ShopClosed,
    MISSING_ContractSelection,
    MISSING_ContractSelected,
    MISSING_ContractCompleted,
    MISSING_BossStarted,
    MISSING_BossDamaged,
    MISSING_BossDefeated
}

public enum DialogueAct : byte
{
    Any,
    Act1,
    Act2,
    Act3,
    Boss
}

public enum DialoguePriority : byte
{
    Normal,
    Important,
    Critical
}

public enum DialogueDisplayMode : byte
{
    Instant,
    Typewriter
}

[System.Serializable]
public struct DialogueLine
{
    [TextArea(2, 4)]
    public string Text;

    public DialogueDisplayMode DisplayMode;

    [Min(1)] public int CharactersPerSecond;
    [Min(0f)] public float DisplayDuration;
    [Min(0f)] public float DelayAfter;
}

[CreateAssetMenu(menuName = AssetMenuEditor.DIALOGUE_DEF)]
public sealed class DialogueDefAsset : ScriptableObject
{
    [Header("Conditions")]
    public DialogueTrigger Trigger;
    public DialogueAct Act;
    public DialoguePriority Priority;

    [Header("Selection")]
    [Range(0f, 1f)]
    public float Chance = 1f;

    [Min(0f)]
    public float Weight = 1f;

    public bool Once;

    [Min(0f)]
    public float Cooldown;

    [Header("Dialogue")]
    public DialogueLine[] Lines;
}
