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
        _tierText.text = $"{hq.Tier}";
        _hpValueText.text = $"{hq.Hp}";
        _entityCountValueText.text = $"{hq.GetEntitiesCount()}";
        _goldValueText.text = $"{hq.Gold}";
    }

    public void Destroy()
    {
        
    }
}
