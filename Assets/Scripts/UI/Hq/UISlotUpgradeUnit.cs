using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private ShakeEffect _shakeEffect;

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
        var spawner = Managers.Game.GameField.PlayerHq.GetSpawner();
        if (spawner == null) return;
        
        var slot = spawner.GetSlot(_slotIndex);
        if (slot == null) return;

        _grade = slot.Grade;
        var gradeInfo = Managers.Data.GetGradeInfo(_grade);
        _bg.color = gradeInfo.color;
        
        _slotNumber.SetText($"{_slotIndex + 1:D2}");
        if(_grade < Grade.Ultimate)
        {
            _canUpgrade.SetActive(true);
            _gradeMax.SetActive(false);
            _goldText.SetText("{0}", gradeInfo.gold);
            _btnSlot.interactable = true;
        }
        else
        {
            _canUpgrade.SetActive(false);
            _gradeMax.SetActive(true);
            _btnSlot.interactable = false;
        }
    }
    
    void OnBtn()
    {
        var cost = Managers.Data.GetGradeInfo(_grade).gold;
        bool isSuccess = Managers.Game.SetRandomGrade(cost, _slotIndex);
        if (!isSuccess)
        {
            Managers.Sound.PlayUpgradeSfx(false);
            _shakeEffect.PlayShakeAnimation();
            return;
        }
        Managers.Sound.PlayUpgradeSfx(true);
        SetInfo();
    }

    /*
    [ContextMenu("Upgrade")]
    void TestUpgrade()
    {
        StartCoroutine(CoTestUpgrade());
    }
    
    IEnumerator CoTestUpgrade()
    {
        int count = 0;
        while (_grade != Grade.Ultimate)
        {
            bool isSuccess = Managers.Game.SetRandomGrade(0, _slotIndex);
            count++;
            if (!isSuccess)
            {
                _shakeEffect.PlayShakeAnimation();
                yield return null;
            }
            SetInfo();

            yield return null;
        }
        Debug.Log($"{count}");
    }
    */
    
    public void Destroy()
    {
        _bg.color = Color.white;
        _slotIndex = INVALID_SLOT_INDEX;
        _btnSlot.onClick.RemoveAllListeners();
    }
}
