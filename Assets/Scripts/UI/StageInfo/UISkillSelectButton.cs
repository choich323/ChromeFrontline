using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISkillSelectButton : MonoBehaviour
{
    [SerializeField] private Button _btn;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _activeText;
    [SerializeField] private GameObject _passiveText;
    [SerializeField] private GameObject _unlockContents;
    [SerializeField] private GameObject _locked;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _desc;
    [SerializeField] private TextMeshProUGUI _level;

    private Action<int> _onSkillSelected;
    private int _id;

    public void Init(int argSkillLevel, SkillInfo argSkillInfo, Action<int> argOnSelect)
    {
        _btn.onClick.RemoveListener(OnClick);
        _btn.onClick.AddListener(OnClick);

        _id = argSkillInfo.id;
        _onSkillSelected = argOnSelect;
        SetIcon(argSkillInfo.icon);
        SetType(argSkillInfo.type);
        SetText(argSkillInfo.nameId, argSkillInfo.descId, argSkillLevel);
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

    void SetText(string argNameId, string argDescId, int argSkillLevel)
    {
        var sm = Managers.String;
        string skillName = sm.GetString(argNameId);
        _name.SetText(skillName);

        var desc = sm.GetString(argDescId);
        _desc.SetText(desc);

        if (argSkillLevel > 0)
        {
            string level = sm.GetString(StringID.Level, argSkillLevel);
            _level.SetText(level);
        }
    }
    
    void OnClick()
    {
        _onSkillSelected?.Invoke(_id);
    }

    public void SetLock()
    {
        _locked.SetActive(true);
        _unlockContents.SetActive(false);
        _btn.interactable = false;
    }

    public void SetUnlock()
    {
        _locked.SetActive(false);
        _unlockContents.SetActive(true);
        _btn.interactable = true;
    }
    
    public void Clear()
    {
        _btn.onClick.RemoveListener(OnClick);
        _icon.sprite = null;
    }
}
