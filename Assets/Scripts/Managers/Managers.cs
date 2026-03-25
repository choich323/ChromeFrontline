using System;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers I => _instance;
    
    private PoolManager _poolManager;
    private DataManager _dataManager;
    private GameManager _gameManager;
    private UIManager _uiManager;
    private StringManager _stringManager;
    private LanguageManager _languageManager;
    private SoundManager _soundManager;
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
        
        _dataManager = GetComponent<DataManager>();
        _dataManager.Init();
        
        _poolManager = GetComponent<PoolManager>();
        _poolManager.Init();
        
        _gameManager = GetComponent<GameManager>();
        _gameManager.Init();
        
        _uiManager = GetComponent<UIManager>();
        _uiManager.Init();
        
        _stringManager = GetComponent<StringManager>();
        _stringManager.Init();

        _languageManager = GetComponent<LanguageManager>();
        _languageManager.Init();
        
        _soundManager = GetComponent<SoundManager>();
        _soundManager.Init();
    }
}
