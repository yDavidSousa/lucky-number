using UnityEngine;

public static class DiceCollisionSystem
{
    public static void Resolve(DieView a, DieView b)
    {
        if (!a.IsRolling && !b.IsRolling)
            return;

        Vector3 delta = b.transform.position - a.transform.position;
        delta.y = 0f;

        float minDistance = a.CollisionRadius + b.CollisionRadius;
        float sqrDistance = delta.sqrMagnitude;

        if (sqrDistance >= minDistance * minDistance)
            return;

        float distance = Mathf.Sqrt(sqrDistance);
        Vector3 direction = distance > 0.0001f ? delta / distance : Vector3.right;

        float penetration = minDistance - distance;
        Vector3 correction = direction * penetration;

        if (a.IsRolling && b.IsRolling)
        {
            a.ResolvePosition(-correction * 0.5f);
            b.ResolvePosition(correction * 0.5f);
        }
        else if (a.IsRolling)
        {
            a.ResolvePosition(-correction);
        }
        else if (b.IsRolling)
        {
            b.ResolvePosition(correction);
        }

        a.ApplyCollision(-direction);
        b.ApplyCollision(direction);
    }
}