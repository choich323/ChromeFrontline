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
        
    }
    
    void Update()
    {
        SetText();
    }

    void SetText()
    {
        var hq = Managers.Game.GameField.PlayerHq;
        _tierText.SetText($"{hq.Tier}");
        _hpValueText.SetText($"{hq.Hp}");
        _entityCountValueText.SetText($"{hq.GetEntitiesCount()}");
        _goldValueText.SetText($"{hq.Gold}");
    }

    public void Destroy()
    {
        
    }
}
