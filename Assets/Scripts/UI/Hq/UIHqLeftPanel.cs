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
        _hpValueText.text = hq.Hp.ToString();
        _shieldValueText.text = hq.Shield.ToString();
        _entityCountValueText.text = hq.GetEntityCount().ToString();
        _goldValueText.text = hq.Gold.ToString();
        _mineralValueText.text = hq.Mineral.ToString();
    }
}
