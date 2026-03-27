using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlotUnit : MonoBehaviour
{
    private const int INVALID_SLOT_INDEX = -1;

    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _slotText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private Button _btnSlot;
    [SerializeField] private Slider _progressSlider;

    private int _slotIndex;
    private int _targetLevel;
    private PrefabID _targetId;
    private Lane _lane;
    private Action<HqRightPanelType, HqPanelTransitionContent> _onSlotAction;
    
    public int SlotIndex => _slotIndex;

    public void Init(int argSlotIndex, Lane argLane)
    {
        SetSlotIndex(argSlotIndex);
        SetLane(argLane);
        
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)argLane);
        var slot = spawner.GetSlot(argSlotIndex);
        RefreshProgress(slot.GetProgress());
    }
    
    public void SetSlotIndex(int argSlotIndex)
    {
        _slotIndex = argSlotIndex;
    }

    public void SetEntityInfo(PrefabID argTargetId)
    {
        var dm = Managers.Data;
        dm.TryGetPrefabInfo((int)argTargetId, out var info);
        _targetId = argTargetId;
        var entityInfo = info as EntityInfo;
        _targetLevel = entityInfo.level;
        // TODO: icon 이미지도 세팅해야 한다.
        
        var sm = Managers.String;
        _levelText.text = _targetLevel > 0 ? sm.GetString(StringID.Lv, _targetLevel) : string.Empty;
    }
    
    public void RefreshProgress(float argProgress)
    {
        var sm = Managers.String;
        
        _slotText.text = sm.GetString(StringID.Slot, _slotIndex + 1);
        _progressText.text = $"{argProgress}%";
        _progressSlider.value = argProgress;

        if (_targetId != PrefabID.None && _targetLevel > 0)
        {
            _stateText.text = sm.GetString(StringID.Producing);
        }
        else
        {
            _stateText.text = string.Empty;
        }
    }

    public void SetBtnAction(Action<HqRightPanelType, HqPanelTransitionContent> argGoToPanel)
    {
        _onSlotAction = argGoToPanel;
        SetListener();
    }

    public void SetLane(Lane argLane)
    {
        _lane = argLane;
    }
    
    void OnBtn()
    {
        var transitionContent = new HqPanelTransitionContent();
        transitionContent.slotIndex = _slotIndex;
        transitionContent.lane = _lane;
        _onSlotAction?.Invoke(HqRightPanelType.Entity, transitionContent);
    }
    
    void SetListener()
    {
        _btnSlot.onClick.RemoveAllListeners();
        _btnSlot.onClick.AddListener(OnBtn);
    }
    
    public void Destroy()
    {
        _slotIndex = INVALID_SLOT_INDEX;
        _targetLevel = 0;
        _targetId = PrefabID.None;
        SetLane(Lane.None);
        _onSlotAction = null;
        _btnSlot.onClick.RemoveAllListeners();
    }
}
