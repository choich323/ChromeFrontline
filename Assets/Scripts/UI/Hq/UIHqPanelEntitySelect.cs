using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHqPanelEntitySelect : AUIHqPanelSelect
{
    [SerializeField] private Transform _entityParent;
    [SerializeField] private Button _stopBtn;
    [SerializeField] private TextMeshProUGUI _stopBtnText;
    
    private List<UIEntityUnit> _entityUnitList = new List<UIEntityUnit>();

    public override void SetType()
    {
        _panelType = HqRightPanelType.Entity;
    }

    protected override void OnInit()
    {
        _stopBtn.onClick.AddListener(OnBtnStopProducing);
    }
    
    public override void SetPanel()
    {
        DestroyEntityUnits();
        CreateEntityUnits();
        SetText();
    }

    void CreateEntityUnits()
    {
        var unlockEntityList = Managers.Game.GetUnlockEntityIDList();
        foreach (var entityId in unlockEntityList)
        {
            CreateEntityUnit(entityId);
        }
    }

    void CreateEntityUnit(PrefabID argPrefabID)
    {
        var entityUnitObj = Managers.Pool.Instantiate(PrefabID.UIEntityUnit);
        if (entityUnitObj == null)
            return;

        entityUnitObj.transform.SetParent(_entityParent);
        entityUnitObj.transform.localScale = Vector3.one;
        var entityUnit = entityUnitObj.GetComponent<UIEntityUnit>();
        entityUnit.Init(_transitionContent.lane, _transitionContent.slotIndex, argPrefabID, OnSelectEntityUnit);
        _entityUnitList.Add(entityUnit);
    }

    void OnSelectEntityUnit(PrefabID argPrefabID)
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)_transitionContent.lane);
        if (spawner == null) return;
        
        var slot = spawner.GetSlot(_transitionContent.slotIndex);
        if (slot == null) return;
        
        slot.ChangeTarget(argPrefabID);
        
        _goBack?.Invoke();
    }

    void SetText()
    {
        _stopBtnText.text = Managers.String.GetString(StringID.StopProducing);
    }

    public void OnBtnStopProducing()
    {
        if (_transitionContent.prefabID == PrefabID.None)
        {
            _goBack?.Invoke();
            return;
        }
        
        var popup = Managers.UI.PopupHandler.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
        if (popup == null) return;

        var sm = Managers.String;
        var msg = sm.GetString(StringID.ConfirmStopProducing) + '\n' + sm.GetString(StringID.NowProducingEntity) + $"{_transitionContent.prefabID}";
        string confirm = sm.GetString(StringID.Yes);
        string cancel = sm.GetString(StringID.No);
        var ph = Managers.UI.PopupHandler;
        popup.Init();
        popup.SetData(msg, OnConfirm, ph.ClosePopup, confirm, cancel);

        void OnConfirm()
        {
            OnSelectEntityUnit(PrefabID.None);
        }
    }
    
    void DestroyEntityUnits()
    {
        foreach (var entityUnit in _entityUnitList)
        {
            Managers.Pool.Destroy(entityUnit, PrefabID.UIEntityUnit);
            entityUnit.Destroy();
        }
        _entityUnitList.Clear();
    }

    public override void Clear()
    {
        var ph = Managers.UI.PopupHandler;
        if (ph.Top().GetType() == typeof(UIEntityStat))
        {
            ph.ClosePopup();
        }
        DestroyEntityUnits();
        _transitionContent = null;
    }
    
    public override void Destroy()
    {
        Clear();
        _stopBtn.onClick.RemoveAllListeners();
    }
}
