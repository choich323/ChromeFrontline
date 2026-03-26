using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHqPanelMenuSelect : AUIHqPanelSelect
{
    [SerializeField] private Button _btnProduce;
    [SerializeField] private Button _btnEntityUpgrade;
    [SerializeField] private Button _btnHqUpgrade;
    
    public override void SetPanel()
    {
        _panelType = HqRightPanelType.Menu;
        SetBtns();
    }

    void SetBtns()
    {
        _btnProduce.onClick.RemoveAllListeners();
        _btnProduce.onClick.AddListener(OnBtnProduce);
    }
    
    void OnBtnProduce()
    {
        _goToPanel?.Invoke(HqRightPanelType.Lane, null);
    }
    
    public override void Destroy()
    {
        
    }
}
