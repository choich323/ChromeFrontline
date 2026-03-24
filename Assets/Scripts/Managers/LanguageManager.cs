using System;
using UnityEngine;

public enum Language
{
    English = 0,
    Korean,
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
}
