using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const ulong INVALID_UID = 0;
    private const int DEFAULT_STAGE = 1;
    private const float DEFAULT_GAME_SPEED = 1f;
    private const float ENTITY_ARRIVAL_GOLD_RATIO = 0.75f;
    
    private ulong _uid = INVALID_UID;
    private int _stage = DEFAULT_STAGE;
    private float _curGameSpeed = DEFAULT_GAME_SPEED;
    private float _elapsedPlayTime = 0f;
    private GameField _gameField;
    private bool _isPaused = false;
    private bool _isEnemyEmergencyTriggered = false;
    private bool _isInGame = false;
    private event Action _onGamePause;
    private event Action _onGameResume;
    private AIScheduleHandler _aiScheduleHandler;
    private SlotUpgradeHandler _slotUpgradeHandler;
    private UserRecord _userRecord;
    
    public GameField GameField => _gameField;
    public ulong CurUid => _uid;
    public int SlotCountMax => _gameField.PlayerHq.MaxSlotCount;
    public int CurSlotCount => _gameField.PlayerHq.GetSlotCount();
    public int Stage => _stage;
    public bool IsGameOver => _gameField.IsGameOver();
    public bool IsPaused => _isPaused;
    public bool IsInGame => _isInGame;
    public float CurGameSpeed  => _curGameSpeed;
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
        if (_isInGame)
        {
            UpdateTimer();
            UpdateAIScheduleHandler();
        }
    }

    public void ExitConfirm()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Managers.Sound.PlaySelectSfx();
            var popup = Managers.UI.PopupHandler.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
            var sm = Managers.String;
            string msg = sm.GetString(StringID.ConfirmExitStage);
            string confirm = sm.GetString(StringID.Yes);
            string cancel = sm.GetString(StringID.No);
            popup.SetData(msg, OnConfirm, OnBtnPopupClose, confirm, cancel, isModal:true);

            void OnConfirm()
            {
                Exit();
            }
            
            void OnBtnPopupClose()
            {
                Managers.Sound.PlaySelectSfx();
                Managers.UI.PopupHandler.ClosePopup();
            }
        }
    }

    void UpdateTimer()
    {
        _elapsedPlayTime += Time.deltaTime;
    }
    
    public void Init()
    {
        _userRecord = Managers.Save.LoadRecord();
        
        // TODO: 로비 제작 전이므로 바로 실행되도록 설정. 로비 제작 후 인게임 진입시 실행되도록 수정.
        var gameFieldObj = Managers.Pool.Instantiate(PrefabID.GameField);
        if (gameFieldObj == null)
        {
            Debug.LogError("Game field could not be instantiated.");
            return;
        }
        
        _isInGame = true;
        InitAIScheduleHandler();
        InitSlotUpgradeHandler();
        _gameField = gameFieldObj.GetComponent<GameField>();
        _gameField.Init();
    }

    public void SaveUserRecord(UserRecord argUserRecord)
    {
        _userRecord.Save(argUserRecord);
        var sm = Managers.Save;
        sm.SaveRecord(_userRecord);
    }

    void InitAIScheduleHandler()
    {
        if (_aiScheduleHandler != null)
        {
            _aiScheduleHandler.Destroy();
        }
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
    
    void InitSlotUpgradeHandler()
    {
        _slotUpgradeHandler = new SlotUpgradeHandler();
        _slotUpgradeHandler.Init();
    }

    public bool SetRandomGrade(int argCost, int argIndex)
    {
        var hq = GameField.PlayerHq;
        if (hq.Gold < argCost)
        {
            return false;
        }
        hq.ConsumeGold(argCost);
        var grade = _slotUpgradeHandler.GetRandomGrade();
        return hq.SetSlotGrade(argIndex, grade);
    }
    
    public ulong GetNewUid()
    {
        _uid++;
        return _uid;
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
        resultData.isClear = argIsPlayerWin;
        resultData.stage = _stage;
        popup.SetData(resultData);
    }

    public void SetGameSpeed(float argSpeed)
    {
        _curGameSpeed = argSpeed;
        if (!_isPaused)
        {
            Time.timeScale = argSpeed;
        }
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
        _elapsedPlayTime = 0f;
        _isEnemyEmergencyTriggered = false;
        Managers.UI.PopupHandler.CloseAllPopup();
        _curGameSpeed = DEFAULT_GAME_SPEED;
        _gameField.Restart();
        ResumeGame();
        InitAIScheduleHandler();
        Managers.UI.RefreshUI();
    }

    public void Exit()
    {
        PauseGame();
        _isInGame = false;
        Managers.UI.PopupHandler.CloseAllPopup();
        
        // TODO: 임시로 게임 종료로 해놨으나 스테이지 종료로 수정
        QuitGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ForceSpawn(SpawnRequest argSpawnRequest)
    {
        GameField.EnemyHq.ForceSpawn(argSpawnRequest);
    }

    public void OnEnemyHqHpChanged(int argHp, int argMaxHp)
    {
        if (_isEnemyEmergencyTriggered || _aiScheduleHandler == null) return;
        
        float hpRatio = argHp / (float)argMaxHp;
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
