using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEntityUnit : MonoBehaviour
{
    private const int INVALID_INDEX = -1;
    
    [SerializeField] private Button _btnSelect;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _productionTimeText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _mineralText;
    [SerializeField] private TextMeshProUGUI _typeTagText;
    [SerializeField] private TextMeshProUGUI _combatRoleTagText;
    [SerializeField] private GameObject _combatRole;
    [SerializeField] private UILongPressDetector _detector;

    private Action<PrefabID> _onSelectEntity;
    private EntityInfo _entityInfo;
    private Lane _lane;
    private int _slotIndex;
    
    public void Init(Lane argLane, int argSlotIndex, PrefabID argPrefabID, Action<PrefabID> argOnSelectEntity)
    {
        _lane = argLane;
        _slotIndex = argSlotIndex;
        SetInfo(argPrefabID);
        SetText();
        SetBtnSelect();
        _onSelectEntity = argOnSelectEntity;
        SetDetector();
    }

    void SetInfo(PrefabID argPrefabID)
    {
        Managers.Data.TryGetPrefabInfo((int)argPrefabID, out var info);
        _entityInfo = info as EntityInfo;
        // TODO: 마찬가지로 icon 이미지 등록 필요
    }

    void SetText()
    {
        var sm = Managers.String;
        var dm = Managers.Data;
        _levelText.text = sm.GetString(StringID.Lv, _entityInfo.level);
        var stringId = dm.ConvertStringToStringID(_entityInfo.stringId);
        _nameText.text = sm.GetString(stringId);
        _productionTimeText.text = $"{_entityInfo.productionTime}";
        _goldText.text = $"{_entityInfo.goldCost}";
        _mineralText.text = $"{_entityInfo.mineralCost}";
        _typeTagText.text = sm.GetString(dm.ConvertStringToStringID(_entityInfo.typeTagStringId));
        if (string.IsNullOrEmpty(_entityInfo.combatRoleTagStringId))
        {
            _combatRole.SetActive(false);
        }
        else
        {
            _combatRoleTagText.text = sm.GetString(dm.ConvertStringToStringID(_entityInfo.combatRoleTagStringId));
        }
    }

    void SetBtnSelect()
    {
        _btnSelect.onClick.RemoveAllListeners();
        _btnSelect.onClick.AddListener(OnBtnSelect);
    }
    
    void SetDetector()
    {
        _detector.OnLongPress -= OpenEntityStats;
        _detector.OnLongPress += OpenEntityStats;
        _detector.SetActionInteractableBtn(InteractableBtnSelect);
    }

    void InteractableBtnSelect(bool argInteractable)
    {
        _btnSelect.interactable = argInteractable;
    }

    void OnBtnSelect()
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)_lane);
        if (spawner == null)
        {
            Debug.LogError($"Can't find spawner. Lane:{_lane}, Slot:{_slotIndex}");
            return;
        }

        var slot = spawner.GetSlot(_slotIndex);
        if (slot == null)
        {
            Debug.LogError($"Can't find slot. Lane:{_lane}, Slot:{_slotIndex}");
            return;
        }

        var curSlotTargetId = slot.GetTargetId();
        var id = Managers.Data.ConvertStringToPrefabID(_entityInfo.prefab.name);
        if (curSlotTargetId == PrefabID.None || curSlotTargetId == id)
        {
            OnConfirm();
            return;
        }
        
        var ph = Managers.UI.PopupHandler;
        var popup = ph.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
        
        var sm = Managers.String;
        string msg = sm.GetString(StringID.ConfirmEntitySelect);
        string confirm = sm.GetString(StringID.Yes);
        string cancel = sm.GetString(StringID.No);
        popup.SetData(msg, OnConfirm, ph.ClosePopup, confirm, cancel);

        void OnConfirm()
        {
            _onSelectEntity?.Invoke(id);
        }
    }

    void OpenEntityStats()
    {
        var ph = Managers.UI.PopupHandler;
        if (ph.Top().GetType() == typeof(UIEntityStat))
        {
            ph.ClosePopup();
        }
        
        var popup = ph.OpenPopup<UIEntityStat>(PrefabID.UIEntityStat);
        if (popup == null) return;
        
        popup.Init();
        popup.SetData(_entityInfo);
        popup.SetOnClose(ph.ClosePopup);
        popup.SetPos(Input.mousePosition);
    }
    
    public void Destroy()
    {
        _lane = Lane.None;
        _slotIndex = INVALID_INDEX;
        _onSelectEntity = null;
        _entityInfo = null;
        _btnSelect.onClick.RemoveAllListeners();
        _detector.OnLongPress -= OpenEntityStats;
        _detector.Clear();
    }
}
