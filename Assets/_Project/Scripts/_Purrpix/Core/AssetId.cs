using UnityEngine;

[System.Serializable]
public struct AssetId
{
    [SerializeField, HideInInspector] private string _guid;
    [SerializeField, HideInInspector] private int _runtimeId;
    public int RuntimeId => _runtimeId;

#if UNITY_EDITOR
    public string Guid => _guid;

    public void Validate(Object owner)
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(owner);

        if (string.IsNullOrEmpty(path))
            return;

        string assetGuid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
        if (_guid != assetGuid)
        {
            _guid = assetGuid;
            _runtimeId = HashUtility.FNV1a32(_guid);
            UnityEditor.EditorUtility.SetDirty(owner);
        }
    }
#endif
}

[System.Serializable]
public struct GuidId
{
    [SerializeField, HideInInspector] private string _guid;
    [SerializeField, HideInInspector] private int _runtimeId;
    public int RuntimeId => _runtimeId;

#if UNITY_EDITOR
    public string Guid => _guid;

    public void Validate(Object owner)
    {
        if (string.IsNullOrEmpty(_guid))
        {
            _guid = System.Guid.NewGuid().ToString();
            _runtimeId = HashUtility.FNV1a32(_guid);
            UnityEditor.EditorUtility.SetDirty(owner);
        }
    }
#endif
}

public static class HashUtility
{
    public static int FNV1a32(string value)
    {
        unchecked
        {
            int hash = (int)2166136261;

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619;
            }

            return hash;
        }
    }
}