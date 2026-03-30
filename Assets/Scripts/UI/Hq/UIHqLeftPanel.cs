using TMPro;
using UnityEngine;

public class UIHqLeftPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hpValueText;
    [SerializeField] private TextMeshProUGUI _shieldValueText;
    [SerializeField] private TextMeshProUGUI _entityCountValueText;
    [SerializeField] private TextMeshProUGUI _goldValueText;
    [SerializeField] private TextMeshProUGUI _mineralValueText;

    public void Init()
    {
        SetText();
    }

    void SetText()
    {
        var hq = Managers.Game.GameField.PlayerHq;
        _hpValueText.text = $"{hq.Hp}";
        _shieldValueText.text = $"{hq.Shield}";
        _entityCountValueText.text = $"{hq.GetEntitiesCount()}";
        _goldValueText.text = $"{hq.Gold}";
        _mineralValueText.text = $"{hq.Mineral}";
    }

    public void Destroy()
    {
        
    }
}
