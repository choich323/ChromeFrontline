using TMPro;
using UnityEngine;

public class UIHqLeftPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _hpValueText;
    [SerializeField] private TextMeshProUGUI _entityCountValueText;
    [SerializeField] private TextMeshProUGUI _goldValueText;

    public void Init()
    {
        SetText();
    }

    void SetText()
    {
        var hq = Managers.Game.GameField.PlayerHq;
        _levelText.text = $"{hq.Level}";
        _hpValueText.text = $"{hq.Hp}";
        _entityCountValueText.text = $"{hq.GetEntitiesCount()}";
        _goldValueText.text = $"{hq.Gold}";
    }

    public void Destroy()
    {
        
    }
}
