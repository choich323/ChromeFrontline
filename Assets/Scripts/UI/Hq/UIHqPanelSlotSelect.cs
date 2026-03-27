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
        SubscribeSpawner(argSlotIndex);
    }

    void SubscribeSpawner(int argSlotIndex)
    {
        var lane = _transitionContent.lane;
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)lane);
        spawner.GetSlot(argSlotIndex).OnSlotProgressChanged += RefreshSlotUI;
    }

    void UnScribeSpawner()
    {
        var lane = _transitionContent.lane;
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)lane);
        for (int i = 0; i < _slotUnitList.Count; i++)
        {
            spawner.GetSlot(i).OnSlotProgressChanged -= RefreshSlotUI;
        }
    }
    
    void RefreshSlotUI(int argSlotIndex, float argProgress)
    {
        var slotUnit = _slotUnitList[argSlotIndex];
        slotUnit.RefreshProgress(argProgress);
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
        UnScribeSpawner();
        DestroySlotUnits();
        _transitionContent.lane = Lane.None;
    }
}
