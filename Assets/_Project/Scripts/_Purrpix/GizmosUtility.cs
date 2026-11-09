using UnityEngine;

public static class GizmosUtility
{
    public static void DrawCircleXZ(Vector3 center, float radius, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            Vector3 nextPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    public static void DrawCubeXZ(Vector3 center, float x, float z)
    {
        Vector3 half = new Vector3(x * 0.5f, 0f, z * 0.5f);

        Vector3 p1 = center + new Vector3(-half.x, 0f, -half.z);
        Vector3 p2 = center + new Vector3(-half.x, 0f,  half.z);
        Vector3 p3 = center + new Vector3( half.x, 0f,  half.z);
        Vector3 p4 = center + new Vector3( half.x, 0f, -half.z);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}