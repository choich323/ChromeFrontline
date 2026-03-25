using TMPro;
using UnityEngine;

public class UISlotUnit : MonoBehaviour
{
    private const int INVALID_SLOT_INDEX = -1;
    
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _slotText;

    private int _slotIndex;
    
    public int SlotIndex => _slotIndex;

    public void Init(int argSlotIndex)
    {
        _slotIndex = argSlotIndex;
        
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

    public void Destroy()
    {
        _slotIndex = INVALID_SLOT_INDEX;
    }
}
