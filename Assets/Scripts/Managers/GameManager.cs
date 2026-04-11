using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const ulong INVALID_UID = 0;
    private const int DEFAULT_SLOT_COUNT = 1;
    private const int DEFAULT_GAME_SPEED = 1;
    
    private ulong _uid = INVALID_UID;
    private int _slotCountMax = DEFAULT_SLOT_COUNT;
    private int _curGameSpeed = DEFAULT_GAME_SPEED;
    private float _elapsedPlayTime = 0f;
    private GameField _gameField;
    private bool _isPaused = false;
    private event Action _onGamePause;
    private event Action _onGameResume;
    private AIScheduleHandler _aiScheduleHandler;
    
    public GameField GameField => _gameField;
    public ulong CurUid => _uid;
    public int SlotCountMax => _slotCountMax;
    public bool IsGameOver => _gameField.IsGameOver();
    public bool IsPaused => _isPaused;
    public float PlayTime => _elapsedPlayTime;
    public AIScheduleHandler AIScheduleHandler => _aiScheduleHandler;

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
        UpdateTimer();
        UpdateAIScheduleHandler();
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
        var gameFieldObj = Managers.Pool.Instantiate(PrefabID.GameField);
        if (gameFieldObj == null)
        {
            Debug.LogError("Game field could not be instantiated.");
            return;
        }
        
        // TODO: game start 버튼을 누르면 실행되도록 수정 필요
        StartAIScheduleHandler();
        _gameField = gameFieldObj.GetComponent<GameField>();
        _gameField.Init();
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
    
    public void EndStage(bool isPlayerWin)
    {
        if(isPlayerWin)
            Debug.Log($"You Win!");
        else
            Debug.Log($"You Lose!");
        
        PauseGame();
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
        StartAIScheduleHandler();
        _gameField.Restart();
    }

    public void ForceSpawn(List<SpawnRequest> argSpawnRequestList)
    {
        GameField.EnemyHq.ForceSpawn(argSpawnRequestList);
    }
}
