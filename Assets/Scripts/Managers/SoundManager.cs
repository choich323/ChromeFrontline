using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private BGMData _ingameBgmData;
    [SerializeField] private BGMData _outgameBgmData;
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    [SerializeField] private AudioClip _selectSfxClip;
    [SerializeField] private AudioClip _upgradeSuccessSfxClip;
    [SerializeField] private AudioClip _upgradeFailSfxClip;
    
    private int _bgmPlaylistIndex = 0;
    private Coroutine _bgmPlaylistCoroutine;
    
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

        StopBgm();

        _bgmPlaylistCoroutine = StartCoroutine(CoPlayBgm(_ingameBgmData));
    }

    public void PlayOutgameBgm()
    {
        if (_outgameBgmData == null || _outgameBgmData.bgmList.Length == 0) return;
        
        StopBgm();
        
        _bgmPlaylistCoroutine = StartCoroutine(CoPlayBgm(_outgameBgmData));
    }

    public void StopBgm()
    {
        if (_bgmPlaylistCoroutine != null)
        {
            StopCoroutine(_bgmPlaylistCoroutine);
            _bgmPlaylistCoroutine = null;
        }
        _bgmSource.Stop();
        _bgmPlaylistIndex = 0;
        _bgmSource.loop = false; 
    }

    IEnumerator CoPlayBgm(BGMData argBGMData)
    {
        while (true)
        {
            // 이름으로 찾지 않고, SO 배열의 인덱스에서 직접 AudioClip을 꺼내옵니다.
            AudioClip clip = argBGMData.bgmList[_bgmPlaylistIndex].clip;

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
            _bgmPlaylistIndex = (_bgmPlaylistIndex + 1) % argBGMData.bgmList.Length;
        }
    }
    
    public void PlaySelectSfx()
    {
        _sfxSource.PlayOneShot(_selectSfxClip);
    }

    public void PlayUpgradeSfx(bool argIsSuccess)
    {
        _sfxSource.PlayOneShot(argIsSuccess ? _upgradeSuccessSfxClip : _upgradeFailSfxClip);
    }
}
