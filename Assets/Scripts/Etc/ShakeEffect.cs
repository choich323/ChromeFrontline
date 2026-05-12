using UnityEngine;
using DG.Tweening;

public class ShakeEffect : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private float _shakeStrengthX = 15f;
    [SerializeField] private float _shakeStrengthY = 15f;
    [SerializeField] private float _duration = 0.4f;
    [SerializeField] private int _vibrato = 10;

    public void PlayShakeAnimation()
    {
        _targetTransform.DOComplete();

        _targetTransform.DOShakePosition(
            _duration,
            strength: new Vector3(_shakeStrengthX, _shakeStrengthY, 0f),
            vibrato: _vibrato,
            randomness: 0f,
            snapping: false
        ).SetUpdate(true);
    }
}
