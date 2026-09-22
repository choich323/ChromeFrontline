using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillSelectPanel : MonoBehaviour
{
    private const int INVALID_ID = -1;
    private const int SKILL_SLOT_MAX = 4;
    
    [SerializeField] private Transform _skillBtnParent;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _backBtn;
    
    private List<UISkillSelectButton> _skillBtnList = new List<UISkillSelectButton>();
    private List<int> _equipedSkillIdList = new List<int>();
    private Action<CanvasGroup> _onSkillSelected;
    // 현재 선택된 슬롯 index
    private int _selectedSkillSlotIndex;
    
    public CanvasGroup CanvasGroup  => _canvasGroup;
    
    public void Init(Action<CanvasGroup> argOnSkillSelected)
    {
        _onSkillSelected = argOnSkillSelected;
        
        _backBtn.onClick.RemoveAllListeners();
        _backBtn.onClick.AddListener(() => OnSkillSelect(INVALID_ID));
    }
    
    public void SetUI(UserRecord argUserRecord)
    {
        if (_skillBtnList.Count > 0)
        {
            ClearBtnList();
        }
        
        foreach (var id in argUserRecord.SkillIds)
        {
            CreateSkillButtons(id);
        }
        
        SetEquipedSkills(argUserRecord);
    }

    public void SetCurSkillSlot(int argIndex)
    {
        _selectedSkillSlotIndex = argIndex;
    }
    
    public void CreateSkillButtons(int argSkillId)
    {
        var obj = Managers.Pool.Instantiate(PrefabID.UISkillSelectButton);
        if (obj == null)
        {
            return;
        }
        
        var skillBtn = obj.GetComponent<UISkillSelectButton>();
        _skillBtnList.Add(skillBtn);

        var skillInfo = Managers.Data.GetSkillInfo(argSkillId);
        skillBtn.Init(skillInfo, OnSkillSelect);
        
        skillBtn.transform.SetParent(_skillBtnParent);
        skillBtn.transform.SetAsLastSibling();
    }

    void OnSkillSelect(int argSkillId)
    {
        // 제대로 선택했다면
        if (argSkillId != INVALID_ID && _selectedSkillSlotIndex < SKILL_SLOT_MAX)
        {
            _equipedSkillIdList[_selectedSkillSlotIndex] = argSkillId;
            Managers.Game.SetEquipedSkillIds(_equipedSkillIdList);
        }
        
        _onSkillSelected?.Invoke(_canvasGroup);
    }
    
    void SetEquipedSkills(UserRecord argUserRecord)
    {
        _equipedSkillIdList.Clear();
        _equipedSkillIdList.AddRange(argUserRecord.EquipmentIds);
        
        foreach (var id in _equipedSkillIdList)
        {
            var btn = _skillBtnList.Find(btn => btn.ID == id);
            if (btn == null)
            {
                continue;
            }
            btn.SetEquipment(true);
        }
    }

    void ClearBtnList()
    {
        foreach (var btn in _skillBtnList)
        {
            btn.Clear();
            Managers.Pool.Destroy(btn, PrefabID.UISkillSelectButton);
        }
        _skillBtnList.Clear();
    }
    
    public void Clear()
    {
        ClearBtnList();
        _equipedSkillIdList.Clear();
        _onSkillSelected = null;
        _selectedSkillSlotIndex = INVALID_ID;
    }
}
