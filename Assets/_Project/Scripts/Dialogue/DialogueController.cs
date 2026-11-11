using UnityEngine;

public sealed class DialogueController
{
    private readonly DialogueSystem _system;
    private readonly DialogueView _view;

    private DialogueDefAsset _current;
    private DialogueDefAsset _pending;
 
    private int _lineIndex;
    private float _timer;
    private float _silenceTimer;

    private bool _waitingNextLine;

    private const float MinSilence = 0.6f;

    public bool IsPlaying => _current != null;
    public bool IsBusy => _current != null || _pending != null || _silenceTimer > 0f;

    public DialogueController(DialogueSystem dialogueSystem, DialogueView view)
    {
        _system = dialogueSystem;
        _view = view;
        _view.Hide();
    }

    public void Trigger(DialogueTrigger trigger, DialogueAct act)
    {
        if (!_system.TryGet(trigger, act, Time.time, out var entry))
            return;

        if (_current == null && _silenceTimer <= 0f)
        {
            Play(entry);
            return;
        }

        if (_current != null && entry.Priority > _current.Priority)
        {
            Play(entry);
            return;
        }

        if (entry.Priority >= DialoguePriority.Important && (_pending == null || entry.Priority >= _pending.Priority))
        {
            _pending = entry;
        }
    }

    public void Tick(float deltaTime)
    {
        _view.Tick(deltaTime);

        if (_current != null)
        {
            if (_view.IsTyping)
                return;

            _timer -= deltaTime;

            if (_timer > 0f)
                return;

            if (!_waitingNextLine)
            {
                _waitingNextLine = true;
                _timer = Mathf.Max(
                    0f,
                    _current.Lines[_lineIndex].DelayAfter
                );
                return;
            }

            _lineIndex++;

            if (_lineIndex < _current.Lines.Length)
            {
                ShowLine();
                return;
            }

            _view.Hide();
            _current = null;
            _silenceTimer = MinSilence;
        }

        if (_silenceTimer > 0f)
        {
            _silenceTimer -= deltaTime;

            if (_silenceTimer > 0f)
                return;
        }

        if (_pending != null)
        {
            var next = _pending;
            _pending = null;
            Play(next);
        }
    }

    public void SkipTyping()
    {
        if (_current != null && _view.IsTyping)
            _view.Complete();
    }

    private void Play(DialogueDefAsset entry)
    {
        _current = entry;
        _lineIndex = 0;
        _silenceTimer = 0f;

        ShowLine();
    }

    private void ShowLine()
    {
        var line = _current.Lines[_lineIndex];

        _waitingNextLine = false;
        _timer = Mathf.Max(0f, line.DisplayDuration);

        _view.Show(line.Text, line.DisplayMode, line.CharactersPerSecond);
    }
}