using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AUIHqPanelSlot : AUIHqRightPanelSelect
{
    [SerializeField] protected Transform _slotParent;
    [SerializeField] protected UIAddSlotUnit _addSlotUnit;
    [SerializeField] protected ScrollRect _scrollRect;
    
    protected override void OnInit()
    {
        _addSlotUnit.Init(AddSlot);
    }
    
    public override void SetType()
    {
        
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

    protected abstract void CreateSlot();
    
    protected abstract void DestroySlotUnits();

    protected virtual void SetAddSlotBtn()
    {
        var gm = Managers.Game;
        var curSlotCount = gm.CurSlotCount;
        var slotMaxCount = gm.SlotCountMax;
        if (curSlotCount >= slotMaxCount)
        {
            _addSlotUnit.gameObject.SetActive(false);
            return;
        }
        _addSlotUnit.gameObject.SetActive(true);
        var cost = Managers.Data.GetAddSlotCost(curSlotCount);
        _addSlotUnit.SetText(cost);
    }
    
    protected virtual void AddSlot()
    {
        var sound = Managers.Sound;
        sound.PlaySelectSfx();
        
        var ph = Managers.UI.PopupHandler;
        var popup = ph.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
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
                sound.PlaySelectSfx();
            }
        }
    }
    
    public override void Clear()
    {
        DestroySlotUnits();
    }
    
    public override void Destroy()
    {
        Clear();
        _transitionContent = null;
    }
}
