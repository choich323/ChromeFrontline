using System;
using UnityEngine;
using UnityEngine.UI;

public enum Lane
{
    None = 0,
    Top,
    Mid,
    Bottom,
}

public class UIHqPanelLaneSelect : AUIHqPanelSelect
{
    [SerializeField] private Button _btnTop;
    [SerializeField] private Button _btnMid;
    [SerializeField] private Button _btnBottom;
    
    public override void SetPanel()
    {
        _panelType = HqRightPanelType.Lane;
        SetBtns();
    }

    void SetBtns()
    {
        _btnTop.onClick.RemoveAllListeners();
        _btnMid.onClick.RemoveAllListeners();
        _btnBottom.onClick.RemoveAllListeners();
        
        _btnTop.onClick.AddListener(OnBtnTop);
        _btnMid.onClick.AddListener(OnBtnMid);
        _btnBottom.onClick.AddListener(OnBtnBottom);
    }

    void OnBtnLane(Lane argLane)
    {
        var nextTransitionContent = new HqPanelTransitionContent();
        nextTransitionContent.lane = argLane;
        _goToPanel?.Invoke(HqRightPanelType.Slot, nextTransitionContent);
    }
    
    void OnBtnTop()
    {
        OnBtnLane(Lane.Top);
    }

    void OnBtnMid()
    {
        OnBtnLane(Lane.Mid);
    }

    void OnBtnBottom()
    {
        OnBtnLane(Lane.Bottom);
    }
    
    public override void Destroy()
    {
        
    }
}
