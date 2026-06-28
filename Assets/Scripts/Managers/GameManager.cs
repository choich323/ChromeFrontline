using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const ulong INVALID_UID = 0;
    private const int DEFAULT_STAGE = 1;
    private const float DEFAULT_GAME_SPEED = 1f;
    private const float ENTITY_ARRIVAL_GOLD_RATIO = 0.75f;
    
    private ulong _uid = INVALID_UID;
    private int _stage = DEFAULT_STAGE;
    private int _playedStageIndex = -1;
    private float _curGameSpeed = DEFAULT_GAME_SPEED;
    private float _elapsedPlayTime = 0f;
    private GameField _gameField;
    private bool _isPaused = false;
    private bool _isEnemyEmergencyTriggered = false;
    private bool _isInGame = false;
    private bool _wasAlreadyClearedBeforePlay = false;
    private bool _isNewStageUnlocked = false;
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
            popup.SetData(msg, OnConfirm, OnBtnPopupClose, confirm, cancel);

            void OnConfirm()
            {
                // 게임 종료 팝업
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
        _isInGame = false;
        
        var gameFieldObj = Managers.Pool.Instantiate(PrefabID.GameField);
        if (gameFieldObj == null)
        {
            Debug.LogError("Game field could not be instantiated.");
            return;
        }
        _gameField = gameFieldObj.GetComponent<GameField>();
    }
    
    public void SaveUserRecord(UserRecord argUserRecord)
    {
        _userRecord.Save(argUserRecord);
        var sm = Managers.Save;
        sm.SaveRecord(_userRecord);
    }

    void RunAIScheduleHandler(AIScheduleInfo argScheduleInfo)
    {
        if (_aiScheduleHandler != null)
        {
            _aiScheduleHandler.Destroy();
        }
        _aiScheduleHandler = new AIScheduleHandler();
        _aiScheduleHandler.Init(argScheduleInfo);
    }

    void ReRunAIScheduleHandler()
    {
        RunAIScheduleHandler(_aiScheduleHandler.ScheduleInfo);
    }

    void UpdateAIScheduleHandler()
    {
        if (_aiScheduleHandler != null)
        {
            _aiScheduleHandler.Update();
        }
    }
    
    void RunSlotUpgradeHandler()
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
    
    public void EnterStage(StageInfo argStageInfo)
    {
        if (_gameField == null)
        {
            return;
        }

        StartCoroutine(CoEnterStage(argStageInfo));
    }

    IEnumerator CoEnterStage(StageInfo argStageInfo)
    {
        yield return Managers.UI.FadeOut().WaitForCompletion();
        
        Managers.Lobby.ToggleLobby(false);
        
        _isInGame = true;
        _elapsedPlayTime = 0f;
        _stage = argStageInfo.stage;
        
        var saveInfo = _userRecord.GetStageSaveInfo(argStageInfo.stage);
        _wasAlreadyClearedBeforePlay = saveInfo != null && saveInfo.isCleared;
        _playedStageIndex = argStageInfo.stageIndex;
        _isNewStageUnlocked = false;
        
        var aiScheduleInfo = Managers.Data.GetAIScheduleInfo(argStageInfo.aiScheduleId);
        RunAIScheduleHandler(aiScheduleInfo);
        RunSlotUpgradeHandler();
        Managers.Sound.PlayIngameBgm();
        _gameField.Run();
        Managers.UI.OnEnterStage();
        Managers.CamController.ResetCamPos();
        PauseGame();
        
        yield return Managers.UI.FadeIn().WaitForCompletion();
        
        ResumeGame();
    }
    
    public void EndStage(bool argIsPlayerWin)
    {
        PauseGame();
        
        if (argIsPlayerWin && !_wasAlreadyClearedBeforePlay)
        {
            _isNewStageUnlocked = true;
        }

        var popup = Managers.UI.PopupHandler.OpenPopup<UIResult>(PrefabID.UIResult);
        popup.Init();
        var resultData = new ResultData();
        resultData.isClear = argIsPlayerWin;
        resultData.stage = _stage;
        popup.SetData(resultData);
    }

    public void ExitStage()
    {
        StartCoroutine(CoExitStage());
    }

    IEnumerator CoExitStage()
    {
        PauseGame();
        
        _isInGame = false;

        yield return Managers.UI.FadeOut().WaitForCompletion();
        
        Managers.UI.PopupHandler.CloseAllPopup();

        if (_aiScheduleHandler != null)
        {
            _aiScheduleHandler.Destroy();
        }
        
        ResetStage();
        
        Managers.Lobby.RefreshLobbyMap(_playedStageIndex, _isNewStageUnlocked);
        _playedStageIndex = -1;
        _isNewStageUnlocked = false;
        Managers.Sound.StopIngameBgm();
        Managers.UI.RefreshUI();
        
        _gameField.ResetField();
        
        Managers.UI.OnExitStage();
        Managers.Lobby.ToggleLobby(true);

        yield return Managers.UI.FadeIn().WaitForCompletion();
        
        ResumeGame();
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

    void ResetStage()
    {
        _uid = INVALID_UID;
        _elapsedPlayTime = 0f;
        _isEnemyEmergencyTriggered = false;
        SetGameSpeed(DEFAULT_GAME_SPEED);
    }
    
    public void RestartStage()
    {
        ResetStage();
        Managers.UI.PopupHandler.CloseAllPopup();

        ReRunAIScheduleHandler();
        RunSlotUpgradeHandler();
        
        _gameField.Run();
        ResumeGame();

        Managers.UI.RefreshUI();
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
