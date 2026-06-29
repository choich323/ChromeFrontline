using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEntityUnit : MonoBehaviour
{
    private const int INVALID_INDEX = -1;
    
    [SerializeField] private Button _btnSelect;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _productionTimeText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private UILongPressDetector _detector;
    [SerializeField] private GameObject _selectedTextObj; 

    private Action<PrefabID> _onSelectEntity;
    private EntityInfo _entityInfo;
    private int _slotIndex;
    private bool _selected;
    
    public void Init(int argSlotIndex, PrefabID argPrefabID, Action<PrefabID> argOnSelectEntity)
    {
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
        _icon.sprite = _entityInfo.iconImage;
        
        var localScale = _icon.rectTransform.localScale;
        localScale.x = _entityInfo.isOriginalSpriteFacingLeft ? -1f : 1f;
        _icon.rectTransform.localScale = localScale;
        
        CheckSelected();
    }

    void CheckSelected()
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
        if (spawner == null)
        {
            Debug.LogError($"Can't find spawner. Slot:{_slotIndex}");
            _selected = false;
            _selectedTextObj.SetActive(false);
            return;
        }

        var slot = spawner.GetSlot(_slotIndex);
        if (slot == null)
        {
            Debug.LogError($"Can't find slot. Slot:{_slotIndex}");
            _selected = false;
            _selectedTextObj.SetActive(false);
            return;
        }

        var curSlotTargetId = slot.GetTargetId();
        var id = Managers.Data.ConvertStringToPrefabID(_entityInfo.id);
        if (curSlotTargetId == id)
        {
            _selected = true;
            _selectedTextObj.SetActive(true);
            return;
        }
        
        _selected = false;
        _selectedTextObj.SetActive(false);
    }

    void SetText()
    {
        var sm = Managers.String;
        var dm = Managers.Data;
        var stringId = dm.ConvertStringToStringID(_entityInfo.id);
        _nameText.SetText(sm.GetString(stringId));
        _productionTimeText.SetText("{0}", _entityInfo.productionTime);
        _goldText.SetText("{0}", _entityInfo.goldCost);
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
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
        var slot = spawner.GetSlot(_slotIndex);
        var curSlotTargetId = slot.GetTargetId();
        var id = Managers.Data.ConvertStringToPrefabID(_entityInfo.id);
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
        popup.SetData(msg, OnConfirm, OnClose, confirm, cancel);

        void OnConfirm()
        {
            _onSelectEntity?.Invoke(id);
        }

        void OnClose()
        {
            Managers.Sound.PlaySelectSfx();
            ph.ClosePopup();
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
        
        popup.SetData(_entityInfo);
        popup.SetOnClose(OnClose);
        popup.SetPos(Input.mousePosition);
        
        void OnClose()
        {
            Managers.Sound.PlaySelectSfx();
            ph.ClosePopup();
        }
    }
    
    public void Destroy()
    {
        _slotIndex = INVALID_INDEX;
        _onSelectEntity = null;
        _entityInfo = null;
        _btnSelect.onClick.RemoveAllListeners();
        _detector.OnLongPress -= OpenEntityStats;
        _detector.Clear();
    }
}
