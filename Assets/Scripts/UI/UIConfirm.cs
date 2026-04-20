using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirm : APopup
{
    [SerializeField] private TextMeshProUGUI _msgText;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Button _cancelBtn;
    [SerializeField] private TextMeshProUGUI _confirmText;
    [SerializeField] private TextMeshProUGUI _cancelText;

    private Action _confirmAction;
    
    public override void Init()
    {
        base.Init();
        
        _cancelBtn.onClick.AddListener(Close);
    }

    public override void Clear()
    {
        base.Clear();
        
        _cancelBtn.onClick.RemoveAllListeners();
    }

    public void SetData(string argMsg, Action argConfirmAction, Action argCancelAction, string argConfirmText, string argCancelText)
    {
        _msgText.text = argMsg;
        _confirmAction = argConfirmAction;
        _onClose = argCancelAction;
        
        _confirmText.text = argConfirmText;
        _cancelText.text = argCancelText;
        
        _confirmBtn.onClick.RemoveAllListeners();
        _confirmBtn.onClick.AddListener(() =>
        {
            _confirmAction?.Invoke();
            Close();
        });
    }
}
