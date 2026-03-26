using System.Collections.Generic;
using UnityEngine;

public class UIPanelSlotSelect : MonoBehaviour
{
    [SerializeField] private Transform _slotParent;

    private List<UISlotUnit> _slotUnitList = new List<UISlotUnit>();

    public void Init()
    {
        SetPanel();
    }
    
    public void SetPanel()
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
        slot.Init(argSlotIndex);
        _slotUnitList.Add(slot);
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

    public void Destroy()
    {
        DestroySlotUnits();
    }
}
