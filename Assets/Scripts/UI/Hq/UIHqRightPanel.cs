using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum HqRightPanelType
{
    Menu = 0,
    Slot,
    Entity,
    HqUpgrade,
    SlotUpgrade,
}

public class HqPanelTransitionContent
{
    public int slotIndex;
    public PrefabID prefabID;
}

public class UIHqRightPanel : MonoBehaviour
{
    private const HqRightPanelType START_PANEL_TYPE = HqRightPanelType.Menu;
    
    [SerializeField] private Button _btnPrev;
    [SerializeField] private float _transitionDuration = 0.2f;
    [SerializeField] private float _slideDistance = 120f;
    [SerializeField] private List<AUIHqRightPanelSelect> _panelList = new List<AUIHqRightPanelSelect>();
    
    private Dictionary<HqRightPanelType, AUIHqRightPanelSelect> _panelDict = new Dictionary<HqRightPanelType, AUIHqRightPanelSelect>();
    private Stack<HqRightPanelType> _panelStack = new Stack<HqRightPanelType>();
    private HqRightPanelType _curType;
    private Action _actionClose;
    
    public void Init(Action argActionClose)
    {
        _btnPrev.onClick.AddListener(GoBack);
        _btnPrev.gameObject.SetActive(false);
        
        _curType = START_PANEL_TYPE;
        foreach (var panel in _panelList)
        {
            panel.gameObject.SetActive(false);
            panel.Init(GoToPanel, GoBack);
            _panelDict.Add(panel.PanelType, panel);
        }

        var startPanel = _panelDict[_curType];
        startPanel.gameObject.SetActive(true);
        startPanel.CanvasGroup.alpha = 1f;
        startPanel.CanvasGroup.interactable = startPanel.CanvasGroup.blocksRaycasts = true;
        startPanel.SetPanel();
        _actionClose = argActionClose;
    }

    public void Destroy()
    {
        _btnPrev.onClick.RemoveAllListeners();
        _btnPrev.gameObject.SetActive(false);
        foreach (var kvp in _panelDict)
        {
            kvp.Value.Destroy();
        }
        _panelDict.Clear();
        _panelStack.Clear();
        _curType = START_PANEL_TYPE;
        _actionClose = null;
    }

    public void GoToPanel(HqRightPanelType argNextPanelType, HqPanelTransitionContent argContent = null)
    {
        if (!_panelDict.ContainsKey(argNextPanelType))
        {
            return;
        }
        
        _panelStack.Push(_curType);
        _panelDict[argNextPanelType].SetTransitionContent(argContent);
        Transition(argNextPanelType, false);
    }

    public void GoBack()
    {
        if (_panelStack.Count > 0)
        {
            var nextPanelType = _panelStack.Pop();
            var prevPanel = _panelDict[_curType];
            prevPanel.OnBeforeGoBackTransition();
            Transition(nextPanelType, true);
        }
        else
        {
            _actionClose?.Invoke();
        }
    }
    
    void Transition(HqRightPanelType argNextType, bool argIsGoBack)
    {
        var prevPanel = _panelDict[_curType];
        var nextPanel = _panelDict[argNextType];

        if (prevPanel == null || nextPanel == null)
        {
            Debug.LogError($"Can't find panels. curPanelType:{_curType}, nextPanelType:{argNextType}");
            return;
        }
        
        // 상호작용과 레이캐스트 설정
        prevPanel.CanvasGroup.interactable = prevPanel.CanvasGroup.blocksRaycasts = false;
        
        // 활성화 패널을 켜고 알파만 0으로
        nextPanel.gameObject.SetActive(true);
        nextPanel.CanvasGroup.alpha = 0f;
        
        // 시작 위치 설정
        float startX = argIsGoBack ? -_slideDistance : _slideDistance;
        nextPanel.RectTransform.anchoredPosition = new Vector2(startX, 0);

        // 트랜지션 시간동안 이전 패널을 투명하게 만들고
        prevPanel.CanvasGroup.DOFade(0, _transitionDuration).SetUpdate(true);
        // 동일한 시간동안 새로운 패널을 선명하게 만든다
        nextPanel.CanvasGroup.DOFade(1, _transitionDuration).SetUpdate(true);
        // 그러면서 위치를 중앙으로 이동시키고, 종료 후 이전 패널은 비활성화.
        nextPanel.RectTransform.DOAnchorPos(Vector2.zero, _transitionDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                nextPanel.CanvasGroup.interactable = nextPanel.CanvasGroup.blocksRaycasts = true;
                prevPanel.gameObject.SetActive(false);
            });
        _curType = argNextType;
        nextPanel.SetPanel();
        // 뒤로가기 키 제어
        if (_panelStack.Count <= 0)
        {
            _btnPrev.gameObject.SetActive(false);
        }
        else
        {
            _btnPrev.gameObject.SetActive(true);
        }
    }
}
