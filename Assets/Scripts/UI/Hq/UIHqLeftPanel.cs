using TMPro;
using UnityEngine;

public class UIHqLeftPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tierText;
    [SerializeField] private TextMeshProUGUI _hpValueText;
    [SerializeField] private TextMeshProUGUI _entityCountValueText;
    [SerializeField] private TextMeshProUGUI _goldValueText;

    public void Init()
    {
        var hq = Managers.Game.GameField.PlayerHq;
        hq.OnGoldChanged -= SetGoldText;
        hq.OnGoldChanged += SetGoldText;
        hq.OnHealthChanged -= SetHpText;
        hq.OnHealthChanged += SetHpText;
        hq.OnEntityCountChanged -= SetEntityCountText;
        hq.OnEntityCountChanged += SetEntityCountText;
        hq.OnTierChanged -= SetTier;
        hq.OnTierChanged += SetTier;
        RefreshText();
    }

    void RefreshText()
    {
        var hq = Managers.Game.GameField.PlayerHq;
        SetTier(hq.Tier);
        SetGoldText(hq.Gold);
        _hpValueText.SetText("{0}", hq.Hp);
        SetEntityCountText(hq.GetEntitiesCount());
    }
    
    void SetTier(int argTier)
    {
        _tierText.SetText("{0}", argTier);
    }

    void SetGoldText(long argGold)
    {
        _goldValueText.SetText("{0}", argGold);
    }

    void SetHpText(int argHp, int _)
    {
        _hpValueText.SetText("{0}", argHp);
    }

    void SetEntityCountText(int argEntityCount)
    {
        _entityCountValueText.SetText("{0}", argEntityCount);
    }

    public void Destroy()
    {
        
    }
}
