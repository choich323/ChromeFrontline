using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum HqRightPanelType
{
    Menu = 0,
    Lane,
    Slot,
    Entity,
}

public class HqPanelTransitionContent
{
    public Lane lane;
    public int slotIndex;

    public void Clear()
    {
        lane = Lane.None;
        slotIndex = -1;
    }
}

public class UIHqRightPanel : MonoBehaviour
{
    private const HqRightPanelType START_PANEL_TYPE = HqRightPanelType.Menu;
    
    [SerializeField] private Button _btnPrev;
    [SerializeField] private float _transitionDuration = 0.2f;
    [SerializeField] private float _slideDistance = 120f;
    [SerializeField] private List<AUIHqPanelSelect> _panelList = new List<AUIHqPanelSelect>();
    
    private Dictionary<HqRightPanelType, AUIHqPanelSelect> _panelDict = new Dictionary<HqRightPanelType, AUIHqPanelSelect>();
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
            panel.Init(GoToPanel);
            _panelDict.Add(panel.PanelType, panel);
        }

        var startPanel = _panelDict[_curType];
        startPanel.gameObject.SetActive(true);
        startPanel.CanvasGroup.alpha = 1f;
        startPanel.CanvasGroup.interactable = startPanel.CanvasGroup.blocksRaycasts = true;
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
        Transition(argNextPanelType, false);
        _panelDict[_curType].SetTransitionContent(argContent);
    }

    public void GoBack()
    {
        if (_panelStack.Count > 0)
        {
            var prevType = _panelStack.Pop();
            Transition(prevType, true);
        }
        else
        {
            _actionClose?.Invoke();
        }
    }
    
    void Transition(HqRightPanelType argNextType, bool argIsGoBack)
    {
        var removePanel = _panelDict[_curType];
        var addPanel = _panelDict[argNextType];

        if (removePanel == null || addPanel == null)
        {
            Debug.LogError($"Can't find panels. curPanelType:{_curType}, nextPanelType:{argNextType}");
            return;
        }
        
        // 상호작용과 레이캐스트 설정
        removePanel.CanvasGroup.interactable = removePanel.CanvasGroup.blocksRaycasts = false;
        
        // 활성화 패널을 켜고 알파만 0으로
        addPanel.gameObject.SetActive(true);
        addPanel.CanvasGroup.alpha = 0f;
        
        // 시작 위치 설정
        float startX = argIsGoBack ? -_slideDistance : _slideDistance;
        addPanel.RectTransform.anchoredPosition = new Vector2(startX, 0);

        // 트랜지션 시간동안 이전 패널을 투명하게 만들고
        removePanel.CanvasGroup.DOFade(0, _transitionDuration).SetUpdate(true);
        // 동일한 시간동안 새로운 패널을 선명하게 만든다
        addPanel.CanvasGroup.DOFade(1, _transitionDuration).SetUpdate(true);
        // 그러면서 위치를 중앙으로 이동시키고, 종료 후 이전 패널은 비활성화.
        addPanel.RectTransform.DOAnchorPos(Vector2.zero, _transitionDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                addPanel.CanvasGroup.interactable = addPanel.CanvasGroup.blocksRaycasts = true;
                removePanel.gameObject.SetActive(false);
                addPanel.SetPanel();
            });
        _curType = argNextType;
        
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
