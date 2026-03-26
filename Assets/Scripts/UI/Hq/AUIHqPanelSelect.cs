using System;
using UnityEngine;

public abstract class AUIHqPanelSelect : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _rectTransform;

    protected Action<HqRightPanelType, HqPanelTransitionContent> _goToPanel;
    protected HqRightPanelType _panelType;
    protected HqPanelTransitionContent _transitionContent;
    
    public CanvasGroup CanvasGroup => _canvasGroup;
    public HqRightPanelType PanelType => _panelType;
    public RectTransform RectTransform => _rectTransform;

    public virtual void Init(Action<HqRightPanelType, HqPanelTransitionContent> argGoToPanel)
    {
        _goToPanel = argGoToPanel;
        
        SetPanel();
    }

    public virtual void SetTransitionContent(HqPanelTransitionContent argContent)
    {
        _transitionContent = argContent;
    }
    
    public abstract void SetPanel();

    public abstract void Destroy();
}
