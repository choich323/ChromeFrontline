using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private string _ingameSceneName;
    [SerializeField] private float _fadeDuration = 0.5f;
    
    public void LoadIngameScene(Action argCallbackDuringFade)
    {
        StartCoroutine(TransitionToScene(_ingameSceneName, argCallbackDuringFade, OnEnd));

        void OnEnd()
        {
            Managers.I.InitCameraController();
            Managers.UI.CreateTopHUD();
            Managers.Game.CreateGameField();
        }
    }

    private IEnumerator TransitionToScene(string argSceneName, Action argCallbackDuringFade = null, Action argCallback = null)
    {
        yield return Managers.UI.FadeOut(_fadeDuration).WaitForCompletion();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(argSceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        argCallbackDuringFade?.Invoke();

        yield return new WaitForSeconds(0.4f);
        
        Managers.UI.FadeIn(_fadeDuration);
        
        argCallback?.Invoke();
    }
}
