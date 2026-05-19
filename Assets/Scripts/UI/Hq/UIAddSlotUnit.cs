using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAddSlotUnit : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _btnAddSlot;

    private Action _onAddBtn;
    
    public void SetText(int argCost)
    {
        _costText.SetText("{0}", argCost);
    }

    public void Init(Action argOnAddBtn)
    {
        _onAddBtn = argOnAddBtn;
        _btnAddSlot.onClick.RemoveAllListeners();
        _btnAddSlot.onClick.AddListener(() => _onAddBtn());
    }
}
