using UnityEngine;
using UnityEngine.UI;

public class UIPauseBtn : MonoBehaviour
{
    [SerializeField] private Image _pauseBtnImage;
    [SerializeField] private Sprite _pauseBtnSprite;
    [SerializeField] private Sprite _playBtnSprite;
    [SerializeField] private Color _pauseBtnColor;
    [SerializeField] private Color _playBtnColor;
    [SerializeField] private Button _pauseBtn;
    
    bool IsPaused => Managers.Game.IsPaused;
    
    public void Init()
    {
        var gm = Managers.Game;
        gm.OnGamePause -= OnPauseGame;
        gm.OnGamePause += OnPauseGame;
        gm.OnGameResume -= OnResumeGame;
        gm.OnGameResume += OnResumeGame;
        
        _pauseBtn.onClick.RemoveAllListeners();
        _pauseBtn.onClick.AddListener(OnBtnPause);
    }

    void OnBtnPause()
    {
        var gm = Managers.Game;
        if (gm.IsGameOver)
        {
            return;
        }
        
        Managers.Sound.PlaySelectSfx();
        if (!IsPaused)
        {
            gm.PauseGame();
        }
        else
        {
            gm.ResumeGame();
        }
    }

    void OnPauseGame()
    {
        _pauseBtnImage.sprite = _playBtnSprite;
        _pauseBtnImage.color = _playBtnColor;
    }
    
    void OnResumeGame()
    {
        _pauseBtnImage.sprite = _pauseBtnSprite;
        _pauseBtnImage.color = _pauseBtnColor;
    }
}
