using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHqPanelMenuSelect : AUIHqPanelSelect
{
    [SerializeField] private Button _btnProduce;
    [SerializeField] private Button _btnEntityUpgrade;
    [SerializeField] private Button _btnHqUpgrade;

    public override void SetType()
    {
        _panelType = HqRightPanelType.Menu;
    }

    protected override void OnInit()
    {
        _btnProduce.onClick.AddListener(OnBtnProduce);
        _btnHqUpgrade.onClick.AddListener(OnBtnHqUpgrade);
    }
    
    public override void SetPanel()
    {
        
    }
    
    void OnBtnProduce()
    {
        _goToPanel?.Invoke(HqRightPanelType.Lane, null);
    }

    void OnBtnHqUpgrade()
    {
        _goToPanel?.Invoke(HqRightPanelType.HqUpgrade, null);
    }
    
    public override void Clear()
    {
        
    }
    
    public override void Destroy()
    {
        _btnProduce.onClick.RemoveAllListeners();
        _btnHqUpgrade.onClick.RemoveAllListeners();
    }
}
