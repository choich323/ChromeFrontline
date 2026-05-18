using System;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    private static Managers I => _instance;
    
    [SerializeField] private PoolManager _poolManager;
    [SerializeField] private DataManager _dataManager;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private StringManager _stringManager;
    [SerializeField] private LanguageManager _languageManager;
    [SerializeField] private SoundManager _soundManager;
    [SerializeField] private SaveManager _saveManager;
    
    private PlayerPrefsManager _prefsManager;
    private CameraController _cameraController;
    
    // 접근용 프로퍼티
    public static PoolManager Pool => I._poolManager;
    public static DataManager Data => I._dataManager;
    public static GameManager Game => I._gameManager;
    public static UIManager UI => I._uiManager;
    public static StringManager String => I._stringManager;
    public static LanguageManager Language => I._languageManager;
    public static SoundManager Sound => I._soundManager;
    public static PlayerPrefsManager Prefs => I._prefsManager;
    public static SaveManager Save => I._saveManager;

    public static CameraController CamController => I._cameraController;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitManagers();
            if (Camera.main != null)
            {
                _cameraController = Camera.main.GetComponent<CameraController>();
                _cameraController.Init();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitManagers()
    {
        _prefsManager = new PlayerPrefsManager();
        _prefsManager.Init();
        
        _dataManager.Init();
        
        _poolManager.Init();
        
        _gameManager.Init();
        
        _uiManager.Init();
        
        _stringManager.Init();

        _languageManager.Init();
        
        _soundManager.Init();
        
        _saveManager.Init();
    }
}
