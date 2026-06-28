using System;
using UnityEngine;

public enum Language
{
    English = 0,
    Korean,
}

[Serializable]
public class LocalizationText
{
    public string en;
    public string kr;
}

public class LanguageManager : MonoBehaviour
{
    private Language _currentLanguage = Language.English;
    
    public Language CurrentLanguage => _currentLanguage;
    
    public void Init()
    {
        LoadLanguage();
    }

    void LoadLanguage()
    {
        _currentLanguage = (Language)Managers.Prefs.Language;
    }
    
    public void SetLanguage(int argLanguageIndex)
    {
        var prevLang = _currentLanguage;
        Managers.Prefs.SetLanguage(argLanguageIndex);
        _currentLanguage = (Language)argLanguageIndex;
        Debug.Log($"Language Changed. {prevLang} -> {_currentLanguage}");
    }

    public string GetLocalizedString(LocalizationText argText)
    {
        switch (_currentLanguage)
        {
            default:
            case Language.English:
                return argText.en;
                break;
            case Language.Korean:
                return argText.kr;
                break;
        }
    }
}
