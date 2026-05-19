using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlotUnit : MonoBehaviour
{
    private const int INVALID_SLOT_INDEX = -1;

    [SerializeField] private Image _bg;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _slotText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private Button _btnSlot;
    [SerializeField] private Slider _progressSlider;

    private int _slotIndex;
    private PrefabID _targetId;
    private Action<HqRightPanelType, HqPanelTransitionContent> _onSlotTransitionAction;
    
    public int SlotIndex => _slotIndex;

    public void Init(int argSlotIndex)
    {
        SetSlotIndex(argSlotIndex);
        SetEntityInfo();
        SetColor();

        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
        if (spawner == null) return;
        var slot = spawner.GetSlot(argSlotIndex);
        if (slot == null) return;
        RefreshProgress(slot.GetProgress());
    }
    
    public void SetSlotIndex(int argSlotIndex)
    {
        _slotIndex = argSlotIndex;
    }

    public void SetEntityInfo()
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
        if (spawner == null) return;
        
        var slot = spawner.GetSlot(_slotIndex);
        if (slot == null) return;
        
        var id = slot.GetTargetId();
        
        var dm = Managers.Data;
        dm.TryGetPrefabInfo((int)id, out var info);
        if (info == null)
        {
            _icon.gameObject.SetActive(false);
            return;
        }
        
        _targetId = id;
        var entityInfo = info as EntityInfo;
        _icon.gameObject.SetActive(true);
        _icon.sprite = entityInfo.iconImage;
    }

    void SetColor()
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
        if (spawner == null) return;
        
        var slot = spawner.GetSlot(_slotIndex);
        if (slot == null) return;

        var grade = slot.Grade;
        _bg.color = Managers.Data.GetGradeInfo(grade).color;
    }
    
    public void RefreshProgress(float argProgress)
    {
        var sm = Managers.String;
        
        _slotText.SetText(sm.GetString(StringID.Slot, _slotIndex + 1));
        _progressText.SetText($"{(argProgress * 100):N0}%");
        _progressSlider.value = argProgress;

        if (_targetId != PrefabID.None)
        {
            _stateText.SetText(sm.GetString(StringID.Producing));
        }
        else
        {
            _stateText.SetText(string.Empty);
        }
    }

    public void SetTransitionAction(Action<HqRightPanelType, HqPanelTransitionContent> argGoToPanel)
    {
        _onSlotTransitionAction = argGoToPanel;
        SetListener();
    }
    
    void OnBtn()
    {
        Managers.Sound.PlaySelectSfx();
        var transitionContent = new HqPanelTransitionContent();
        transitionContent.slotIndex = _slotIndex;
        transitionContent.prefabID = _targetId;
        _onSlotTransitionAction?.Invoke(HqRightPanelType.Entity, transitionContent);
    }
    
    void SetListener()
    {
        _btnSlot.onClick.RemoveAllListeners();
        _btnSlot.onClick.AddListener(OnBtn);
    }
    
    public void Destroy()
    {
        RefreshProgress(0);
        _bg.color = Color.white;
        _slotIndex = INVALID_SLOT_INDEX;
        _targetId = PrefabID.None;
        _onSlotTransitionAction = null;
        _btnSlot.onClick.RemoveAllListeners();
    }
}
