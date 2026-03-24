using UnityEngine;

public class PlayerPrefsManager
{
    private const string SOUND_KEY = "Sound";
    private const string SENSITIVITY_KEY = "Sensitivity";
    private const string FRAME_KEY = "Frame";
    private const string QUALITY_KEY = "Quality";
    private const string LANG_KEY = "Language";
    
    public float Sound => PlayerPrefs.GetFloat(SOUND_KEY);
    public float Sensitivity => PlayerPrefs.GetFloat(SENSITIVITY_KEY);
    public int Frame => PlayerPrefs.GetInt(FRAME_KEY);
    public int Quality => PlayerPrefs.GetInt(QUALITY_KEY);
    public int Language => PlayerPrefs.GetInt(LANG_KEY);
    
    public void Init()
    {
        
    }

    public void SetSound(float argValue)
    {
        PlayerPrefs.SetFloat(SOUND_KEY, argValue);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float argValue)
    {
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, argValue);
        PlayerPrefs.Save();
    }

    public void SetFrame(int argValue)
    {
        PlayerPrefs.SetInt(FRAME_KEY, argValue);
        PlayerPrefs.Save();
    }

    public void SetQuality(int argValue)
    {
        PlayerPrefs.SetInt(QUALITY_KEY, argValue);
        PlayerPrefs.Save();
    }

    public void SetLanguage(int argValue)
    {
        PlayerPrefs.SetInt(LANG_KEY, argValue);
        PlayerPrefs.Save();
    }
}
