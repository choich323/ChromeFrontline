using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const ulong INVALID_UID = 0;
    private const int DEFAULT_SLOT_COUNT = 1;
    private const int DEFAULT_GAME_SPEED = 1;
    private const int DEFAULT_STAGE = 1;
    private const float ENTITY_ARRIVAL_GOLD_RATIO = 0.75f;
    
    private ulong _uid = INVALID_UID;
    private int _slotCountMax = DEFAULT_SLOT_COUNT;
    private int _curGameSpeed = DEFAULT_GAME_SPEED;
    private int _stage = DEFAULT_STAGE;
    private float _elapsedPlayTime = 0f;
    private GameField _gameField;
    private bool _isPaused = false;
    private bool _isEnemyEmergencyTriggered = false;
    private bool _isInGame = false;
    private event Action _onGamePause;
    private event Action _onGameResume;
    private AIScheduleHandler _aiScheduleHandler;
    private UserRecord _userRecord;
    
    public GameField GameField => _gameField;
    public ulong CurUid => _uid;
    public int SlotCountMax => _slotCountMax;
    public int Stage => _stage;
    public bool IsGameOver => _gameField.IsGameOver();
    public bool IsPaused => _isPaused;
    public bool IsInGame => _isInGame;
    public float PlayTime => _elapsedPlayTime;
    public AIScheduleHandler AIScheduleHandler => _aiScheduleHandler;
    public UserRecord UserRecord => _userRecord;
    
    public event Action OnGamePause
    {
        add => _onGamePause += value;
        remove => _onGamePause -= value;
    }
    
    public event Action OnGameResume
    {
        add => _onGameResume += value;
        remove => _onGameResume -= value;
    }

    void Update()
    {
        CheckEscapeKey();

        if (_isInGame)
        {
            UpdateTimer();
            UpdateAIScheduleHandler();
        }
    }

    void CheckEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // TODO: 게임 종료 팝업 호출
        }
    }

    void UpdateTimer()
    {
        _elapsedPlayTime += Time.deltaTime;
    }
    
    public void Init()
    {
        _userRecord = Managers.Save.LoadRecord();
        
        // TODO: game start 버튼을 누르면 실행되도록 수정 필요
        var gameFieldObj = Managers.Pool.Instantiate(PrefabID.GameField);
        if (gameFieldObj == null)
        {
            Debug.LogError("Game field could not be instantiated.");
            return;
        }
        
        _isInGame = true;
        StartAIScheduleHandler();
        _gameField = gameFieldObj.GetComponent<GameField>();
        _gameField.Init();
    }

    public void SaveUserRecord(UserRecord argUserRecord)
    {
        _userRecord.Save(argUserRecord);
        var sm = Managers.Save;
        sm.SaveRecord(_userRecord);
    }

    void StartAIScheduleHandler()
    {
        _aiScheduleHandler = new AIScheduleHandler();
        _aiScheduleHandler.Init();
    }

    void UpdateAIScheduleHandler()
    {
        if (_aiScheduleHandler != null)
        {
            _aiScheduleHandler.Update();
        }
    }
    
    public ulong GetNewUid()
    {
        _uid++;
        return _uid;
    }

    public void SetSlotCountMax(int argCount)
    {
        _slotCountMax = argCount;
    }

    public IEnumerable<PrefabID> GetPlayerUsableEntityIDList()
    {
        return GameField.PlayerHq.GetUsableEntityIDList();
    }
    
    public void EndStage(bool argIsPlayerWin)
    {
        PauseGame();

        var popup = Managers.UI.PopupHandler.OpenPopup<UIResult>(PrefabID.UIResult);
        popup.Init();
        var resultData = new ResultData();
        resultData.isVictory = argIsPlayerWin;
        resultData.stage = _stage;
        popup.SetData(resultData);
    }

    public void SetGameSpeed(int argSpeed)
    {
        _curGameSpeed = argSpeed;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        _isPaused = true;
        _onGamePause?.Invoke();
    }

    public void ResumeGame()
    {
        Time.timeScale = _curGameSpeed;
        _isPaused = false;
        _onGameResume?.Invoke();
    }

    public void RestartStage()
    {
        _uid = INVALID_UID;
        _slotCountMax = DEFAULT_SLOT_COUNT;
        _curGameSpeed = DEFAULT_GAME_SPEED;
        _elapsedPlayTime = 0f;
        _isEnemyEmergencyTriggered = false;
        Managers.UI.PopupHandler.CloseAllPopup();
        ResumeGame();
        StartAIScheduleHandler();
        _gameField.Restart();
    }

    public void ExitStage()
    {
        Debug.Log("out from stage.");
        _isInGame = false;
        Managers.UI.PopupHandler.CloseAllPopup();
        
        // TODO: 실제 씬 이름을 가져오도록 수정 필요
        // UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby Scene");
    }

    public void ForceSpawn(List<SpawnRequest> argSpawnRequestList)
    {
        GameField.EnemyHq.ForceSpawn(argSpawnRequestList);
    }

    public void OnEnemyHqHpChanged(int argHp, int argMaxHp)
    {
        if (_isEnemyEmergencyTriggered || _aiScheduleHandler == null) return;
        
        float hpRatio = argHp / argMaxHp;
        if (hpRatio <= _aiScheduleHandler.ScheduleInfo.emergencyHpThreshold)
        {
            _isEnemyEmergencyTriggered = true;
            Debug.Log("Emergency triggered!");
            _aiScheduleHandler.Emergency();
        }
    }
    
    public void OnEntityArrivedAtDestination(Team argTeam, int argDamage, long argEntityGold)
    {
        if (argTeam == Team.Player)
        {
            GameField.EnemyHq.OnHqDamaged(argDamage);
            GameField.PlayerHq.EarnGold((int)(argEntityGold * ENTITY_ARRIVAL_GOLD_RATIO));
        }
        else
        {
            GameField.PlayerHq.OnHqDamaged(argDamage);
        }
    }
}
