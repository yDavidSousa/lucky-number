using UnityEngine;

public abstract class IdentifiedScriptableObject : ScriptableObject
{
    [SerializeField, HideInInspector] private AssetId _id;
    [SerializeField, HideInInspector] private ulong _version;

    public int Id => _id.RuntimeId;
    public ulong Version => _version;

#if UNITY_EDITOR
    public string Guid => _id.Guid;

    protected virtual void OnEnable()
    {
        _version = 0;
        _id.Validate(this);
    }

    protected virtual void Reset()
    {
        _version = 0;
        _id.Validate(this);
    }

    public void Validate()
    {
        _version++;
        _id.Validate(this);
    }
#endif
}