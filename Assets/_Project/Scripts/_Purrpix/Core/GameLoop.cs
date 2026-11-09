using System.Collections;

namespace Purrpix.Core
{
    public static class GameLoopDefaults
    {
        public static void EmptyUpdate(float dt) { }
        public static void Empty() { }
        public static IEnumerator EmptyEnumerator() { yield break; }
    }

    public class GameLoop : UnityEngine.MonoBehaviour
    {
        public delegate void EmptyDelegate();
        public delegate void UpdateDelegate(float dt);
        public delegate IEnumerator EnumeratorDelegate();

        private static EnumeratorDelegate _initialize = GameLoopDefaults.EmptyEnumerator;
        private static EmptyDelegate _shutdown = GameLoopDefaults.Empty;

        private static UpdateDelegate _fixedUpdate = GameLoopDefaults.EmptyUpdate;
        private static UpdateDelegate _update = GameLoopDefaults.EmptyUpdate;
        private static UpdateDelegate _lateUpdate = GameLoopDefaults.EmptyUpdate;

        private static EmptyDelegate _drawGizmos = GameLoopDefaults.Empty;

        private static GameLoop _instance;

        public static void Initialize(EnumeratorDelegate initialize, EmptyDelegate shutdown, UpdateDelegate update = null, UpdateDelegate lateUpdate = null, UpdateDelegate fixedUpdate = null, EmptyDelegate drawGizmos = null)
        {
            _initialize = initialize ?? _initialize;
            _shutdown = shutdown ?? _shutdown;

            _update = update ?? _update;
            _lateUpdate = lateUpdate ?? _lateUpdate;
            _fixedUpdate = fixedUpdate ?? _fixedUpdate;

            _drawGizmos = drawGizmos ?? _drawGizmos;

            _instance = new UnityEngine.GameObject(nameof(GameLoop), typeof(GameLoop)).GetComponent<GameLoop>();
            DontDestroyOnLoad(_instance);
        }

        private IEnumerator Start()
        {
            yield return _initialize();
        }

        private void OnDestroy()
        {
            _shutdown();
        }

        private void Update()
        {
            _update(UnityEngine.Time.deltaTime);
        }

        private void LateUpdate()
        {
            _lateUpdate(UnityEngine.Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _fixedUpdate(UnityEngine.Time.fixedDeltaTime);
        }

        private void OnApplicationQuit()
        {
            
        }

        public static UnityEngine.Coroutine Run(IEnumerator routine)
        {
            return _instance.StartCoroutine(routine);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            _drawGizmos();
        }
#endif
    }
}