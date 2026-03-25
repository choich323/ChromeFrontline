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
        _panelSlotSelect.Destroy();
        
        base.Close();
    }
}
