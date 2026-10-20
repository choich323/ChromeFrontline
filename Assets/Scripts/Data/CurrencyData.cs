using System;
using UnityEngine;

[Serializable]
public class CurrencyInfo
{
    public int startGold;

    public int defaultStageChromeReward;
    public float stageRewardRatio;
    
    public int defaultSkillTrainingPrice;
    public float skillTrainingPriceRatio;

    public int maxSkillLevel;
    public int defaultSkillUpgradePrice;
    public float skillUpgradePriceRatio;

    public void SetInfo(CurrencyData argData)
    {
        startGold = argData.startGold;
        defaultStageChromeReward = argData.defaultStageChromeReward;
        stageRewardRatio = argData.stageRewardRatio;
        
        defaultSkillTrainingPrice = argData.defaultSkillTrainingPrice;
        skillTrainingPriceRatio = argData.skillTrainingPriceRatio;
        
        maxSkillLevel = argData.maxSkillLevel;
        defaultSkillUpgradePrice = argData.defaultSkillUpgradePrice;
        skillUpgradePriceRatio = argData.skillUpgradePriceRatio;
    }
}

[CreateAssetMenu(fileName = "PlayerCurrencyData", menuName = "Custom/PlayerCurrencyData")]
public class CurrencyData : ScriptableObject
{
    public int startGold = 300;

    public int defaultStageChromeReward = 100;
    public float stageRewardRatio = 0.05f;
    
    public int defaultSkillTrainingPrice = 600;
    public float skillTrainingPriceRatio = 0.5f;

    public int maxSkillLevel = 5;
    public int defaultSkillUpgradePrice = 3000;
    public float skillUpgradePriceRatio = 1f;
}
