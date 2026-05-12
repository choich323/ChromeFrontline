using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlotUpgradeUnit : MonoBehaviour
{
    private const int INVALID_SLOT_INDEX = -1;
    
    [SerializeField] private Image _bg;
    [SerializeField] private TextMeshProUGUI _slotNumber;
    [SerializeField] private GameObject _canUpgrade;
    [SerializeField] private GameObject _gradeMax;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private Button _btnSlot;

    private int _slotIndex;
    private Grade _grade;

    public void Init(int argSlotIndex)
    {
        _slotIndex = argSlotIndex;
        
        SetListener();
        SetInfo();
    }

    void SetListener()
    {
        _btnSlot.onClick.RemoveAllListeners();
        _btnSlot.onClick.AddListener(OnBtn);
    }

    void SetInfo()
    {
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner((int)Lane.Top);
        if (spawner == null) return;
        
        var slot = spawner.GetSlot(_slotIndex);
        if (slot == null) return;

        _grade = slot.Grade;
        var gradeInfo = Managers.Data.GetGradeInfo(_grade);
        _bg.color = gradeInfo.color;
        
        _slotNumber.text = $"{_slotIndex + 1:D2}";
        if(_grade < Grade.Ultimate)
        {
            _canUpgrade.SetActive(true);
            _gradeMax.SetActive(false);
            _goldText.text = $"{gradeInfo.gold}";
        }
        else
        {
            _canUpgrade.SetActive(false);
            _gradeMax.SetActive(true);
        }
    }
    
    void OnBtn()
    {
        var cost = Managers.Data.GetGradeInfo(_grade).gold;
        Managers.Game.SetRandomGrade(cost, _slotIndex);
        SetInfo();
    }
    
    public void Destroy()
    {
        _bg.color = Color.white;
        _slotIndex = INVALID_SLOT_INDEX;
        _btnSlot.onClick.RemoveAllListeners();
    }
}
