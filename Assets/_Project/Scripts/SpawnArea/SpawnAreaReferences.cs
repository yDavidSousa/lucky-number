using UnityEngine;

public enum SpawnMode : byte
{
    Random,
    Sequential,
    FixedPoints
}

public enum SpawnAxis : byte
{
    X,
    Z
}

public sealed class SpawnAreaReferences : MonoBehaviour
{
    [Header("Configuration")]
    public SpawnMode Mode = SpawnMode.Random;

    [Header("Bounds")]
    public Vector2 Size = new(4f, 4f);

    [Header("Sequential")]
    public SpawnAxis Axis = SpawnAxis.X;
    public float Spacing = 2f;

    [Header("Fixed Points")]
    public Transform[] Points;

    public Bounds Bounds => new(transform.position, new Vector3(Size.x, 0f, Size.y));

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (Mode == SpawnMode.FixedPoints)
        {
            if (Points == null)
                return;

            foreach (Transform point in Points)
            {
                if (point == null)
                    continue;

                Gizmos.DrawWireSphere(point.position, 0.15f);
                Gizmos.DrawLine(transform.position, point.position);
            }

            return;
        }

        Gizmos.DrawWireCube(transform.position, new Vector3(Size.x, 0f, Size.y));
    }
}
