using UnityEngine;

public class PlayerPrefsManager
{
    private const string SOUND_KEY = "Sound";
    private const string SENSITIVITY_KEY = "Sensitivity";
    private const string FRAME_KEY = "Frame";
    private const string QUALITY_KEY = "Quality";
    private const string LANG_KEY = "Language";

    private const float DEFAULT_SOUND = 1f;
    private const float DEFAULT_SENSITIVITY = 0.22f;
    private const int DEFAULT_FRAME_INDEX = 1;
    private const int DEFAULT_QUALITY_INDEX = 1;
    private const int DEFAULT_LANG_INDEX = 0;
    
    public float Sound => PlayerPrefs.GetFloat(SOUND_KEY, DEFAULT_SOUND);
    public float Sensitivity => PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENSITIVITY);
    public int Frame => PlayerPrefs.GetInt(FRAME_KEY, DEFAULT_FRAME_INDEX);
    public int Quality => PlayerPrefs.GetInt(QUALITY_KEY, DEFAULT_QUALITY_INDEX);
    public int Language => PlayerPrefs.GetInt(LANG_KEY, DEFAULT_LANG_INDEX);
    
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
