using System;
using UnityEngine;
using UnityEngine.UI;

public enum Lane
{
    None = -1,
    Top = 0,
    Mid,
    Bottom,
}

public class UIHqPanelLaneSelect : AUIHqRightPanelSelect
{
    [SerializeField] private Button _btnTop;
    [SerializeField] private Button _btnMid;
    [SerializeField] private Button _btnBottom;

    public override void SetType()
    {
        _panelType = HqRightPanelType.Lane;
    }

    protected override void OnInit()
    {
        _btnTop.onClick.AddListener(OnBtnTop);
        _btnMid.onClick.AddListener(OnBtnMid);
        _btnBottom.onClick.AddListener(OnBtnBottom);
    }
    
    public override void SetPanel()
    {
        
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

    public override void Clear()
    {
        
    }
    
    public override void Destroy()
    {
        _btnTop.onClick.RemoveAllListeners();
        _btnMid.onClick.RemoveAllListeners();
        _btnBottom.onClick.RemoveAllListeners();
    }
}
