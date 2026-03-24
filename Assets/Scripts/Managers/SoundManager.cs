using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public float MasterVolume => AudioListener.volume;
    
    public void Init()
    {
        AudioListener.volume = Managers.Prefs.Sound;
    }

    public void SetMasterVolume(float argValue)
    {
        AudioListener.volume = argValue;
        Managers.Prefs.SetSound(argValue);
    }
}
