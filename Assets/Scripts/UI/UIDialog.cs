using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _talker;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _textSpeed = 0.03f;
    [SerializeField] private float _dialogShowDelay = 0.15f;

    private Coroutine _typingCoroutine;
    private bool _isTyping;
    private string _curText;
    
    public void ShowText(string argTalker, string argText)
    {
        ClearText();
        
        _talker.SetText(argTalker);
        _curText = argText;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _typingCoroutine = StartCoroutine(CoShowText());
    }

    void ClearText()
    {
        _talker.SetText(string.Empty);
        _text.SetText(string.Empty);
    }
    
    IEnumerator CoShowText()
    {
        _isTyping = true;
        
        _text.SetText(_curText);
        _text.ForceMeshUpdate();
        _text.maxVisibleCharacters = 0;
        
        yield return new WaitForSecondsRealtime(_dialogShowDelay);
        
        int characterCount = _text.textInfo.characterCount;

        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(_textSpeed);
        
        for (int i = 0; i < characterCount; i++)
        {
            _text.maxVisibleCharacters = i + 1;

            yield return wait;
        }

        _isTyping = false;
        _typingCoroutine = null;
    }

    // typing 이면 true, 아니면 false
    public void OnClickDialog(Action<bool> argCallback)
    {
        if (_isTyping)
        {
            CompleteTyping(argCallback);
        }
        else
        {
            argCallback?.Invoke(false);
        }
    }

    void CompleteTyping(Action<bool> argCallback)
    {
        if (!_isTyping)
        {
            argCallback?.Invoke(false);
            return;
        }

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        _text.maxVisibleCharacters = int.MaxValue;
        _isTyping = false;
        
        argCallback?.Invoke(true);
    }
}
