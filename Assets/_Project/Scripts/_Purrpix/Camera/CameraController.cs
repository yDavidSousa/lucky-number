namespace Purrpix.Camera
{
    using UnityEngine;

    public sealed class CameraController
    {
        private readonly Camera _camera;

        private CameraAxisData _yawProperties;
        private CameraAxisData _pitchProperties;

        private bool _blending;
        private CameraDOFData _blendFrom;
        private CameraDOFData _blendTo;
        private float _blendTime;
        private float _blendDuration;

        private bool _shaking;
        private float _shakeTime;
        private float _shakeDuration;
        private float _shakeAmplitude;
        private float _shakeFrequency;

        private bool _custom;
        private CameraDOFData _customDOF;
        private CameraDOFData _orbitDOF;

        private CameraDOFData _currentDOF;
        private CameraPOVData _currentPOV;

        public Camera Main => _camera;

        public CameraController(Camera camera)
        {
            _camera = camera;
        }

        private CameraPOVData SolvePointOfView(Vector3 tracking, float distance, float yaw, float pitch, Vector2 framing)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0.0f);

            if (_camera.orthographic)
            {
                float orthoHeight = _camera.orthographicSize * 2f;
                float orthoWidth = orthoHeight * _camera.aspect;

                // Offset in camera space (screen-aligned)
                Vector3 right = rotation * Vector3.right;
                Vector3 up = rotation * Vector3.up;
                Vector3 forward = rotation * Vector3.forward;

                Vector3 framingOffset = right * (framing.x * orthoWidth * 0.5f) + up * (framing.y * orthoHeight * 0.5f);
                Vector3 depthOffset = -forward * distance;
                Vector3 position = tracking + framingOffset + depthOffset;

                return new CameraPOVData
                {
                    position = position,
                    rotation = rotation
                };
            }
            else
            {
                //Convert the screen-space framing into a world-space "Parallax" offset
                //using the camera's field of view and the screen's aspect ratio.
                float tanFov = Mathf.Tan(Mathf.Deg2Rad * _camera.fieldOfView * 0.5f);
                Vector2 screenToWorld = distance * new Vector2(tanFov, tanFov / _camera.aspect);
                Vector2 parallax = screenToWorld * framing;

                //Pull-back in the local forward/backward direction(x) and parallax in the side-to-side directions(yz).
                Vector3  localOffset = new Vector3(parallax.x, parallax.y, -distance);

                return new CameraPOVData
                {
                    position = tracking + rotation * localOffset,
                    rotation = rotation
                };
            }
        }

        public static void LateTick(CameraController camera, Vector3 tracking, Vector2 input)
        {
            // Blend Point of View
            if (camera._blending)
            {
                camera._blendTime += Time.deltaTime;

                float t = Mathf.Clamp01(camera._blendTime / camera._blendDuration);
                float k = Mathf.SmoothStep(0, 1, t);

                camera._currentDOF.distance = Mathf.Lerp(camera._blendFrom.distance, camera._blendTo.distance, k);
                camera._currentDOF.orthographicSize = Mathf.Lerp(camera._blendFrom.orthographicSize, camera._blendTo.orthographicSize, k);
                camera._currentDOF.yaw = Mathf.LerpAngle(camera._blendFrom.yaw, camera._blendTo.yaw, k);
                camera._currentDOF.pitch = Mathf.LerpAngle(camera._blendFrom.pitch, camera._blendTo.pitch, k);
                camera._currentDOF.framing = Vector2.Lerp(camera._blendFrom.framing, camera._blendTo.framing, k);
                camera._currentDOF.tracking = Vector3.Lerp(camera._blendFrom.tracking, camera._blendTo.tracking, k);

                if (t >= 1f)
                {
                    camera._blending = false;
                }
            }
            else
            {
                if (camera._custom)
                {
                    camera._currentDOF = camera._customDOF;
                }
                else
                {
                    // Orbit yaw & pitch inputs
                    if (input != Vector2.zero)
                    {
                        int yawDirection = camera._yawProperties.inverted ? 1 : -1;
                        camera._orbitDOF.yaw = (camera._currentDOF.yaw + input.x * (camera._yawProperties.speed * 2 / 10) * yawDirection) % 360;

                        int pitchDirection = camera._pitchProperties.inverted ? -1 : 1;
                        camera._orbitDOF.pitch = (camera._currentDOF.pitch + input.y * (camera._pitchProperties.speed * 2 / 10) * pitchDirection) % 360;
                    }
                    camera._orbitDOF.tracking = tracking;

                    camera._currentDOF = camera._orbitDOF;
                }
            }

            if (camera._camera.orthographic)
            {
                camera._camera.orthographicSize = camera._currentDOF.orthographicSize;
            }
            camera._currentPOV = camera.SolvePointOfView(camera._currentDOF.tracking, camera._currentDOF.distance, camera._currentDOF.yaw, camera._currentDOF.pitch, camera._currentDOF.framing);

            // Apply Shake
            if (camera._shaking)
            {
                camera._shakeTime += Time.deltaTime;

                float t = Mathf.Clamp01(camera._shakeTime / camera._shakeDuration);
                float falloff = (1f - t) * (1f - t);

                float noiseTime = Time.time * camera._shakeFrequency;

                Vector3 offset = new Vector3(
                    Mathf.PerlinNoise(noiseTime, 0) - 0.5f,
                    Mathf.PerlinNoise(0, noiseTime) - 0.5f,
                    0
                );

                offset *= camera._shakeAmplitude * falloff;

                camera._currentPOV.position += camera._currentPOV.rotation * offset;

                if (t >= 1f)
                {
                    camera._shaking = false;
                }
            }

            camera._camera.transform.SetLocalPositionAndRotation(camera._currentPOV.position, camera._currentPOV.rotation);
        }

        public static void SetDegreesOfFreedom(CameraController camera, CameraDOFData dof)
        {
            camera._orbitDOF.distance = dof.distance;
            camera._orbitDOF.orthographicSize = dof.orthographicSize;
            camera._orbitDOF.framing = dof.framing;
            camera._orbitDOF.yaw = dof.yaw;
            camera._orbitDOF.pitch = dof.pitch;
            camera._orbitDOF.tracking = dof.tracking;

            if (camera._custom == false)
            {
                camera._currentDOF = camera._orbitDOF;
            }
        }

        public static void SetYawAxisProperties(CameraController camera, bool inverted, float speed)
        {
            camera._yawProperties.inverted = inverted;
            camera._yawProperties.speed = speed;
        }

        public static void SetPitchAxisProperties(CameraController camera, bool inverted, float speed)
        {
            camera._pitchProperties.inverted = inverted;
            camera._pitchProperties.speed = speed;
        }

        public static void SetZoom(CameraController camera, float zoom)
        {
            if (camera._camera.orthographic)
            {
                camera._camera.orthographicSize = zoom;
            }
            else
            {
                camera._orbitDOF.distance = zoom;
            }
        }

        public static void PlayShake(CameraController camera, CameraShakeData shake)
        {
            camera._shakeAmplitude = shake.amplitude;
            camera._shakeFrequency = shake.frequency;
            camera._shakeDuration = shake.duration;

            camera._shakeTime = 0f;
            camera._shaking = true;
        }

        public static void EnableCustom(CameraController camera, CameraDOFData dof, CameraBlendData blend)
        {
            camera._customDOF = dof;

            camera._blendFrom = camera._currentDOF;
            camera._blendTo = camera._customDOF;

            camera._blendTime = 0f;
            camera._blendDuration = blend.duration;
            camera._blending = true;
            camera._custom = true;
        }

        public static void DisableCustom(CameraController camera, CameraBlendData blend)
        {
            camera._blendFrom = camera._currentDOF;
            camera._blendTo = camera._orbitDOF;

            camera._blendTime = 0f;
            camera._blendDuration = blend.duration;
            camera._blending = true;
            camera._custom = false;
        }
    }
}