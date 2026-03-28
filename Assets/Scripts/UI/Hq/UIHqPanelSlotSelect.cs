using System;
using System.Collections.Generic;
using UnityEngine;

public class UIHqPanelSlotSelect : AUIHqPanelSelect
{
    [SerializeField] private Transform _slotParent;

    private List<UISlotUnit> _slotUnitList = new List<UISlotUnit>();

    public override void SetType()
    {
        _panelType = HqRightPanelType.Slot;
    }
    
    public override void SetPanel()
    {
        Clear();
        CreateSlots();
    }

    public void CreateSlots()
    {
        int slotCount = Managers.Game.GameField.PlayerHq.GetSlotCount();
        for (int i = 0; i < slotCount; i++)
        {
            CreateSlot(i);
        }
    }

    public void CreateSlot(int argSlotIndex)
    {
        var slotObj = Managers.Pool.Instantiate(PrefabID.UISlotUnit);
        if (slotObj == null)
            return;
        
        slotObj.transform.SetParent(_slotParent);
        slotObj.transform.localScale = Vector3.one;
        var slot = slotObj.GetComponent<UISlotUnit>();
        slot.Init(argSlotIndex, _transitionContent.lane);
        _slotUnitList.Add(slot);
        SetBtn(slot);
        SubscribeSlotProgress(argSlotIndex);
    }

    void SubscribeSlotProgress(int argSlotIndex)
    {
        var lane = _transitionContent.lane;
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)lane);
        if (spawner == null) return;
        
        var slot = spawner.GetSlot(argSlotIndex);
        if (slot == null) return;
        
        slot.OnSlotProgressChanged -= RefreshSlotUI;
        slot.OnSlotProgressChanged += RefreshSlotUI;
    }

    void UnSubscribeSlotProgress()
    {
        // 슬롯 선택 창을 한 번도 안 열고 팝업을 닫으면 없음.
        if (_transitionContent == null) return;
        
        var lane = _transitionContent.lane;
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)lane);
        if (spawner == null) return;
        for (int i = 0; i < _slotUnitList.Count; i++)
        {
            var slot = spawner.GetSlot(i);
            if (slot == null) continue;
            
            slot.OnSlotProgressChanged -= RefreshSlotUI;
        }
    }
    
    void RefreshSlotUI(int argSlotIndex, float argProgress)
    {
        if (argSlotIndex < 0 || argSlotIndex >= _slotUnitList.Count)
            return;
        
        var slotUnit = _slotUnitList[argSlotIndex];
        slotUnit.RefreshProgress(argProgress);
        slotUnit.SetEntityInfo();
    }

    void SetBtn(UISlotUnit argSlot)
    {
        argSlot.SetBtnAction(_goToPanel);
    }
    
    public void DestroySlotUnits()
    {
        foreach (var slot in _slotUnitList)
        {
            Managers.Pool.Destroy(slot, PrefabID.UISlotUnit);
            slot.Destroy();
        }
        _slotUnitList.Clear();
    }

    void Clear()
    {
        UnSubscribeSlotProgress();
        DestroySlotUnits();
    }
    
    public override void Destroy()
    {
        Clear();
        _transitionContent = null;
    }
}
