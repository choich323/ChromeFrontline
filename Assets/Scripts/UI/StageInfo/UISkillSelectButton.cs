using System;
using UnityEngine;
using UnityEngine.UI;

public class UISkillSelectButton : MonoBehaviour
{
    [SerializeField] private Button _btn;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _activeText;
    [SerializeField] private GameObject _passiveText;

    private Action<int> _onSkillSelected;
    private int _id;

    public int ID => _id;
    
    public void Init(SkillInfo argSkillInfo, Action<int> argOnSelect)
    {
        _btn.onClick.RemoveListener(OnClick);
        _btn.onClick.AddListener(OnClick);

        _id = argSkillInfo.id;
        _onSkillSelected = argOnSelect;
        SetIcon(argSkillInfo.icon);
        SetType(argSkillInfo.type);
    }

    void SetIcon(Sprite argSprite)
    {
        _icon.sprite = argSprite;
    }
    
    void SetType(SkillType argSkillType)
    {
        if (argSkillType == SkillType.Active)
        {
            _activeText.SetActive(true);
            _passiveText.SetActive(false);
        }
        else
        {
            _activeText.SetActive(false);
            _passiveText.SetActive(true);
        }
    }
    
    void OnClick()
    {
        _onSkillSelected?.Invoke(_id);
    }

    public void Clear()
    {
        _btn.onClick.RemoveListener(OnClick);
        _icon.sprite = null;
    }
}
