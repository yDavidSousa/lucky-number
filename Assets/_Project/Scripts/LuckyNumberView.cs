using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class LuckyNumberView : MonoBehaviour
{
    private const int MIN_NUMBER = 0;
    private const int MAX_NUMBER = 6;

    public TextMeshProUGUI NumberText;
    public float Duration = 2.5f;

    private float _elapsed;
    private int _start;
    private int _target;
    private int _totalSteps;
    private int _previousStep;

    private bool _isSpinning;
    public bool IsSpinning => _isSpinning;

    private Action _onComplete;

    public void SetNumber(int number)
    {
        _isSpinning = false;
        NumberText.SetText("{0:0}", number);
    }

    public void Spin(int target, Action onComplete = null)
    {
        if (target < MIN_NUMBER || target > MAX_NUMBER)
            return;

        int count = MAX_NUMBER - MIN_NUMBER + 1;

        _start = Random.Range(0, count);
        _target = target;

        int targetIndex = target - MIN_NUMBER;
        int offset = (targetIndex - _start + count) % count;

        int rotations = 5;
        _totalSteps = rotations * count + offset;

        _elapsed = 0f;
        _previousStep = -1;
        _isSpinning = true;
        _onComplete = onComplete;
    }

    public void Tick(float deltaTime)
    {
        if (!_isSpinning)
            return;

        _elapsed += deltaTime;

        float t = Mathf.Clamp01(_elapsed / Duration);
        float eased = 1f - Mathf.Pow(1f - t, 3f);

        int step = Mathf.FloorToInt(eased * _totalSteps);

        if (step != _previousStep)
        {
            int count = MAX_NUMBER - MIN_NUMBER + 1;
            int index = (_start + step) % count;

            int number = MIN_NUMBER + index;

            NumberText.SetText("{0:0}", number);

            _previousStep = step;
        }

        if (t >= 1f)
        {
            _onComplete?.Invoke();
            _isSpinning = false;
            NumberText.SetText("{0:0}", _target);
        }
    }
}