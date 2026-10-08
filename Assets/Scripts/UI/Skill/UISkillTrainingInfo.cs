using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillTrainingInfo : APopup
{
    private const int INVALID_ID = 0;
    
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private TextMeshProUGUI _activeType;
    [SerializeField] private TextMeshProUGUI _passiveType;
    
    [SerializeField] private GameObject _arrow;
    [SerializeField] private UISkillChangeInfo _currentInfo;
    [SerializeField] private UISkillChangeInfo _nextInfo;
    
    [SerializeField] private Button _btn;

    private int _id = INVALID_ID;

    public void SetInfo(int argSkillId)
    {
        _id = argSkillId;

        Refresh();
    }

    void Refresh()
    {
        SetButton();
        SetUI();
    }
    
    void SetUI()
    {
        var ur = Managers.Game.UserRecord;
        bool isUnlocked = ur.IsUnlockedSkillId(_id);
        if (isUnlocked)
        {
            _arrow.SetActive(true);
            _nextInfo.gameObject.SetActive(true);
        }
        else
        {
            _arrow.SetActive(false);
            _nextInfo.gameObject.SetActive(false);
        }
        
        var skillInfo = Managers.Data.GetSkillInfo(_id);
        _icon.sprite = skillInfo.icon;
        var sm = Managers.String;
        string name = sm.GetString(skillInfo.nameId);
        _name.SetText(name);

        bool isActive = skillInfo.type == SkillType.Active;
        
        _activeType.gameObject.SetActive(isActive);
        var active = sm.GetString(StringID.Active);
        _activeType.SetText(active);
        
        var passive = sm.GetString(StringID.Passive);
        _passiveType.SetText(passive);
        _passiveType.gameObject.SetActive(!isActive);
        
        SetChangeInfo(isUnlocked, skillInfo);
    }

    void SetChangeInfo(bool argIsUnlocked, SkillInfo argSkillInfo)
    {
        var ur = Managers.Game.UserRecord;
        var level = ur.GetSkillLevel(_id);
        level = level <= 0 ? 1 : level;
        var maxLevel = Managers.Data.SkillMaxLevel;
        bool isMaxLevel = level >= maxLevel;
        
        if (argIsUnlocked && !isMaxLevel)
        {
            _nextInfo.Init(level + 1, argSkillInfo);
        }
        _currentInfo.Init(level, argSkillInfo);
    }
    
    void SetButton()
    {
        var ur = Managers.Game.UserRecord;
        var level = ur.GetSkillLevel(_id);
        var maxLevel = Managers.Data.SkillMaxLevel;
        if (level >= maxLevel)
        {
            _btn.gameObject.SetActive(false);
            return;
        }
        
        _btn.gameObject.SetActive(true);
        _btn.onClick.RemoveAllListeners();            
        if (ur.IsUnlockedSkillId(_id))
        {
            _btn.onClick.AddListener(OnBtnUpgrade);
        }
        else
        {
            _btn.onClick.AddListener(OnBtnBuy);
        }
        
        // price setting
        var price = Managers.Data.GetSkillPrice(_id);
        _price.SetText($"{price:N0}");
    }

    void OnBtnBuy()
    {
        var sm = Managers.Sound;
        if (Managers.Game.TryBuySkill(_id))
        {
            sm.PlayUpgradeSfx(true);
            Refresh();
        }
        else
        {
            sm.PlayUpgradeSfx(false);
            OnFail();
        }
    }

    void OnBtnUpgrade()
    {
        var sm = Managers.Sound;
        if (Managers.Game.TryUpgradeSkill(_id))
        {
            sm.PlayUpgradeSfx(true);
            Refresh();
        }
        else
        {
            sm.PlayUpgradeSfx(false);
            OnFail();
        }
    }

    void OnFail()
    {
        var ph = Managers.UI.PopupHandler;
        var notice = ph.OpenPopup<UINotice>(PrefabID.UINotice);
        string msg = Managers.String.GetString(StringID.NotEnoughChrome);
        notice.SetData(msg, ph.ClosePopup);
    }
    
    public override void Clear()
    {
        base.Clear();
        
        _btn.onClick.RemoveAllListeners();
        _id = INVALID_ID;
    }
}
