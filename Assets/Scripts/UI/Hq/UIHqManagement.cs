using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHqManagement : APopup
{
    [SerializeField] private UIHqLeftPanel _leftPanel;
    [SerializeField] private UIHqRightPanel _rightPanel;
    
    public override void Init()
    {
        base.Init();
        
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(Close);
            _closeButton.onClick.AddListener(ForceClose);
        }

        if (_dimmedBg != null && InputMode == PopupInputMode.Modeless)
        {
            if(_dimmedBg.TryGetComponent<Button>(out var btn))
            {
                btn.onClick.RemoveListener(Close);
                btn.onClick.AddListener(ForceClose);
            }
        }
        
        _leftPanel.Init();
        _rightPanel.Init(ForceClose);
    }

    // esc 키를 누르는 경우 실행
    public override void Close()
    {
        _rightPanel.GoBack();
    }
    
    // menu select 패널일 때만 닫히도록 하고, 나머지는 패널 복귀로 하도록 강제 닫기를 별도로 제작.
    void ForceClose()
    {
        _leftPanel.Destroy();
        _rightPanel.Destroy();
        
        base.Close();
    }
}
