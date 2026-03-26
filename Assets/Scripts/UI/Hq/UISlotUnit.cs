using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlotUnit : MonoBehaviour
{
    private const int INVALID_SLOT_INDEX = -1;
    
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _slotText;
    [SerializeField] private Button _btnSlot;

    private int _slotIndex;
    private Action<HqRightPanelType, HqPanelTransitionContent> _onSlotAction;
    
    public int SlotIndex => _slotIndex;

    public void Init(int argSlotIndex)
    {
        SetSlotIndex(argSlotIndex);
        
        SetText();    
    }
    
    public void SetSlotIndex(int argSlotIndex)
    {
        _slotIndex = argSlotIndex;
    }

    void SetText()
    {
        _slotText.text = Managers.String.GetString(StringID.Slot, _slotIndex + 1);
    }

    public void SetBtnAction(Action<HqRightPanelType, HqPanelTransitionContent> argGoToPanel)
    {
        _onSlotAction = argGoToPanel;
        SetListener();
    }

    void OnBtn()
    {
        var transitionContent = new HqPanelTransitionContent();
        transitionContent.slotIndex = _slotIndex;
        _onSlotAction?.Invoke(HqRightPanelType.Entity, transitionContent);
    }
    
    void SetListener()
    {
        _btnSlot.onClick.RemoveAllListeners();
        _btnSlot.onClick.AddListener(OnBtn);
    }
    
    public void Destroy()
    {
        _slotIndex = INVALID_SLOT_INDEX;
        _onSlotAction = null;
        _btnSlot.onClick.RemoveAllListeners();
    }
}
