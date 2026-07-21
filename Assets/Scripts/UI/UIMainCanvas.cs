using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIMainCanvas : MonoBehaviour
{
    private static UIMainCanvas _instance;
    public static UIMainCanvas I => _instance;
    
    [SerializeField] private Button _btnStartGame;
    [SerializeField] private GameObject _initSceneBg;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        _btnStartGame.onClick.AddListener(OnClickStartGame);
        _btnStartGame.gameObject.SetActive(false);
        StartCoroutine(CoCheckGameStart());
    }

    void OnClickStartGame()
    {
        Managers.Transition.LoadIngameScene(DisableInitSceneBg);
    }

    void DisableInitSceneBg()
    {
        _initSceneBg.SetActive(false);
    }
    
    IEnumerator CoCheckGameStart()
    {
        yield return null;

        while (!Managers.I.IsSystemInitialized)
        {
            yield return null;
        }
        
        _btnStartGame.gameObject.SetActive(true);
    }
}
