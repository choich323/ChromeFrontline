using System;
using UnityEngine;

public abstract class AUIHqPanelSelect : MonoBehaviour
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
        _goToPanel = argGoToPanel;
        _goBack = argGoBack;
        
        SetType();
    }

    public virtual void SetTransitionContent(HqPanelTransitionContent argContent)
    {
        _transitionContent = argContent;
    }

    public virtual void OnBeforeGoBackTransition()
    {
        Destroy();
    }
    
    public abstract void SetType();
    public abstract void SetPanel();
    public abstract void Destroy();
}
