using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillTrainingCenter : APopup
{
    [SerializeField] private Transform _btnParent;
    [SerializeField] private TextMeshProUGUI _chrome;
    [SerializeField] private ScrollRect _scrollRect;
    
    private List<UISkillSelectButton> _btnList = new List<UISkillSelectButton>();
    
    public override void Init()
    {
        base.Init();

        _scrollRect.verticalNormalizedPosition = 1f;
        CreateBtns();
    }

    void CreateBtns()
    {
        var infoList = Managers.Data.GetSkillInfoList();
        foreach(var info in infoList)
        {
            var obj = Managers.Pool.Instantiate(PrefabID.UISkillSelectButton);
            var btn = obj.GetComponent<UISkillSelectButton>();
            _btnList.Add(btn);
            btn.transform.SetParent(_btnParent, false);
            btn.transform.SetAsLastSibling();
            int level = Managers.Game.UserRecord.GetSkillLevel(info.id);
            btn.Init(level, info, OnSelectSkill);
            btn.SetUnlock();
            if (level <= 0)
            {
                btn.SetTextsEmpty();
            }
        }
    }

    void OnSelectSkill(int argId)
    {
        Managers.Sound.PlaySelectSfx();
        var popup = Managers.UI.PopupHandler.OpenPopup<UISkillTrainingInfo>(PrefabID.UISkillTrainingInfo);
        if (popup == null)
        {
            return;
        }
        
        var info = popup.GetComponent<UISkillTrainingInfo>();
        info.SetInfo(argId);
        info.SetOnClose(OnClose);
        
        void OnClose()
        {
            Managers.Sound.PlaySelectSfx();
            Managers.UI.PopupHandler.ClosePopup();
        }
    }

    void ClearBtns()
    {
        var pool = Managers.Pool;
        foreach (var btn in _btnList)
        {
            btn.Clear();
            pool.Destroy(btn, PrefabID.UISkillSelectButton);
        }
        _btnList.Clear();
    }
    
    public override void Clear()
    {
        base.Clear();
        
        ClearBtns();
    }
}
