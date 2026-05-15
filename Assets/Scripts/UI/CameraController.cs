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
        SetSensitivity(DEFAULT_SENSITIVITY);
    }

    void Update()
    {
        HandleInput();
        MoveCamera();
    }

    // drag 상태가 멈추지 않는 경우 방어
    public void ResetDragging()
    {
        
    }

    // 1. 입력 처리 및 드래그 계산 로직
    private void HandleInput()
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
    
    private void MoveCamera()
    {
        float smoothedX = Mathf.Lerp(transform.position.x, _targetX, _smoothSpeed);
        transform.position = new Vector3(smoothedX, transform.position.y, transform.position.z);
    }

    private Vector3 ScreenToWorld(Vector2 argScreenPos)
    {
        float zDepth = Mathf.Abs(_mainCam.transform.position.z);
        return _mainCam.ScreenToWorldPoint(new Vector3(argScreenPos.x, argScreenPos.y, zDepth));
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Application.isMobilePlatform && Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void SetSensitivity(float argSensitivity)
    {
        _smoothSpeed = argSensitivity;
        Managers.Prefs.SetSensitivity(argSensitivity);
    }
}