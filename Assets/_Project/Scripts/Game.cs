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

    public ContractDefData ActiveContract; // change to id
    public ContractDefData[] ContractOffers = new ContractDefData[2];  // change to id

    public const int MaxModifiers = 3;
    public ModifierType[] EquippedModifiers = new ModifierType[MaxModifiers];  // change to id
    public LuckyCharmDefData[] ShopOffers = new LuckyCharmDefData[3];  // change to id
    public bool[] ShopPurchased = new bool[3];

    public DiceResult DiceResult;

    public void Reset()
    {
        Phase = GamePhase.None;
        PendingPhase = null;

        Stage = 0;
        StageScore = 0;
        RollsRemaining = 0;

        LuckyNumber = 0;
        Currency = 0;

        LossStreak = 0;

        StageSettings = default;
        BossHealth = 0;

        ActiveContract = default;
        ContractOffers = new ContractDefData[2];

        EquippedModifiers = new ModifierType[MaxModifiers];
        ShopOffers = new LuckyCharmDefData[3];
        ShopPurchased = new bool[3];

        DiceResult = new DiceResult();
    }
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
    private static Transform[] _inventorySlots;

    //Systems
    private static ScreenSystem _screenSystem;
    private static GameFlowSystem _flowSystem;
    private static ShopSystem _shopSystem;
    private static LuckyCharmSystem _luckyCharmSystem;
    private static ContractSystem _contractSystem;
    private static DialogueSystem _dialogueSystem;

    // User Interface (UI)
    private static GameplayHUDController _gameplayHUDController;
    private static ShopController _shopController;
    private static ContractController _contractController;
    private static GameOverController _gameOverController;
    private static DialogueController _dialogueController;

    private static IEnumerator Initialize()
    {
        // Game Data
        GameData.Initialize();
        _state = new GameState {Stage = 1};
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

        ScreenView[] screens = new ScreenView[(int)ScreenId.Count];
        {
            screens[(int)ScreenId.HUD] = startupReferences.GameplayHUD.Screen;
            screens[(int)ScreenId.GameOver] = startupReferences.GameOver.Screen;
            screens[(int)ScreenId.Shop] = startupReferences.Shop.Screen;
            screens[(int)ScreenId.Contracts] = startupReferences.Contract.Screen;   
        }
        _screenSystem = new ScreenSystem(screens);
        _shopSystem = new ShopSystem(GameData.LuckyCharmDefRegistry);
        _contractSystem = new ContractSystem(GameData.ContractDefRegistry);
        _luckyCharmSystem = new LuckyCharmSystem();
        _dialogueSystem = new DialogueSystem(GameData.DialogueDefAssets);

        yield return null;

        _inventorySlots = startupReferences.InventorySlots;
        _gameplayHUDController = new GameplayHUDController(startupReferences.GameplayHUD, () => _flowSystem.Enqueue(FlowAction.SetPhase(GamePhase.Rolling)));
        _shopController = new ShopController(_state, startupReferences.Shop, startupReferences.WorldCamera, startupReferences.ModifierReferences, LeaveShop, BuyModifier);
        _contractController = new ContractController(_state, startupReferences.Contract, startupReferences.ContractReferences, SelectContract);
        _gameOverController = new GameOverController(startupReferences.GameOver, Restart);
        _dialogueController = new DialogueController(_dialogueSystem, startupReferences.Dialogue);

        _flowSystem = new GameFlowSystem(_screenSystem, _dialogueController, _state, true);

        yield return null;

        _flowSystem.Enqueue(FlowAction.Speak(DialogueTrigger.FirstEncounter));
        _flowSystem.Enqueue(FlowAction.Show(GameplayHUDView.Id));
        _flowSystem.Enqueue(FlowAction.Wait(0.5f));

        StartStage();
        _initialized = true;
    }

    private static void Shutdown()
    {
        GameData.Dispose();

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
        
        _screenSystem.Tick(deltaTime);
        {
            _gameplayHUDController.Tick(deltaTime);
            _dialogueController.Tick(deltaTime);
        }
        _flowSystem.Tick(deltaTime);

        if (_flowSystem.IsBusy)
        {
            return;
        }

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
                    _flowSystem.Enqueue(FlowAction.SetPhase(GamePhase.Result));
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
            _shopController.LateTick(_state.ShopOffers);
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

        _contractSystem.ApplyStageRules(_state.ActiveContract, ref targetScore, ref maxRolls);

        settings.TargetScore = targetScore;
        settings.MaxRolls = maxRolls;

        _state.StageScore = 0;
        _state.StageSettings = settings;
        _state.RollsRemaining = settings.MaxRolls;

        _gameplayHUDController.Hydrate(_state);
        _state.LuckyNumber = _generator.Generate(difficulty: 0.5f, lossStreak: _state.LossStreak);
        _flowSystem.Enqueue(FlowAction.Speak(DialogueTrigger.LuckyNumberRevealed));
        _flowSystem.Enqueue(FlowAction.SetPhase(GamePhase.Preparation));
    }

    private static void ChangeStagePhase(GamePhase nextPhase)
    {
        switch (nextPhase)
        {
            case GamePhase.Preparation:
            {
                _gameplayHUDController.SpinLuckyNumber(_state.LuckyNumber);
                break;
            }
            case GamePhase.Rolling:
            {
                _state.RollsRemaining--;
                RollDices();
                _gameplayHUDController.Hydrate(_state);
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
                _flowSystem.Enqueue(FlowAction.Speak(DialogueTrigger.ShopOpened));

                for (int i = 0; i < _dices.Length; i++)
                {
                    Dice dice = _dices[i];
                    dice.gameObject.SetActive(false);
                }

                _shopController.SetOffers(_state);
                break;
            }
            case GamePhase.ContractSelection:
            {
                _flowSystem.Enqueue(FlowAction.Hide(GameplayHUDView.Id));

                for (int i = 0; i < _dices.Length; i++)
                {
                    Dice dice = _dices[i];
                    dice.gameObject.SetActive(false);
                }

                _contractController.SetContracts(_state.ContractOffers);
                _flowSystem.Enqueue(FlowAction.Show(ContractView.Id));
                break;
            }
            case GamePhase.GameOver:
            {
                _flowSystem.Enqueue(FlowAction.Hide(GameplayHUDView.Id));
                _flowSystem.Enqueue(FlowAction.Show(GameOverView.Id));
                break;
            }
        }

        _state.Phase = nextPhase;
    }

    private static void CompleteRoll(int score)
    {
        score = _contractSystem.ApplyScorePenalty(_state.ActiveContract, score);

        _state.StageScore = Mathf.Max(0, _state.StageScore + score);

        if (_state.StageScore >= _state.StageSettings.TargetScore)
        {
            CompleteStage();
        }
        else if (_state.RollsRemaining <= 0)
        {
            _flowSystem.Enqueue(FlowAction.SetPhase(GamePhase.GameOver));
        }
        else
        {
            _state.LuckyNumber = _generator.Generate(difficulty: 0.5f, lossStreak: _state.LossStreak);
            _flowSystem.Enqueue(FlowAction.Speak(DialogueTrigger.LuckyNumberRevealed));
            _flowSystem.Enqueue(FlowAction.SetPhase(GamePhase.Preparation));
        }
    }

    private static void CompleteStage()
    {
        const int baseReward = 5;
        const int bonusPerRemainingRoll = 2;

        _state.Currency += baseReward + _state.RollsRemaining * bonusPerRemainingRoll;

        if (StageProgression.IsActComplete(_state.Stage))
        {
            _state.Currency += _state.ActiveContract.reward;
            _state.ActiveContract = default;
        }

        _shopSystem.GenerateOffers(_state.ShopOffers, ref _state.ShopPurchased);

        _flowSystem.Enqueue(FlowAction.Wait(0.5f));
        _flowSystem.Enqueue(FlowAction.Hide(GameplayHUDView.Id));
        _flowSystem.Enqueue(FlowAction.Show(ShopView.Id));
        _flowSystem.Enqueue(FlowAction.SetPhase(GamePhase.Shop));
    }

    private static void LeaveShop()
    {
        _flowSystem.Enqueue(FlowAction.Hide(ShopView.Id));
        _flowSystem.Enqueue(FlowAction.Show(GameplayHUDView.Id));

        if (StageProgression.IsRunComplete(_state.Stage))
        {
            _flowSystem.Enqueue(FlowAction.SetPhase(GamePhase.Boss));
            return;
        }

        bool actComplete = StageProgression.IsActComplete(_state.Stage);
        _state.Stage++;

        if (actComplete)
        {
            _state.ActiveContract = default;
            _contractSystem.GenerateOffers(_state.ContractOffers);

            _flowSystem.Enqueue(FlowAction.SetPhase(GamePhase.ContractSelection));
            return;
        }
            
        StartStage();
    }

    private static void BuyModifier(int offerIndex)
    {
        bool success = _shopSystem.TryBuy(_state, offerIndex, out int slot);
        if (success)
        {
            _shopController.Hydrate(_state);

            LuckyCharmDefData luckyCharmData = _state.ShopOffers[offerIndex];
            GameObject luckyCharmView = Object.Instantiate(luckyCharmData.viewPrefab, _inventorySlots[slot]);
            luckyCharmView.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
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

        _flowSystem.Enqueue(FlowAction.Hide(ContractView.Id));
        _flowSystem.Enqueue(FlowAction.Show(GameplayHUDView.Id));
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

        _luckyCharmSystem.ApplyDiceModifiers(
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

        int finalScore = _luckyCharmSystem.ApplyScoreModifiers(
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

        DialogueTrigger trigger = luckyResult.MatchType switch
        {
            LuckyMatchType.SingleDie => DialogueTrigger.SingleMatch,
            LuckyMatchType.Sum => DialogueTrigger.SumMatch,
            LuckyMatchType.DoubleDice => DialogueTrigger.DoubleMatch,
            _ => DialogueTrigger.RollMissed
        };

        DialogueAct act = GetDialogueAct(_state.Stage, _state.Phase == GamePhase.Boss);
        _flowSystem.Enqueue(FlowAction.Speak(trigger));

        Debug.Log(
            $"Lucky: {_state.LuckyNumber} | " +
            $"Dados: {diceResult.DieA}, {diceResult.DieB} | " +
            $"Acerto: {diceResult.MatchType} | " +
            $"Pontos: {diceResult.FinalScore}"
        );
        return finalScore;
    }

    private static void Restart()
    {
        _flowSystem.Clear();

        _state.Reset();
        _state.Phase = GamePhase.GameOver;
        _state.Stage = 1;
        _generator = new LuckyNumberGenerator();

        for (int i = 0; i < _dices.Length; i++)
        {
            Dice dice = _dices[i];
            dice.gameObject.SetActive(false);
        }

        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            for (int j = 0; j < _inventorySlots[i].childCount; j++)
            {
                Transform luckyCharmView = _inventorySlots[i].GetChild(j);
                Object.Destroy(luckyCharmView.gameObject);
            }
        }

        _flowSystem.Enqueue(FlowAction.Hide(GameOverView.Id));
        _flowSystem.Enqueue(FlowAction.Show(GameplayHUDView.Id));
        StartStage();
    }

    public static DialogueAct GetDialogueAct(int stage, bool isBoss)
    {
        if (isBoss)
        {
            return DialogueAct.Boss;   
        }

        return stage switch
        {
            <= StageProgression.STAGES_PER_ACT => DialogueAct.Act1,
            <= StageProgression.STAGES_PER_ACT * 2 => DialogueAct.Act2,
            _ => DialogueAct.Act3
        };
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EntryPoint()
    {
        GameLoop.Initialize(Initialize, Shutdown, Update, LateUpdate, FixedUpdate, DrawGizmos);
    }
}