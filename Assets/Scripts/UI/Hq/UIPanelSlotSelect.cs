using System.Collections.Generic;
using UnityEngine;

public class UIPanelSlotSelect : MonoBehaviour
{
    [SerializeField] private Transform _slotParent;

    private List<UISlot> _slotList = new List<UISlot>();

    public void Init()
    {
        SetPanel();
    }
    
    public void SetPanel()
    {
        DestroySlots();
        CreateSlots();
    }

    public void CreateSlots()
    {
        int slotCount = Managers.Game.GameField.GetSlotCount(Team.Player);
        for (int i = 0; i < slotCount; i++)
        {
            CreateSlot(i);
        }
    }

    public void CreateSlot(int argSlotIndex)
    {
        var slotObj = Managers.Pool.Instantiate(PrefabID.UISlot);
        if(slotObj == null)
            return;
        
        slotObj.transform.SetParent(_slotParent);
        var slot = slotObj.GetComponent<UISlot>();
        slot.Init(argSlotIndex);
    }

    public void DestroySlots()
    {
        foreach (var slot in _slotList)
        {
            Managers.Pool.Destroy(slot, PrefabID.UISlot);
            slot.Destroy();
        }
        _slotList.Clear();
    }

    public void Destroy()
    {
        DestroySlots();
    }
}
