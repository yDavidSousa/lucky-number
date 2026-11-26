using UnityEngine;
using Random = UnityEngine.Random;

public enum DiceAnimationState : byte
{
    Idle,
    Rolling,
    Finished
}

public sealed class DieView : MonoBehaviour
{
    [Header("Face Normals")]
    [SerializeField] private Vector3[] _faceNormals =
    {
        Vector3.up,
        Vector3.right,
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.down
    };

    [Header("Dimensions")]
    [SerializeField] private Vector3 _halfExtents = new(0.5f, 0.5f, 0.5f);

    [Header("Animation")]
    [SerializeField] private Vector2 _duration = new(1.4f, 1.9f);
    [SerializeField] private Vector2 _spinCount = new(3f, 5f);
    [SerializeField] private Vector2 _bounceHeight = new(0.3f, 0.6f);

    [Header("Bounce")]
    [SerializeField, Range(0.1f, 0.8f)] private float _firstImpactTime = 0.4f;
    [SerializeField, Range(0f, 1f)] private float _bounceDecay = 0.4f;
    [SerializeField, Range(1, 5)] private int _bounceCount = 3;

    [Header("Collision Reaction")]
    [SerializeField] private float _collisionDisplacement = 0.15f;
    [SerializeField] private float _collisionRotation = 25f;
    [SerializeField] private float _collisionRecovery = 8f;

    private DiceAnimationState _state;

    private int _targetValue;
    private float _elapsed;
    private float _durationValue;
    private float _bounceHeightValue;
    private float _spinAngle;
    private float _tableHeight;

    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private Vector3 _spinAxis;

    private Quaternion _startRotation;
    private Quaternion _endRotation;

    private Vector3 _collisionOffset;
    private Quaternion _collisionRotationOffset = Quaternion.identity;

    public bool IsFinished => _state == DiceAnimationState.Finished;
    public bool IsRolling => _state == DiceAnimationState.Rolling;

    public int Value => _targetValue;

    public float CollisionRadius => _halfExtents.magnitude;

    public void Throw(int targetValue, Vector3 startPosition, Vector3 endPosition, float tableHeight)
    {
        _targetValue = Mathf.Clamp(targetValue, 1, 6);

        _startPosition = startPosition;
        _endPosition = endPosition;
        _tableHeight = tableHeight;

        _startRotation = Random.rotation;
        _endRotation = GetFaceRotation(_targetValue);

        _durationValue = Random.Range(_duration.x, _duration.y);
        _bounceHeightValue = Random.Range(_bounceHeight.x, _bounceHeight.y);
        _spinAngle = Random.Range(_spinCount.x, _spinCount.y) * 360f;
        _spinAxis = Random.onUnitSphere;

        _elapsed = 0f;
        _collisionOffset = Vector3.zero;
        _collisionRotationOffset = Quaternion.identity;

        _state = DiceAnimationState.Rolling;

        transform.SetPositionAndRotation(_startPosition, _startRotation);
    }

    private void Update()
    {
        if (_state != DiceAnimationState.Rolling)
            return;

        _elapsed += Time.deltaTime;

        float t = Mathf.Clamp01(_elapsed / Mathf.Max(0.01f, _durationValue));

        UpdateRotation(t);
        UpdatePosition(t);

        UpdateCollisionReaction(Time.deltaTime);

        if (t >= 1f)
        {
            _state = DiceAnimationState.Finished;

            transform.SetPositionAndRotation(
                new Vector3(
                    _endPosition.x,
                    _tableHeight +
                    GetSupportHeight(_endRotation),
                    _endPosition.z
                ),
                _endRotation
            );
        }
    }

    private void UpdatePosition(float t)
    {
        float horizontalT = 1f - Mathf.Pow(1f - t, 2f);

        Vector3 position = Vector3.Lerp(_startPosition, _endPosition, horizontalT);

        float supportHeight = GetSupportHeight(transform.rotation);

        if (t < _firstImpactTime)
        {
            float u = t / Mathf.Max(0.001f, _firstImpactTime);

            float startHeight = Mathf.Max(_startPosition.y, _tableHeight + supportHeight);
            float endHeight = _tableHeight + supportHeight;

            position.y = Mathf.Lerp(startHeight, endHeight, u);
            position.y += Mathf.Sin(u * Mathf.PI) * _bounceHeightValue;
        }
        else
        {
            position.y = _tableHeight + supportHeight + EvaluateBounce(t);
        }

        position += _collisionOffset;

        transform.position = position;
    }

    private void UpdateRotation(float t)
    {
        float spinT = Mathf.Pow(1f - t, 2f);
        Quaternion spin = Quaternion.AngleAxis(_spinAngle * spinT, _spinAxis);

        float alignmentT = t * t * (3f - 2f * t);
        Quaternion alignment = Quaternion.Slerp(_startRotation, _endRotation, alignmentT);

        transform.rotation = _collisionRotationOffset * spin * alignment;
    }

    private float EvaluateBounce(float t)
    {
        if (t < _firstImpactTime)
            return 0f;

        float remaining = 1f - _firstImpactTime;

        float totalWeight = 0f;
        float weight = 1f;

        for (int i = 0; i < _bounceCount; i++)
        {
            totalWeight += weight;
            weight *= 0.7f;
        }

        float current = _firstImpactTime;
        weight = 1f;

        for (int i = 0; i < _bounceCount; i++)
        {
            float segmentDuration = remaining * weight / totalWeight;
            float next = current + segmentDuration;

            if (t <= next || i == _bounceCount - 1)
            {
                float u = Mathf.Clamp01((t - current) / Mathf.Max(0.001f, segmentDuration));
                float height = _bounceHeightValue * Mathf.Pow(_bounceDecay, i + 1);

                return Mathf.Sin(u * Mathf.PI) * height;
            }

            current = next;
            weight *= 0.7f;
        }

        return 0f;
    }

    private float GetSupportHeight(Quaternion rotation)
    {
        Vector3 right = rotation * Vector3.right;
        Vector3 up = rotation * Vector3.up;
        Vector3 forward = rotation * Vector3.forward;

        return Mathf.Abs(right.y) * _halfExtents.x + Mathf.Abs(up.y) * _halfExtents.y + Mathf.Abs(forward.y) * _halfExtents.z;
    }

    private Quaternion GetFaceRotation(int value)
    {
        Vector3 normal = _faceNormals[value - 1].normalized;

        Quaternion alignment = Quaternion.FromToRotation(normal, Vector3.up);
        Quaternion yaw = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);

        return yaw * alignment;
    }

    public void ApplyCollision(Vector3 direction)
    {
        if (!IsRolling)
            return;

        direction.y = 0f;
        direction.Normalize();

        _collisionOffset += direction * _collisionDisplacement;

        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);
        Quaternion rotation = Quaternion.AngleAxis(_collisionRotation, rotationAxis);

        _collisionRotationOffset = rotation * _collisionRotationOffset;
    }

    private void UpdateCollisionReaction(float deltaTime)
    {
        float factor = 1f - Mathf.Exp(-_collisionRecovery * deltaTime);

        _collisionOffset = Vector3.Lerp(_collisionOffset, Vector3.zero, factor);
        _collisionRotationOffset = Quaternion.Slerp(_collisionRotationOffset, Quaternion.identity, factor);
    }

    public void ResolvePosition(Vector3 correction)
    {
        if (!IsRolling)
            return;

        transform.position += correction;
        _collisionOffset += correction;
    }
}