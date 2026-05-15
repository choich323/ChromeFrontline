using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private const float MIN_BOUNDARY_X = -8f;
    private const float MAX_BOUNDARY_X = 30f;
    private const float DEFAULT_SENSITIVITY = 30f;
    private const float DEFAULT_SLIDING_AMOUNT = 10.0f;
    
    [Header("Boundary Settings")]
    [SerializeField] private float _minX = MIN_BOUNDARY_X;
    [SerializeField] private float _maxX = MAX_BOUNDARY_X;
    [SerializeField] private Camera _mainCam;

    [Range(1f, 100f)]
    [SerializeField] private float _smoothSpeed = DEFAULT_SENSITIVITY;
    [Range(0f, 50f)]
    [SerializeField] private float _slidingAmount = DEFAULT_SLIDING_AMOUNT;
    
    public float Sensitivity => _smoothSpeed;
    
    private Vector2 _lastScreenPos;
    private bool _isDragging = false;
    private float _targetX;
    private float _lastDeltaX;

    public void Init()
    {
        if (_mainCam == null) _mainCam = Camera.main;
        _targetX = transform.position.x;
    }

    void Update()
    {
        HandleInput();
    }

    void LateUpdate()
    {
        MoveCamera();
    }

    // 1. 입력 처리 및 드래그 계산 로직
    void HandleInput()
    {
        Vector2 screenPos = Input.mousePosition;
        bool wasPressed = Input.GetMouseButtonDown(0);
        bool isPressed = Input.GetMouseButton(0);
        bool wasReleased = Input.GetMouseButtonUp(0);

        // 드래그 시작
        if (wasPressed)
        {
            if (IsPointerOverUI())
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

        // 드래그 중
        if (isPressed && _isDragging)
        {
            Vector3 lastWorldPos = ScreenToWorld(_lastScreenPos);
            Vector3 curWorldPos = ScreenToWorld(screenPos);
            
            _lastDeltaX = lastWorldPos.x - curWorldPos.x;
            _targetX += _lastDeltaX;
            _targetX = Mathf.Clamp(_targetX, _minX, _maxX);
            
            _lastScreenPos = screenPos;
        }

        // 드래그 종료 및 관성 적용
        if (wasReleased && _isDragging)
        {
            _targetX += _lastDeltaX * _slidingAmount;
            _targetX = Mathf.Clamp(_targetX, _minX, _maxX);
            _isDragging = false;
        }
    }
    
    void MoveCamera()
    {
        // Time.deltaTime을 곱해 프레임 변동폭 보정. 
        // (기존 _smoothSpeed 수치 체감이 달라질 수 있으므로 뒤에 보정값(예: 10f)을 곱해줍니다)
        float smoothedX = Mathf.Lerp(transform.position.x, _targetX, _smoothSpeed * Time.deltaTime);
        transform.position = new Vector3(smoothedX, transform.position.y, transform.position.z);
    }

    Vector3 ScreenToWorld(Vector2 argScreenPos)
    {
        float zDepth = Mathf.Abs(_mainCam.transform.position.z);
        return _mainCam.ScreenToWorldPoint(new Vector3(argScreenPos.x, argScreenPos.y, zDepth));
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // 모바일 환경 (실기기): 터치 ID 기반 내장 함수가 가장 정확함
        if (Application.isMobilePlatform && Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        
        // PC/에디터 환경: 1프레임 딜레이를 무시하고 마우스 위치에 물리적으로 직접 레이캐스트를 쏨
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        return results.Count > 0;
    }

    public void SetSensitivity(float argSensitivity)
    {
        _smoothSpeed = argSensitivity;
        Managers.Prefs.SetSensitivity(argSensitivity);
    }
}