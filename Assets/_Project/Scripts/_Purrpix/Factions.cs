using System;
using System.Runtime.CompilerServices;

public static class Factions
{
    private const int SIZE = (byte)Faction.Count;

    private static readonly FactionRelation[] _relations =
    {
        // None row
        FactionRelation.Neutral, FactionRelation.Neutral, FactionRelation.Neutral,
        // Player row
        FactionRelation.Neutral, FactionRelation.Ally, FactionRelation.Hostile,
        // Enemy row
        FactionRelation.Neutral, FactionRelation.Hostile, FactionRelation.Ally,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FactionRelation GetRelation(Faction a, Faction b)
    {
#if DEBUG
        if (_relations.Length != SIZE * SIZE)
            throw new Exception("Faction table size mismatch");
        if ((byte)a >= SIZE || (byte)b >= SIZE)
            throw new ArgumentOutOfRangeException();
#endif

        int index = (byte)a * SIZE + (byte)b;
        return _relations[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAny(this FactionRelation mask, FactionRelation value) => (mask & value) != 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAll(this FactionRelation mask, FactionRelation value) => (mask & value) == value;
}

[Flags]
public enum FactionRelation { Ally = 1 << 0, Neutral = 1 << 1, Hostile = 1 << 2 }

public enum Faction : byte { None, Player, Enemy, Count }