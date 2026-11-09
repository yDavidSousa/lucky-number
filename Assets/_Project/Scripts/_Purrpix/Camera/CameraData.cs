namespace Purrpix.Camera
{
    using UnityEngine;

    public struct CameraAxisData
    {
        public bool inverted;
        public float speed;
    }

    public struct CameraDOFData
    {
        public Vector3 tracking;
        public Vector2 framing;
        public float distance;
        public float yaw;
        public float pitch;

        public float orthographicSize;
    }

    public struct CameraPOVData
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public struct CameraShakeData
    {
        public float amplitude;
        public float frequency;
        public float duration;
    }

    public struct CameraBlendData
    {
        public float duration;
    }
}