using System.Collections;
using Purrpix.Camera;
using Purrpix.Core;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public struct DiceResult
{
    public int DieA;
    public int DieB;
    public int BaseScore;
    public int FinalScore;
    public LuckyMatchType MatchType;
}

public enum GamePhase : byte
{
    None,
    Preparation,
    Rolling,
    Modifying,
    Result,
    ContractSelection,
    Shop,
    Boss,
    GameOver
}

public sealed class GameState
{
    public GamePhase Phase;
    public GamePhase? PendingPhase;

    public int Stage;
    public int StageScore;
    public int RollsRemaining;

    public int LuckyNumber;
    public int Currency;
    
    public int LossStreak;

    public StageSettings StageSettings;
    public int BossHealth;

    public ContractData ActiveContract;
    public readonly ContractData[] ContractOffers = new ContractData[2];

    public const int MaxModifiers = 3;
    public ModifierType[] EquippedModifiers = new ModifierType[MaxModifiers];
    public ModifierData[] ShopOffers = new ModifierData[3];
    public bool[] ShopPurchased = new bool[3];

    public DiceResult DiceResult;
}

public static class Game
{
    private static GameState _state;
    private static LuckyNumberGenerator _generator;
    private static bool _initialized;

    private static CameraController _cameraController;
    private static Dice[] _dices;
    private static DiceSettings _diceSettings;
    private static int _physicsStepsSinceRoll;

    // User Interface (UI)
    private static GameplayHUDController _gameplayHUDController;
    private static ShopController _shopController;
    private static GameOverController _gameOverController;

    private static IEnumerator Initialize()
    {
        // Game Data
        GameData.Initialize();
        _state = new GameState();
        _generator = new LuckyNumberGenerator();

        StartupSceneReferences startupReferences = Object.FindAnyObjectByType<StartupSceneReferences>();

        _cameraController = new CameraController(startupReferences.WorldCamera);
        CameraController.SetDegreesOfFreedom(_cameraController, new CameraDOFData
        {
            distance = startupReferences.CameraSettings.distance,
            framing = startupReferences.CameraSettings.framing,
            orthographicSize = startupReferences.CameraSettings.orthographicSize,
            pitch = startupReferences.CameraSettings.pitch,
            yaw = startupReferences.CameraSettings.yaw
        });

        _diceSettings = startupReferences.DiceSettings;
        _dices = new Dice[2];
        for (int i = 0; i < _dices.Length; i++)
        {
            Dice dice = Object.Instantiate(_diceSettings.Prefab, _diceSettings.SpawnPoint.localPosition, Quaternion.identity);
            dice.gameObject.SetActive(false);
            _dices[i] = dice;
        }

        yield return null;
        _gameplayHUDController = new GameplayHUDController(startupReferences.GameplayHUD, _state);
        _shopController = new ShopController(startupReferences.Shop);
        _gameOverController = new GameOverController(startupReferences.GameOver);

        _state.Stage = 1;
        StartStage();
        _initialized = true;
    }

    private static void Shutdown()
    {
        _initialized = false;
    }

    private static void FixedUpdate(float deltaTime)
    {
        if (!_initialized) return;

        if (_state.Phase == GamePhase.Rolling)
            _physicsStepsSinceRoll++;
    }

    private static void Update(float deltaTime)
    {
        if (!_initialized) return;

        switch (_state.Phase)
        {
            case GamePhase.Rolling:
            {
                if (_physicsStepsSinceRoll < 2)
                    return;

                bool allStopped = true;
                foreach (Dice dice in _dices)
                {
                    if (!dice.IsStopped)
                    {
                        allStopped = false;
                        break;
                    }
                }

                if (allStopped)
                {
                    _state.DiceResult = new DiceResult()
                    {
                        DieA = _dices[0].GetValue(),
                        DieB = _dices[1].GetValue(),
                        BaseScore = 100
                    };
                    _state.PendingPhase = GamePhase.Result;
                }
                break;
            }
        }
    }

    private static void LateUpdate(float deltaTime)
    {
        if (!_initialized) return;

        // User Interface (UI)
        {
            _gameplayHUDController.LateTick();
        }

        CameraController.LateTick(_cameraController, Vector3.zero, Vector2.zero);

        if (_state.PendingPhase.HasValue)
        {
            GamePhase nextPhase = _state.PendingPhase.Value;
            _state.PendingPhase = null;

            ChangeStagePhase(nextPhase);
        }
    }

    private static void DrawGizmos()
    {
    }

    private static void StartStage()
    {
        StageSettings settings = StageProgression.GetSettings(_state.Stage);

        int targetScore = settings.TargetScore;
        int maxRolls = settings.MaxRolls;

        ContractSystem.ApplyStageRules(_state.ActiveContract, ref targetScore, ref maxRolls);

        settings.TargetScore = targetScore;
        settings.MaxRolls = maxRolls;

        _state.StageScore = 0;
        _state.StageSettings = settings;
        _state.RollsRemaining = settings.MaxRolls;

        _state.PendingPhase = GamePhase.Preparation;
    }

    private static void ChangeStagePhase(GamePhase nextPhase)
    {
        switch (nextPhase)
        {
            case GamePhase.Preparation:
            {
                _state.LuckyNumber = _generator.Generate(
                    difficulty: 0.5f,
                    lossStreak: _state.LossStreak
                );

                _gameplayHUDController.Hydrate(_state);

                break;
            }
            case GamePhase.Rolling:
            {
                RollDices();
                break;
            }
            case GamePhase.Result:
            {
                int finalScore = EvaluateResult();

                CompleteRoll(finalScore);

                _gameplayHUDController.Hydrate(_state);

                break;
            }
            case GamePhase.Shop:
            {
                _shopController.Hydrate(_state, LeaveShop, BuyModifier);
                break;
            }
            case GamePhase.ContractSelection:
            {
                int contractIndex = Random.Range(0, _state.ContractOffers.Length);
                SelectContract(contractIndex);
                break;
            }
            case GamePhase.GameOver:
            {
                _gameOverController.Hydrate();
                break;
            }
        }

        _state.Phase = nextPhase;
    }

    private static void CompleteRoll(int score)
    {
        score = ContractSystem.ApplyScorePenalty(_state.ActiveContract, score);

        _state.StageScore = Mathf.Max(0, _state.StageScore + score);
        _state.RollsRemaining--;

        if (_state.StageScore >= _state.StageSettings.TargetScore)
        {
            CompleteStage();
        }
        else if (_state.RollsRemaining <= 0)
        {
            _state.PendingPhase = GamePhase.GameOver;
        }
        else
        {
            _state.PendingPhase = GamePhase.Preparation;
        }
    }

    private static void CompleteStage()
    {
        const int baseReward = 5;
        const int bonusPerRemainingRoll = 2;

        _state.Currency += baseReward + _state.RollsRemaining * bonusPerRemainingRoll;

        if (StageProgression.IsActComplete(_state.Stage))
        {
            _state.Currency += _state.ActiveContract.Reward;
            _state.ActiveContract = default;
        }

        ShopSystem.GenerateOffers(_state.ShopOffers, _state.ShopPurchased);

        _state.PendingPhase = GamePhase.Shop;
    }

    private static void LeaveShop()
    {
        if (StageProgression.IsRunComplete(_state.Stage))
        {
            _state.PendingPhase = GamePhase.Boss;
            return;
        }

        bool actComplete = StageProgression.IsActComplete(_state.Stage);
        _state.Stage++;

        if (actComplete)
        {
            _state.ActiveContract = default;
            ContractSystem.GenerateOffers(_state.ContractOffers);

            _state.PendingPhase = GamePhase.ContractSelection;
            return;
        }
            
        StartStage();
    }

    private static void BuyModifier(int offerIndex)
    {
        bool success = ShopSystem.TryBuy(_state, offerIndex);
        if (success)
        {
            _shopController.Hydrate(_state);
        }
    }

    public static void SelectContract(int index)
    {
        if (_state.Phase != GamePhase.ContractSelection)
        {
            return;
        }

        if (index < 0 || index >= _state.ContractOffers.Length)
        {
            return;
        }

        _state.ActiveContract = _state.ContractOffers[index];
        StartStage();
    }

    private static void RollDices()
    {
        _physicsStepsSinceRoll = 0;
        float offset = Random.Range(0.5f, 1.0f);
        Vector3[] spawnPoint = {
            _diceSettings.SpawnPoint.position - (_diceSettings.SpawnPoint.right * offset), 
            _diceSettings.SpawnPoint.position + (_diceSettings.SpawnPoint.right * offset)
        };
        for (int i = 0; i < _dices.Length; i++)
        {
            Dice dice = _dices[i];
            Vector3 position = spawnPoint[i];
            float force = Random.Range(_diceSettings.ThrowForce.x, _diceSettings.ThrowForce.y);
            float torque = Random.Range(_diceSettings.ThrowTorque.x, _diceSettings.ThrowTorque.y);

            dice.transform.SetPositionAndRotation(position, _diceSettings.SpawnPoint.rotation);
            dice.gameObject.SetActive(true);
            dice.Throw(_diceSettings.SpawnPoint.localPosition, _diceSettings.SpawnPoint.forward, force, torque);
        }
    }

    private static int EvaluateResult()
    {
        DiceResult diceResult = _state.DiceResult;

        int dieA = diceResult.DieA;
        int dieB = diceResult.DieB;

        ModifierSystem.ApplyDiceModifiers(
            _state.EquippedModifiers,
            _state.LuckyNumber,
            ref dieA,
            ref dieB
        );

        LuckyResult luckyResult = LuckyNumberEvaluator.Evaluate(
            _state.LuckyNumber,
            diceResult.DieA,
            diceResult.DieB,
            diceResult.BaseScore
        );

        int finalScore = ModifierSystem.ApplyScoreModifiers(
            _state.EquippedModifiers,
            luckyResult.MatchType,
            luckyResult.Score,
            _state.RollsRemaining
        );

        diceResult.FinalScore = finalScore;
        diceResult.MatchType = luckyResult.MatchType;

        _state.DiceResult = diceResult;

        if (luckyResult.MatchType == LuckyMatchType.None)
            _state.LossStreak++;
        else
            _state.LossStreak = 0;

        Debug.Log(
            $"Lucky: {_state.LuckyNumber} | " +
            $"Dados: {diceResult.DieA}, {diceResult.DieB} | " +
            $"Acerto: {diceResult.MatchType} | " +
            $"Pontos: {diceResult.FinalScore}"
        );
        return finalScore;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EntryPoint()
    {
        GameLoop.Initialize(Initialize, Shutdown, Update, LateUpdate, FixedUpdate, DrawGizmos);
    }
}