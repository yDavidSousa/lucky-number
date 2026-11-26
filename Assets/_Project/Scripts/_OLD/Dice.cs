using UnityEngine;

public class Dice : MonoBehaviour
{
    [SerializeField] private Rigidbody Rigidbody;

    private static readonly Vector3[] FaceDirections =
    {
        Vector3.up,
        Vector3.down,
        Vector3.forward,
        Vector3.back,
        Vector3.right,
        Vector3.left
    };

    private static readonly int[] FaceValues =
    {
        1, 3, 6, 5, 4, 2
    };

    public bool IsStopped => Rigidbody.IsSleeping();

    public int GetValue()
    {
        float bestDot = float.NegativeInfinity;
        int result = 0;

        for (int i = 0; i < 6; i++)
        {
            Vector3 direction = transform.TransformDirection(FaceDirections[i]);
            float dot = Vector3.Dot(direction, Vector3.up);
            if (dot > bestDot)
            {
                bestDot = dot;
                result = FaceValues[i];
            }
        }

        return result;
    }

    public void Roll(Vector3 force, Vector3 torque)
    {
        Rigidbody.WakeUp();
        Rigidbody.AddForce(force, ForceMode.Impulse);
        Rigidbody.AddTorque(torque, ForceMode.Impulse);
    }

    public void Throw(Vector3 spawnPosition, Vector3 direction, float force, float torque)
    {
        Rigidbody.position = spawnPosition;
        Rigidbody.rotation = Random.rotation;

        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;

        Rigidbody.WakeUp();

        Rigidbody.AddForce(
            direction.normalized * force,
            ForceMode.Impulse
        );

        Rigidbody.AddTorque(
            Random.onUnitSphere * torque,
            ForceMode.Impulse
        );
    }
}