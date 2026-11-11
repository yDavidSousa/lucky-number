using Random = UnityEngine.Random;

public sealed class DialogueSystem
{
    private readonly DialogueDefAsset[] _entries;
    private readonly float[] _lastPlayed;
    private readonly bool[] _used;

    public DialogueSystem(DialogueDefAsset[] entries)
    {
        _entries = entries;
        _lastPlayed = new float[_entries.Length];
        _used = new bool[_entries.Length];

        for (int i = 0; i < _entries.Length; i++)
            _lastPlayed[i] = float.NegativeInfinity;
    }

    public bool TryGet(DialogueTrigger trigger, DialogueAct act, float time, out DialogueDefAsset result)
    {
        result = null;

        float totalWeight = 0f;
        for (int i = 0; i < _entries.Length; i++)
        {
            if (IsEligible(i, trigger, act, time))
            {
                totalWeight += _entries[i].Weight;
            }
        }

        if (totalWeight <= 0f)
            return false;

        float random = Random.value * totalWeight;
        int selected = -1;

        for (int i = 0; i < _entries.Length; i++)
        {
            if (!IsEligible(i, trigger, act, time))
                continue;

            selected = i;
            random -= _entries[i].Weight;

            if (random < 0f)
                break;
        }

        if (selected < 0)
            return false;

        var entry = _entries[selected];
        if (Random.value > entry.Chance)
            return false;

        _lastPlayed[selected] = time;
        _used[selected] = true;

        result = entry;
        return true;
    }

    private bool IsEligible(int index, DialogueTrigger trigger, DialogueAct act, float time)
    {
        var entry = _entries[index];

        if (entry == null || entry.Lines == null || entry.Lines.Length == 0)
            return false;

        if (entry.Trigger != trigger)
            return false;

        if (entry.Act != DialogueAct.Any && entry.Act != act)
            return false;

        if (entry.Weight <= 0f)
            return false;

        if (entry.Once && _used[index])
            return false;

        if (time - _lastPlayed[index] < entry.Cooldown)
            return false;

        return true;
    }
}