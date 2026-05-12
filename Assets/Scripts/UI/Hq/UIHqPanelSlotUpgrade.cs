using System.Collections.Generic;
using UnityEngine;

public class UIHqPanelSlotUpgrade : AUIHqPanelSlot
{
    private List<UISlotUpgradeUnit> _slotUnitList = new List<UISlotUpgradeUnit>();
    
    public override void SetType()
    {
        _panelType = HqRightPanelType.SlotUpgrade;
    }

    protected override void CreateSlot()
    {
        var slotObj = Managers.Pool.Instantiate(PrefabID.UISlotUpgradeUnit);
        if (slotObj == null)
            return;
        
        slotObj.transform.SetParent(_slotParent);
        slotObj.transform.localScale = Vector3.one;
        var slot = slotObj.GetComponent<UISlotUpgradeUnit>();
        var slotIndex = _slotUnitList.Count;
        _slotUnitList.Add(slot);
        slotObj.transform.SetSiblingIndex(slotIndex);
        slot.Init(slotIndex);
        
        _addSlotUnit.gameObject.transform.SetAsLastSibling();
    }

    protected override void DestroySlotUnits()
    {
        foreach (var slot in _slotUnitList)
        {
            Managers.Pool.Destroy(slot, PrefabID.UISlotUpgradeUnit);
            slot.Destroy();
        }
        _slotUnitList.Clear();
    }
}
