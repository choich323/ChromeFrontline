using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct UIGradeInfo
{
    public Image img;
    public TextMeshProUGUI probabilityText;
}

public class UIHqPanelSlotUpgrade : AUIHqPanelSlot
{
    [SerializeField] private List<UIGradeInfo> _gradeInfoList;
    
    private List<UISlotUpgradeUnit> _slotUnitList = new List<UISlotUpgradeUnit>();

    protected override void OnInit()
    {
        base.OnInit();

        var infoList = Managers.Data.GetGradeInfoList();
        int i = 0;
        foreach (var info in infoList)
        {
            var uiInfo = _gradeInfoList[i++];
            uiInfo.img.color = info.color;
            uiInfo.probabilityText.SetText($"{info.probability}%");
        }
    }
    
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
