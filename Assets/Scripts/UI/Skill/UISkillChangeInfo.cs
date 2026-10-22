using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillChangeInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _level;
    [SerializeField] private TextMeshProUGUI _desc;
    [SerializeField] private TextMeshProUGUI _cooltime;
    [SerializeField] private GameObject _cooltimeObj;
    
    public void Init(int argTargetLevel, SkillInfo argSkillInfo) 
    {
        SetText(argTargetLevel, argSkillInfo);
    }

    void SetText(int argTargetLevel, SkillInfo argSkillInfo)
    {
        var sm = Managers.String;
        
        var level = sm.GetString(StringID.LevelString, argTargetLevel);
        _level.SetText(level);

        if (argSkillInfo.type == SkillType.Passive)
        {
            _cooltimeObj.SetActive(false);
        }
        else
        {
            _cooltimeObj.SetActive(true);
            var upgradeInfo = Managers.Data.GetSkillUpgradeInfo(argSkillInfo.id);
            var calcCool = argSkillInfo.cooldown - ((argTargetLevel - 1) * upgradeInfo.cooldown);
            var cooltime = sm.GetString(StringID.Second, calcCool);
            _cooltime.SetText($"<color=#2C4EFE>{cooltime}</color>");
        }
        
        var descDetailId = argSkillInfo.descDetailId;
        var descDetail = sm.GetString(descDetailId, argSkillInfo.GetUIParams(argTargetLevel));
        _desc.SetText(descDetail);
        
        var rectT = _cooltimeObj.transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectT);
    }
}
