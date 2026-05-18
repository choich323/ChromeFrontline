using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHqPanelSlotSelect : AUIHqPanelSlot
{
    private List<UISlotUnit> _slotUnitList = new List<UISlotUnit>();
    
    public override void SetType()
    {
        _panelType = HqRightPanelType.Slot;
    }
    
    protected override void CreateSlot()
    {
        var slotObj = Managers.Pool.Instantiate(PrefabID.UISlotUnit);
        if (slotObj == null)
            return;
        
        slotObj.transform.SetParent(_slotParent);
        slotObj.transform.localScale = Vector3.one;
        var slot = slotObj.GetComponent<UISlotUnit>();
        var slotIndex = _slotUnitList.Count;
        _slotUnitList.Add(slot);
        slotObj.transform.SetSiblingIndex(slotIndex);
        slot.Init(slotIndex);
        SetBtn(slot);
        SubscribeSlotProgress(slotIndex);
        
        _addSlotUnit.gameObject.transform.SetAsLastSibling();
    }

    void SubscribeSlotProgress(int argSlotIndex)
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
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
        
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
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
        argSlot.SetTransitionAction(_goToPanel);
    }
    
    protected override void DestroySlotUnits()
    {
        foreach (var slot in _slotUnitList)
        {
            Managers.Pool.Destroy(slot, PrefabID.UISlotUnit);
            slot.Destroy();
        }
        _slotUnitList.Clear();
    }

    public override void Clear()
    {
        UnSubscribeSlotProgress();
        
        base.Clear();
    }
}
