using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINotice : APopup
{
    [SerializeField] private TextMeshProUGUI _msg;
    [SerializeField] private Button _confirmBtn;
    
    public void SetData(string argMessage, Action argConfirmAction)
    {
        _msg.text = argMessage;
        
        _confirmBtn.onClick.RemoveAllListeners();
        _confirmBtn.onClick.AddListener(() =>
        {
            argConfirmAction?.Invoke();
            Close();
        });
    }
}
