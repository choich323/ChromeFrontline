using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private const float DEFAULT_SENSITIVITY = 30f;
    private const float DEFAULT_SLIDING_AMOUNT = 10.0f;
    
    [Header("Boundary Settings")]
    [SerializeField] private float _zoomOutMinX = 0f;    
    [SerializeField] private float _zoomOutMaxX = 20f;
    [SerializeField] private float _zoomInMinX = -10f;
    [SerializeField] private float _zoomInMaxX = 30f;
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 3f;
    [SerializeField] private Camera _mainCam;

    [Range(1f, 100f)]
    [SerializeField] private float _smoothSpeed = DEFAULT_SENSITIVITY;
    [Range(0f, 50f)]
    [SerializeField] private float _slidingAmount = DEFAULT_SLIDING_AMOUNT;

    [SerializeField] private Vector3 _defaultPos;
    
    [Header("Zoom Settings")]
    [SerializeField] private float _minZoom = 6f;
    [SerializeField] private float _maxZoom = 10f;
    [SerializeField] private float _zoomSpeed = 15f; 

    [Header("Zoom Y-Offset Settings (Auto Alignment)")]
    [SerializeField] private float _zoomInY = 1.0f;
    [SerializeField] private float _zoomOutY = 3.0f;
    
    [Header("Platform Specific Sensitivity")]
    [SerializeField] private float _pcZoomSensitivity = 2f;
    [SerializeField] private float _mobileZoomSensitivity = 0.01f;
    
    private Vector2 _lastScreenPos;
    private bool _isDragging = false;
    private bool _wasZooming = false;
    
    private float _targetX;
    private float _lastDeltaX;

    private float _targetZoom;
    
    public float Sensitivity => _smoothSpeed;
    
    public void Init()
    {
        if (_mainCam == null)
            _mainCam = Camera.main;
        
        _targetX = transform.position.x;
        _targetZoom = _mainCam.orthographicSize;
    }

    void Update()
    {
        HandleInput();
    }

    void LateUpdate()
    {
        MoveCamera();
        ApplyZoomSmoothly();
    }

    public void ResetCamPos()
    {
        _targetX = _defaultPos.x;
        transform.position = _defaultPos;
    }

    void HandleInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandlePCInput();
#elif UNITY_ANDROID || UNITY_IOS
        HandleMobileInput();
#endif
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void HandlePCInput()
    {
        // 1. 줌 입력 처리
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            ApplyZoomDelta(-scrollInput * _pcZoomSensitivity);
        }

        // 2. 드래그 입력 처리
        Vector2 screenPos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            BeginDrag(screenPos);
        }
        else if (Input.GetMouseButton(0))
        {
            ProcessDrag(screenPos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }
#endif

#if UNITY_ANDROID || UNITY_IOS
    void HandleMobileInput()
    {
        if (Input.touchCount == 0) return;

        // 1. 줌 입력 처리 (최우선)
        if (Input.touchCount >= 2)
        {
            _isDragging = false;
            _wasZooming = true;
            HandlePinchZoom();
            return;
        }

        // 2. 드래그 입력 처리
        Touch touch = Input.GetTouch(0);
        Vector2 screenPos = touch.position;

        if (_wasZooming)
        {
            _wasZooming = false;
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // 줌 종료 직후 드래그 강제 시작 (UI 체크 생략)
                _lastScreenPos = screenPos;
                _isDragging = true;
            }
            return;
        }

        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginDrag(screenPos);
                break;
            case TouchPhase.Moved:
                ProcessDrag(screenPos);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndDrag();
                break;
        }
    }

    private void HandlePinchZoom()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
        float touchDeltaMag = (touch0.position - touch1.position).magnitude;

        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        ApplyZoomDelta(deltaMagnitudeDiff * _mobileZoomSensitivity);
    }
#endif
    
    void ApplyZoomDelta(float argZoomDelta)
    {
        _targetZoom += argZoomDelta;
        _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
        
        ClampTargetX();
    }
    
    void BeginDrag(Vector2 argScreenPos)
    {
        if (IsPointerOverUI())
        {
            // UI 터치 시 드래그 차단 및 비정상적인 상태 초기화 (안전장치)
            _isDragging = false; 
        }
        else
        {
            _lastScreenPos = argScreenPos;
            _isDragging = true;
        
            // 관성 이동 중 화면을 터치했을 때 즉시 멈추도록 현재 좌표로 타겟 동기화
            _targetX = transform.position.x; 
        }
    }
    
    void ProcessDrag(Vector2 argScreenPos)
    {
        if (!_isDragging)
            return;

        Vector3 lastWorldPos = ScreenToWorld(_lastScreenPos);
        Vector3 curWorldPos = ScreenToWorld(argScreenPos);
        
        _lastDeltaX = lastWorldPos.x - curWorldPos.x;
        _targetX += _lastDeltaX;
        
        ClampTargetX();
        
        _lastScreenPos = argScreenPos;
    }

    void EndDrag()
    {
        if (!_isDragging) return;

        // 관성(Sliding) 적용
        _targetX += _lastDeltaX * _slidingAmount;
        ClampTargetX();
        
        _isDragging = false;
    }
    
    void ClampTargetX()
    {
        // 1. 현재 타겟 줌이 전체 줌 범위(_minZoom ~ _maxZoom) 중 어느 비율에 있는지 계산 (0.0 ~ 1.0)
        float zoomT = Mathf.InverseLerp(_minZoom, _maxZoom, _targetZoom);
    
        // 2. 비율에 맞춰 현재 허용되는 최소/최대 X값을 보간
        float currentMinX = Mathf.Lerp(_zoomInMinX, _zoomOutMinX, zoomT);
        float currentMaxX = Mathf.Lerp(_zoomInMaxX, _zoomOutMaxX, zoomT);
    
        // 3. 계산된 동적 범위를 기준으로 _targetX 제한
        _targetX = Mathf.Clamp(_targetX, currentMinX, currentMaxX);
    }
    
    void MoveCamera()
    {
        float zoomT = Mathf.InverseLerp(_minZoom, _maxZoom, _mainCam.orthographicSize);
        float autoTargetY = Mathf.Lerp(_zoomInY, _zoomOutY, zoomT);
        
        // Time.deltaTime을 곱해 프레임 변동폭 보정. 
        float smoothedX = Mathf.Lerp(transform.position.x, _targetX, _smoothSpeed * Time.unscaledDeltaTime);
        float smoothedY = Mathf.Lerp(transform.position.y, autoTargetY, _smoothSpeed * Time.unscaledDeltaTime);
        transform.position = new Vector3(smoothedX, smoothedY, transform.position.z);
    }

    void ApplyZoomSmoothly()
    {
        if (Mathf.Abs(_mainCam.orthographicSize - _targetZoom) > 0.01f)
        {
            _mainCam.orthographicSize = Mathf.Lerp(_mainCam.orthographicSize, _targetZoom, Time.unscaledDeltaTime * _zoomSpeed);
        }
    }
    
    Vector3 ScreenToWorld(Vector2 argScreenPos)
    {
        float zDepth = Mathf.Abs(_mainCam.transform.position.z);
        return _mainCam.ScreenToWorldPoint(new Vector3(argScreenPos.x, argScreenPos.y, zDepth));
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) 
            return false;

#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        return false;
#endif
        
#if UNITY_STANDALONE || UNITY_EDITOR
        // PC/에디터 환경: 1프레임 딜레이를 무시하고 마우스 위치에 물리적으로 직접 레이캐스트를 쏨
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        return results.Count > 0;
#endif
    }

    public void SetSensitivity(float argSensitivity)
    {
        _smoothSpeed = argSensitivity;
        Managers.Prefs.SetSensitivity(argSensitivity);
    }
}