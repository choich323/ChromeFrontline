using System;
using UnityEngine;

public abstract class AUIHqRightPanelSelect : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _rectTransform;

    protected Action<HqRightPanelType, HqPanelTransitionContent> _goToPanel;
    protected Action _goBack;
    protected HqRightPanelType _panelType;
    protected HqPanelTransitionContent _transitionContent;
    
    public CanvasGroup CanvasGroup => _canvasGroup;
    public HqRightPanelType PanelType => _panelType;
    public RectTransform RectTransform => _rectTransform;

    public virtual void Init(Action<HqRightPanelType, HqPanelTransitionContent> argGoToPanel, Action argGoBack)
    {
        _canvasGroup.interactable = _canvasGroup.blocksRaycasts = false;
        _goToPanel = argGoToPanel;
        _goBack = argGoBack;
        
        SetType();
        
        OnInit();
    }

    public virtual void SetTransitionContent(HqPanelTransitionContent argContent)
    {
        _transitionContent = argContent;
    }

    public virtual void OnBeforeGoBackTransition()
    {
        Clear();
    }

    protected virtual void OnInit()
    {
        
    }
    
    public abstract void SetType();
    public abstract void SetPanel();
    // panel을 앞뒤로 이동할 때 호출
    public abstract void Clear();
    // 팝업 자체가 닫힐 때 호출
    public abstract void Destroy();
}
