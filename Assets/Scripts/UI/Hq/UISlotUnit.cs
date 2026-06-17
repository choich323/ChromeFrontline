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
    [SerializeField] private GameObject _stateText;
    [SerializeField] private Button _btnSlot;
    [SerializeField] private Slider _progressSlider;

    private int _slotIndex;
    private int _lastProgressPercent = -1;
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
        SetProductionState();
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
        var localScale = _icon.rectTransform.localScale;
        localScale.x = entityInfo.isOriginalSpriteFacingLeft ? -1f : 1f;
        _icon.rectTransform.localScale = localScale;
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

    void SetProductionState()
    {
        var sm = Managers.String;
    
        _slotText.SetText(sm.GetString(StringID.Slot, _slotIndex + 1));

        if (_targetId != PrefabID.None)
        {
            _lastProgressPercent = -1;
        }
        else
        {
            _progressText.SetText("0%");
            _progressSlider.value = 0f;
            _stateText.SetActive(false);
        }
    }
    
    public void RefreshProgress(float argProgress)
    {
        _progressSlider.value = argProgress;
        int currentPercent = Mathf.FloorToInt(argProgress * 100f);

        if (currentPercent != _lastProgressPercent)
        {
            _lastProgressPercent = currentPercent;
            _progressText.SetText("{0}%", currentPercent);
        }

        _stateText.SetActive(_targetId != PrefabID.None);
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
