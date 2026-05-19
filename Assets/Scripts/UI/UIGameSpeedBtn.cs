using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameSpeedBtn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private Button _btn;
    [SerializeField] private Image _btnImage;

    private int _index = 0;
    
    public void Init()
    {
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(OnBtn);
        Reset();
    }

    public void Reset()
    {
        _index = 0;
        var info = Managers.Data.GetGameSpeedInfo(_index);
        Managers.Game.SetGameSpeed(info.speed);
        _btnImage.color = info.color;
        SetText();
    }
    
    void SetText()
    {
        _speedText.SetText($"x{Managers.Game.CurGameSpeed}");
    }
    
    void OnBtn()
    {
        Managers.Sound.PlaySelectSfx();
        var gm = Managers.Game;
        _index++;
        var info = Managers.Data.GetGameSpeedInfo(_index);
        _btnImage.color = info.color;
        if (info.speed < gm.CurGameSpeed)
        {
            _index = 0;
        }
        gm.SetGameSpeed(info.speed);
        SetText();
    }
}
