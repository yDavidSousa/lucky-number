using TMPro;
using UnityEngine;

public sealed class DialogueView : MonoBehaviour
{
    public CanvasGroup CanvasGroup;
    public TMP_Text DialogueText;

    private float _elapsed;
    private bool _typing;
    private int _charactersPerSecond;

    public bool IsTyping => _typing;

    public void Show(string text, DialogueDisplayMode mode, int charactersPerSecond)
    {
        DialogueText.text = text;
        DialogueText.ForceMeshUpdate();

        _elapsed = 0f;
        _charactersPerSecond = Mathf.Max(1, charactersPerSecond);

        _typing = mode == DialogueDisplayMode.Typewriter;

        DialogueText.maxVisibleCharacters = _typing ? 0 : int.MaxValue;

        CanvasGroup.alpha = 1f;
        CanvasGroup.interactable = false;
        CanvasGroup.blocksRaycasts = false;
    }

    public void Tick(float deltaTime)
    {
        if (!_typing)
            return;

        _elapsed += deltaTime;

        int count = Mathf.FloorToInt(_elapsed * _charactersPerSecond);

        DialogueText.maxVisibleCharacters = count;

        if (count >= DialogueText.textInfo.characterCount)
        {
            Complete();
        }
    }

    public void Complete()
    {
        DialogueText.maxVisibleCharacters = int.MaxValue;
        _typing = false;
    }

    public void Hide()
    {
        CanvasGroup.alpha = 0f;
        _typing = false;
    }
}