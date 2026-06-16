using System;
using System.Collections;
using UnityEngine;

public class BulletEffect : MonoBehaviour
{
    private const string ANIM_DEFAULT = "Default";
    
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Coroutine _coroutine;
    
    public void Init (RuntimeAnimatorController argController)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        if (argController == null)
        {
            Managers.Pool.Destroy(this, PrefabID.BulletEffect);
            return;
        }

        _animator.runtimeAnimatorController = argController;

        // 2. 애니메이션 재생 (기본 레이어의 0번째 기본 스테이트 실행)
        _animator.Play(ANIM_DEFAULT, 0, 0f);

        // 3. 현재 프레임에 컨트롤러 상태를 강제로 업데이트하여 
        // 변경된 오버라이드 애니메이션의 실제 재생 시간(length)을 정확히 가져옵니다.
        _animator.Update(0f);
        float duration = _animator.GetCurrentAnimatorStateInfo(0).length;

        _coroutine = StartCoroutine(CoInit(duration));
    }

    IEnumerator CoInit(float argDuration)
    {
        yield return new WaitForSeconds(argDuration);

        _coroutine = null;
        Managers.Pool.Destroy(this, PrefabID.BulletEffect);
    }

    void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }
}
