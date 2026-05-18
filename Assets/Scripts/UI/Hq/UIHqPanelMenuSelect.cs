using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHqPanelMenuSelect : AUIHqRightPanelSelect
{
    [SerializeField] private Button _btnProduce;
    [SerializeField] private Button _btnSlotUpgrade;
    [SerializeField] private Button _btnHqUpgrade;

    private SoundManager Sm => Managers.Sound;
    
    public override void SetType()
    {
        _panelType = HqRightPanelType.Menu;
    }

    protected override void OnInit()
    {
        _btnProduce.onClick.AddListener(OnBtnProduce);
        _btnSlotUpgrade.onClick.AddListener(OnBtnSlotUpgrade);
        _btnHqUpgrade.onClick.AddListener(OnBtnHqUpgrade);
    }
    
    public override void SetPanel()
    {
        
    }
    
    void OnBtnProduce()
    {
        Sm.PlaySelectSfx();
        _goToPanel?.Invoke(HqRightPanelType.Slot, null);
    }

    void OnBtnSlotUpgrade()
    {
        Sm.PlaySelectSfx();
        _goToPanel?.Invoke(HqRightPanelType.SlotUpgrade, null);
    }
    
    void OnBtnHqUpgrade()
    {
        Sm.PlaySelectSfx();
        _goToPanel?.Invoke(HqRightPanelType.HqUpgrade, null);
    }
    
    public override void Clear()
    {
        
    }
    
    public override void Destroy()
    {
        _btnProduce.onClick.RemoveAllListeners();
        _btnSlotUpgrade.onClick.RemoveAllListeners();
        _btnHqUpgrade.onClick.RemoveAllListeners();
    }
}
