using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHqPanelSlotSelect : AUIHqPanelSelect
{
    [SerializeField] private Transform _slotParent;
    [SerializeField] private UIAddSlotUnit _addSlotUnit;
    [SerializeField] private ScrollRect _scrollRect;

    private List<UISlotUnit> _slotUnitList = new List<UISlotUnit>();

    protected override void OnInit()
    {
        _addSlotUnit.Init(AddSlot);
    }
    
    public override void SetType()
    {
        _panelType = HqRightPanelType.Slot;
    }
    
    public override void SetPanel()
    {
        Clear();
        ResetScrollRect();
        CreateSlots();
        SetAddSlotBtn();
    }

    void ResetScrollRect()
    {
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 1f;
    }
    
    void CreateSlots()
    {
        int slotCount = Managers.Game.GameField.PlayerHq.GetSlotCount();
        for (int i = 0; i < slotCount; i++)
        {
            CreateSlot();
        }
    }

    void CreateSlot()
    {
        var slotObj = Managers.Pool.Instantiate(PrefabID.UISlotUnit);
        if (slotObj == null)
            return;
        
        slotObj.transform.SetParent(_slotParent);
        slotObj.transform.localScale = Vector3.one;
        var slot = slotObj.GetComponent<UISlotUnit>();
        _slotUnitList.Add(slot);
        var slotIndex = _slotUnitList.Count - 1;
        slotObj.transform.SetSiblingIndex(slotIndex);
        slot.Init(slotIndex, _transitionContent.lane);
        SetBtn(slot);
        SubscribeSlotProgress(slotIndex);
        
        _addSlotUnit.gameObject.transform.SetAsLastSibling();
    }

    void SetAddSlotBtn()
    {
        var curSlotCount = _slotUnitList.Count;
        var slotMaxCount = Managers.Game.SlotCountMax;
        if (curSlotCount >= slotMaxCount)
        {
            _addSlotUnit.gameObject.SetActive(false);
            return;
        }
        _addSlotUnit.gameObject.SetActive(true);
        var cost = Managers.Data.GetAddSlotCost(curSlotCount);
        _addSlotUnit.SetText(cost);
    }
    
    void AddSlot()
    {
        var ph = Managers.UI.PopupHandler;
        var popup = ph.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
        popup.Init();
        var sm = Managers.String;
        string msg = sm.GetString(StringID.ConfirmAddSlot);
        string confirm = sm.GetString(StringID.Yes);
        string cancel = sm.GetString(StringID.No);
        bool isConfirm = false;
        popup.SetData(msg, OnConfirm, OnClose, confirm, cancel);

        void OnConfirm()
        {
            isConfirm = true;
        }

        void OnClose()
        {
            ph.ClosePopup();

            if (isConfirm)
            {
                var isSuccess = Managers.Game.GameField.PlayerHq.AddSlot();
                if (isSuccess)
                {
                    CreateSlot();
                    SetAddSlotBtn();
                }
            }
        }
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

    public override void Clear()
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
