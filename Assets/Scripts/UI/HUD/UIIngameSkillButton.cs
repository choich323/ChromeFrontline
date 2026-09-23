using UnityEngine;
using UnityEngine.UI;

public class UIIngameSkillButton : MonoBehaviour
{
    private const int INVALID_ID = 0;
    private const int INVALID_SKILL_LEVEL = 0;
    
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    [SerializeField] private Image _cooldown;
    [SerializeField] private GameObject _lock;
    [SerializeField] private GameObject _unlock;

    private int _id;
    private int _skillLevel;
    private SkillInfo _skillInfo;
    
    public void Init(int argSkillId, int argSkillLevel)
    {
        _id = argSkillId;
        _skillLevel = argSkillLevel;

        _skillInfo = Managers.Data.GetSkillInfo(argSkillId);
        if (_skillInfo == null)
        {
            SetLock();
            return;
        }
        
        SetUnlock();
        SetIcon(_skillInfo.icon);
        SetButton();
    }

    void SetIcon(Sprite argIcon)
    {
        _icon.sprite = argIcon;
    }

    void SetButton()
    {
        
    }

    void SetLock()
    {
        _lock.SetActive(true);
        _unlock.SetActive(false);
        _button.interactable = false;
    }

    void SetUnlock()
    {
        _lock.SetActive(false);
        _unlock.SetActive(true);
        _button.interactable = true;
    }

    public void Clear()
    {
        _id = INVALID_ID;
        _skillLevel = INVALID_SKILL_LEVEL;
        _skillInfo = null;
        
        SetLock();
    }
}
