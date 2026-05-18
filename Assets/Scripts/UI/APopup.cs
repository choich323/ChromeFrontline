using UnityEngine;
using UnityEngine.UI;
using System;

public enum PopupStackAction
{
    None,
    Additive,   // 이전 팝업을 유지한 채 새로 생성
    Exclusive,  // 이전 팝업을 비활성화 하고 새로 생성
}

public enum PopupInputMode
{
    None,
    Modal,      // 팝업 외부 터치 차단
    Modeless    // 팝업 외부 터치 허용 또는 dimmed 터치를 통해 닫힘
}

public abstract class APopup : MonoBehaviour
{
    [Header("Stack Rule")]
    [SerializeField] private PopupStackAction _action = PopupStackAction.Exclusive;

    [Header("Input Rule")]
    [SerializeField] private PopupInputMode _inputMode = PopupInputMode.Modeless;

    [Header("Common UI Components")] 
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected GameObject _dimmedBg;

    protected Action _onClose;
    
    public PopupStackAction StackAction => _action;
    public PopupInputMode InputMode => _inputMode;
    public bool IsClosed { get; private set; } = true;

    public virtual void Init()
    {
        Clear();
        
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Close);
            _closeButton.onClick.AddListener(Managers.Sound.PlaySelectSfx);
        }

        if (_dimmedBg != null && _inputMode == PopupInputMode.Modeless)
        {
            if(_dimmedBg.TryGetComponent<Button>(out var btn))
            {
                btn.onClick.AddListener(Close);
                btn.onClick.AddListener(Managers.Sound.PlaySelectSfx);
            }
        }
    }

    public virtual void Clear()
    {
        _onClose = null;
        
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveAllListeners();
        }

        if (_dimmedBg != null && _inputMode == PopupInputMode.Modeless)
        {
            if(_dimmedBg.TryGetComponent<Button>(out var btn))
            {
                btn.onClick.RemoveAllListeners();
            }
        }
    }
    
    public virtual void Open()
    {
        if (!IsClosed) return;
        
        IsClosed = false;
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        if (IsClosed) return;

        IsClosed = true;
        
        _onClose?.Invoke();
    }

    public virtual void SetOnClose(Action argOnClose)
    {
        _onClose = argOnClose;
    }
}
