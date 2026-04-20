using TMPro;
using UnityEngine;

public class UIHqLeftPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelText;
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
        _levelText.text = $"{hq.Level}";
        _hpValueText.text = $"{hq.Hp}";
        // 필요할 경우 다시 사용
        //_shieldValueText.text = $"{hq.Shield}";
        _entityCountValueText.text = $"{hq.GetEntitiesCount()}";
        _goldValueText.text = $"{hq.Gold}";
        // 필요할 경우 다시 사용
        //_mineralValueText.text = $"{hq.Mineral}";
    }

    public void Destroy()
    {
        
    }
}
