using UnityEngine;

public struct SpawnData
{
    public Vector3 position;
    public Quaternion rotation;
    public Transform parent;

    public SpawnData(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        this.position = position;
        this.rotation = rotation;
        this.parent = parent;
    }
}

public static class SpawnUtility
{
    public static SpawnData GetPose(SpawnAreaReferences area, int count, int index = -1, bool randomRotation = true)
    {
        Vector3 position = area.Mode switch
        {
            SpawnMode.Random => GetRandomPosition(area.Bounds),
            SpawnMode.Sequential => GetSequentialPosition(area.Bounds, index, count, area.Spacing, area.Axis),
            SpawnMode.FixedPoints => GetFixedPosition(area.Points, index),
            _ => area.transform.position
        };
        Quaternion rotation;
        if (randomRotation)
        {
            Vector3 euler = Random.rotationUniform.eulerAngles;
            rotation = Quaternion.Euler(0f, euler.y, 0f);
        }
        else
        {
            rotation = Quaternion.identity;
        }

        Transform parent = area.Mode switch
        {
            SpawnMode.FixedPoints => GetFixedTransform(area.Points, index),
            _ => area.transform
        };

        return new SpawnData(position, rotation, parent);
    }

    public static Vector3 GetRandomPosition(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public static Vector3 GetSequentialPosition(Bounds bounds, int index, int count, float spacing, SpawnAxis axis)
    {
        float length = (count - 1) * spacing;
        float offset = index * spacing - length * 0.5f;

        Vector3 position = bounds.center;

        if (axis == SpawnAxis.X)
            position.x += offset;
        else
            position.z += offset;

        return position;
    }

    public static Vector3 GetFixedPosition(Transform[] points, int index)
    {
        if (points == null || points.Length == 0) return Vector3.zero;

        if (index == -1)
        {
            index = Random.Range(0, points.Length);
        }

        return points[index].position;
    }

    public static Transform GetFixedTransform(Transform[] points, int index)
    {
        if (index == -1)
        {
            index = Random.Range(0, points.Length);
        }

        return points[index];
    }
}