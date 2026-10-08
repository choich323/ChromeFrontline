using TMPro;
using UnityEngine;

public class UISkillChangeInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _level;
    [SerializeField] private TextMeshProUGUI _desc;
    [SerializeField] private TextMeshProUGUI _cooltime;

    public void Init(int argTargetLevel, SkillInfo argSkillInfo) 
    {
        SetText(argTargetLevel, argSkillInfo);
    }

    void SetText(int argTargetLevel, SkillInfo argSkillInfo)
    {
        var sm = Managers.String;
        
        var level = sm.GetString(StringID.Level, argTargetLevel);
        _level.SetText(level);

        var upgradeInfo = Managers.Data.GetSkillUpgradeInfo(argSkillInfo.id);
        var calcCool = argSkillInfo.cooldown - ((argTargetLevel - 1) * upgradeInfo.cooldown);
        var cooltime = sm.GetString(StringID.Second, calcCool);
        _cooltime.SetText(cooltime);
        
        var descDetailId = argSkillInfo.descDetailId;
        var descDetail = sm.GetString(descDetailId, argSkillInfo.GetUIParams(argTargetLevel));
        _desc.SetText(descDetail);
    }
}
