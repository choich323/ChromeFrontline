using TMPro;
using UnityEngine;

public class UIHqManagement : APopup
{
    [SerializeField] private UIHqLeftPanel _leftPanel;
    [SerializeField] private UIPanelSlotSelect _panelSlotSelect;
    
    public override void Init()
    {
        base.Init();
        
        _leftPanel.Init();
        _panelSlotSelect.Init();
    }

    public override void Close()
    {
        // menu select 패널일 때만 닫히도록 하고, 나머지는 패널 복귀로 하도록 수정.
        
        _panelSlotSelect.Destroy();
        
        base.Close();
    }
}
