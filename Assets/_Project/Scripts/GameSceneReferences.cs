using System;
using System.Collections;
using Purrpix.Camera;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct SceneCameraSettings
{
    public float distance;
    public float orthographicSize;
    public Vector2 framing;
    public float yaw;
    public float pitch;
}

[System.Serializable]
public class DiceSettings
{
    public Dice Prefab;
    public Transform SpawnPoint;
    public Vector2 ThrowForce;
    public Vector2 ThrowTorque;
}

[System.Serializable]
public struct ModifierReferences
{
    public Transform[] slots;
}

[System.Serializable]
public struct ContractReferences
{
    public Transform[] slots;
}

public enum ScreenId : byte
{
    HUD,
    GameOver,
    Shop,
    Contracts,
    Count
}

[ExecuteInEditMode]
public class GameSceneReferences : SceneReferences
{
    public Camera WorldCamera;

    [Space]
    public DiceSettings DiceSettings;

    [Header("Contract")]
    public SpawnAreaReferences ContractArea;

    [Header("Shop")]
    public SpawnAreaReferences ShopLuckyCharmsArea;

    [Header("HUD")]
    public SpawnAreaReferences PlayerDiceRollsArea;
    public SpawnAreaReferences PlayerCurrencyArea;
    public SpawnAreaReferences PlayerLuckyCharmsArea;

    [Header("Interface")]
    public GameplayHUDView GameplayHUD;
    public ShopView Shop;
    public ContractView Contract;
    public GameOverView GameOver;
    public DialogueView Dialogue;

    [Header("Debug")]
    public SceneCameraSettings CameraSettings;

#if UNITY_EDITOR
    private CameraController _camera;

    private void LateUpdate()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }

        if (_camera == null)
        {
            _camera = new CameraController(WorldCamera);

            CameraDOFData sceneDof = new CameraDOFData
            {
                distance = CameraSettings.distance,
                orthographicSize = CameraSettings.orthographicSize,
                framing = CameraSettings.framing,
                pitch = CameraSettings.pitch,
                yaw = CameraSettings.yaw
            };
            CameraController.SetDegreesOfFreedom(_camera, sceneDof);
        }

        CameraController.LateTick(_camera, Vector3.zero, Vector2.zero);
    }

    private void OnValidate()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }

        if (_camera != null)
        {
            CameraDOFData sceneDof = new CameraDOFData
            {
                distance = CameraSettings.distance,
                orthographicSize = CameraSettings.orthographicSize,
                framing = CameraSettings.framing,
                pitch = CameraSettings.pitch,
                yaw = CameraSettings.yaw
            };
            CameraController.SetDegreesOfFreedom(_camera, sceneDof);
        }
    }
#endif
}

public abstract class SceneReferences : MonoBehaviour
{
}

public static class SceneSystem
{
    public static T GetSceneReferences<T>(Scene scene) where T : SceneReferences
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (!root.CompareTag("References/Scene"))
                continue;

            if (root.TryGetComponent(out T references))
                return references;

            references = root.GetComponentInChildren<T>();
            if (references != null)
                return references;
        }

        throw new InvalidOperationException($"Scene '{scene.name}' has no {typeof(T).Name}.");
    }

    public static IEnumerator Load(string sceneName, LoadSceneMode mode)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
        yield return operation;
    }

    public static IEnumerator Unload(string sceneName)
    {
        AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

        if (operation != null)
            yield return operation;
    }
}