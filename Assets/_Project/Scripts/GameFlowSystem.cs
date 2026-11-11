using System.Collections.Generic;
using UnityEngine;

public enum FlowActionType : byte
{
    None,
    Wait,
    ShowScreen,
    HideScreen,
    Dialogue,
    RollDice,
    SetPhase
}

public struct FlowAction
{
    public FlowActionType Type;

    public int ScreenId;
    public float Duration;
    public DialogueTrigger DialogueTrigger;
    public GamePhase Phase;

    public static FlowAction Wait(float seconds) => new()
    {
        Type = FlowActionType.Wait,
        Duration = seconds
    };

    public static FlowAction Show(int index) => new()
    {
        Type = FlowActionType.ShowScreen,
        ScreenId = index
    };

    public static FlowAction Hide(int index) => new()
    {
        Type = FlowActionType.HideScreen,
        ScreenId = index
    };

    public static FlowAction Speak(DialogueTrigger trigger) => new()
    {
        Type = FlowActionType.Dialogue,
        DialogueTrigger = trigger
    };

    public static FlowAction SetPhase(GamePhase phase) => new()
    {
        Type = FlowActionType.SetPhase,
        Phase = phase
    };
}

public sealed class GameFlowSystem
{
    private readonly Queue<FlowAction> _queue = new();

    private readonly ScreenSystem _screens;
    private readonly DialogueController _dialogue;
    private readonly GameState _gameState;

    private FlowAction _current;
    private float _elapsed;
    private bool _running;

    private readonly bool _debug;

    public bool IsBusy => _running || _queue.Count > 0;

    public GameFlowSystem(ScreenSystem screens, DialogueController dialogue, GameState gameState, bool debug = false)
    {
        _debug = debug;
        _screens = screens;
        _dialogue = dialogue;
        _gameState = gameState;
    }

    public void Enqueue(FlowAction action)
    {
        _queue.Enqueue(action);
        Log($"ENQUEUE {Describe(action)}");
    }

    public void Clear()
    {
        _queue.Clear();
        _running = false;
        
        Log("CLEAR");
    }

    public void Tick(float deltaTime)
    {
        if (!_running)
        {
            if (_queue.Count == 0)
                return;

            _current = _queue.Dequeue();
            _elapsed = 0f;
            _running = true;

            StartAction(_current);
        }

        if (!IsActionComplete(_current, deltaTime))
            return;

        _running = false;
    }

    private void StartAction(FlowAction action)
    {
        Log($"START {Describe(action)}");
        switch (action.Type)
        {
            case FlowActionType.ShowScreen:
                _screens.Show(action.ScreenId);
                break;
            case FlowActionType.HideScreen:
                _screens.Hide(action.ScreenId);
                break;
            case FlowActionType.Dialogue:
                _dialogue.Trigger(action.DialogueTrigger, DialogueAct.Act1);
                break;
            case FlowActionType.SetPhase:
                _gameState.PendingPhase = action.Phase;
                break;
        }
    }

    private bool IsActionComplete(FlowAction action, float deltaTime)
    {
        switch (action.Type)
        {
            case FlowActionType.Wait:
            {
                _elapsed += deltaTime;
                return _elapsed >= action.Duration;   
            }
            case FlowActionType.ShowScreen:
            case FlowActionType.HideScreen:
            {
                return !_screens.IsTransitioning;   
            }
            case FlowActionType.Dialogue:
            {
                return !_dialogue.IsBusy;   
            }
            case FlowActionType.SetPhase:
            {
                return _gameState.Phase == action.Phase;   
            }
            default:
                return true;
        }
    }
        
    private string Describe(FlowAction action)
    {
        string name = action.Type.ToString();

        return action.Type switch
        {
            FlowActionType.Wait => $"{name} ({action.Duration:0.00}s)",
            FlowActionType.ShowScreen or FlowActionType.HideScreen => $"{name} [Screen {action.ScreenId}]",
            FlowActionType.Dialogue => $"{name} [{action.DialogueTrigger}]",
            FlowActionType.SetPhase => $"{name} [{action.Phase}]",
            _ => name
        };
    }

    private void Log(string message)
    {
        if (!_debug)
            return;

        Debug.Log($"[GameFlow] {message} | " + $"Running: {_running}, " + $"Queued: {_queue.Count}");
    }
}