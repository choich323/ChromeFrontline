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
        DestroySlotUnits();
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
        if(slotObj == null)
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
        
        slot.OnSlotProgressChanged += RefreshSlotUI;
    }

    void UnSubscribeSlotProgress()
    {
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

    public override void Destroy()
    {
        UnSubscribeSlotProgress();
        DestroySlotUnits();
        _transitionContent.lane = Lane.None;
    }
}
