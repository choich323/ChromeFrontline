using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILongPressDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _threshold = 0.5f; // 길게 누르기 인정 시간
    
    private event Action _onLongPress;
    private Action<bool> _interactableBtn;
    private bool _isPointerDown = false;
    private bool _longPressTriggered = false;
    private float _pointerDownTimer = 0f;

    public event Action OnLongPress
    {
        add => _onLongPress += value;
        remove => _onLongPress -= value;
    }
    
    void Update()
    {
        if (_isPointerDown && !_longPressTriggered)
        {
            _pointerDownTimer += Time.unscaledDeltaTime;
            if (_pointerDownTimer >= _threshold)
            {
                _longPressTriggered = true;
                _onLongPress?.Invoke();
                EnableBtn(false);
            }
        }
    }

    public void SetActionInteractableBtn(Action<bool> argInteractableBtn)
    {
        _interactableBtn = argInteractableBtn;
    }
    
    public void OnPointerDown(PointerEventData argEventData)
    {
        _isPointerDown = true;
        _pointerDownTimer = 0f;
        _longPressTriggered = false;

        EnableBtn(true);
    }
    
    public void OnPointerUp(PointerEventData argEventData)
    {
        Clear();
    }

    public void OnPointerExit(PointerEventData argEventData)
    {
        Clear();
    }
    
    void EnableBtn(bool argEnable)
    {
        _interactableBtn?.Invoke(argEnable);
    }
    
    public void Clear()
    {
        _isPointerDown = false;
        _pointerDownTimer = 0f;
        _longPressTriggered = false;
    }
}
