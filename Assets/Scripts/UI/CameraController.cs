using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private const float MIN_BOUNDARY_X = -8f;
    private const float MAX_BOUNDARY_X = 22f;
    private const float DEFAULT_SENSITIVITY = 0.22f;
    private const float DEFAULT_SLIDING_AMOUNT = 3.0f;
    
    [Header("Boundary Settings")]
    [SerializeField] private float _minX = MIN_BOUNDARY_X;
    [SerializeField] private float _maxX = MAX_BOUNDARY_X;
    [SerializeField] private Camera _mainCam;

    [Range(0.01f, 1f)]
    [SerializeField] private float _smoothSpeed = DEFAULT_SENSITIVITY; // 낮을수록 부드러움
    [Range(0f, 50f)]
    [SerializeField] private float _slidingAmount = DEFAULT_SLIDING_AMOUNT; // 슬라이딩 강도
    
    public float Sensitivity => _smoothSpeed;
    
    private Vector2 _lastScreenPos;
    private bool _isDragging = false;
    private float _targetX;
    private float _lastDeltaX; // 마지막 프레임의 이동량

    private void Awake()
    {
        if(_mainCam == null)
            _mainCam = Camera.main;
        _targetX = transform.position.x;
        SetSensitivity(DEFAULT_SENSITIVITY);
    }

    void Update()
    {
        Vector2 screenPos = Input.mousePosition;
        // 방금 터치했는지
        bool wasPressed = Input.GetMouseButtonDown(0);
        // 지금 터치 중인지
        bool isPressed = Input.GetMouseButton(0);
        // 방금 터치를 해제했는지
        bool wasReleased = Input.GetMouseButtonUp(0);

        if (wasPressed)
        {
            if (IsPointerOverUI(screenPos))
            {
                _isDragging = false;
            }
            else
            {
                _lastScreenPos = screenPos;
                _isDragging = true;
                _targetX = transform.position.x;
            }
        }

        if (isPressed && _isDragging)
        {
            Vector3 lastWorldPos = ScreenToWorld(_lastScreenPos);
            Vector3 curWorldPos = ScreenToWorld(screenPos);
            
            // screen을 얼마나 이동시킨 것인지 계산
            _lastDeltaX = lastWorldPos.x - curWorldPos.x;
            _targetX += _lastDeltaX;
            _targetX = Mathf.Clamp(_targetX, _minX, _maxX);
            
            _lastScreenPos = screenPos;
        }

        if (wasReleased && _isDragging)
        {
            // 관성 가중치만큼 목적지를 재설정
            _targetX += _lastDeltaX * _slidingAmount;
            _targetX = Mathf.Clamp(_targetX, _minX, _maxX);
            _isDragging = false;
        }
        
        float smoothedX = Mathf.Lerp(transform.position.x, _targetX, _smoothSpeed);
        transform.position = new Vector3(smoothedX, transform.position.y, transform.position.z);
    }

    Vector3 ScreenToWorld(Vector2 argScreenPos)
    {
        return _mainCam.ScreenToWorldPoint(new Vector3(argScreenPos.x, argScreenPos.y, _mainCam.nearClipPlane));
    }

    bool IsPointerOverUI(Vector2 argScreenPos)
    {
        if (EventSystem.current == null) return false;
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = argScreenPos
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public void SetSensitivity(float argSensitivity)
    {
        _smoothSpeed = argSensitivity;
        Managers.Prefs.SetSensitivity(argSensitivity);
    }
}
