using UnityEngine;

public static class LayerUtil
{
    public static void SetLayerRecursively(Transform transform, int layer, bool allChild = true)
    {
        transform.gameObject.layer = layer;

        if (!allChild)
        {
            return;
        }

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            SetLayerRecursively(child, layer);
        }
    }
}