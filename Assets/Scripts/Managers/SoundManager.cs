using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private BGMData _ingameBgmData;
    [SerializeField] private AudioSource _bgmSource;
    
    private int _playlistIndex = 0;
    private Coroutine _playlistCoroutine;
    
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
    
    public void PlayIngameBgm()
    {
        if (_ingameBgmData == null || _ingameBgmData.bgmList.Length == 0) return;

        StopIngameBgm();

        _playlistIndex = 0;
        _bgmSource.loop = false; 

        _playlistCoroutine = StartCoroutine(CoPlayIngameBgm());
    }

    // 재생 중지용
    public void StopIngameBgm()
    {
        if (_playlistCoroutine != null)
        {
            StopCoroutine(_playlistCoroutine);
            _playlistCoroutine = null;
        }
        _bgmSource.Stop();
    }

    private IEnumerator CoPlayIngameBgm()
    {
        while (true)
        {
            // 이름으로 찾지 않고, SO 배열의 인덱스에서 직접 AudioClip을 꺼내옵니다.
            AudioClip clip = _ingameBgmData.bgmList[_playlistIndex].clip;

            if (clip != null)
            {
                _bgmSource.clip = clip;
                _bgmSource.Play();
                
                // 곡의 길이만큼 대기
                yield return new WaitForSecondsRealtime(clip.length);
            }
            else
            {
                // 클립이 비어있는 칸이 있다면 그냥 넘어감
                yield return null; 
            }

            // 다음 인덱스로 이동 (마지막 곡 다음에는 다시 0으로)
            _playlistIndex = (_playlistIndex + 1) % _ingameBgmData.bgmList.Length;
        }
    }
}
