using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillTrainingInfo : APopup
{
    private const int INVALID_ID = 0;
    
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _desc;
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private TextMeshProUGUI _activeType;
    [SerializeField] private TextMeshProUGUI _passiveType;
    
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _nextInfo;
    
    [SerializeField] private Button _btn;

    private int _id = INVALID_ID;

    public void SetInfo(int argSkillId)
    {
        _id = argSkillId;

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
            _nextInfo.SetActive(true);
        }
        else
        {
            _arrow.SetActive(false);
            _nextInfo.SetActive(false);
        }
        // price setting
        
        
        
        var skillInfo = Managers.Data.GetSkillInfo(_id);
        _icon.sprite = skillInfo.icon;
        var sm = Managers.String;
        string name = sm.GetString(skillInfo.nameId);
        string desc = sm.GetString(skillInfo.descId);
        _name.SetText(name);
        _desc.SetText(desc);

        bool isActive = skillInfo.type == SkillType.Active;
        
        _activeType.gameObject.SetActive(isActive);
        var active = sm.GetString(StringID.Active);
        _activeType.SetText(active);
        
        var passive = sm.GetString(StringID.Passive);
        _passiveType.SetText(passive);
        _passiveType.gameObject.SetActive(!isActive);
        
        SetUpgradeInfo();
    }

    void SetUpgradeInfo()
    {
        // upgrade text info
    }
    
    void SetButton()
    {
        var ur = Managers.Game.UserRecord;
        _btn.onClick.RemoveAllListeners();            
        if (ur.IsUnlockedSkillId(_id))
        {
            _btn.onClick.AddListener(OnBtnUpgrade);
        }
        else
        {
            _btn.onClick.AddListener(OnBtnBuy);
        }
    }

    void OnBtnBuy()
    {
        
    }

    void OnBtnUpgrade()
    {
        
    }
    
    public override void Clear()
    {
        base.Clear();
        
        _btn.onClick.RemoveAllListeners();
        _id = INVALID_ID;
    }
}
