using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const ulong INVALID_UID = 0;
    private const int DEFAULT_SLOT_COUNT = 2;
    private const int DEFAULT_GAME_SPEED = 1;
    
    private ulong _uid = INVALID_UID;
    private int _slotCountMax = DEFAULT_SLOT_COUNT;
    private int _curGameSpeed = DEFAULT_GAME_SPEED;
    private GameField _gameField;
    private bool _isPaused = false;
    private List<PrefabID> _unlockEntityIDList = new List<PrefabID>();
    private event Action _onGamePause;
    private event Action _onGameResume;
    
    public GameField GameField => _gameField;
    public ulong CurUid => _uid;
    public int SlotCountMax => _slotCountMax;
    public bool IsGameOver => _gameField.IsGameOver();
    public bool IsPaused => _isPaused;

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
    }

    void CheckEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // TODO: 게임 종료 팝업 호출
        }
    }
    
    public void Init()
    {
        var gameFieldObj = Managers.Pool.Instantiate(PrefabID.GameField);
        if (gameFieldObj == null)
        {
            Debug.LogError("Game field could not be instantiated.");
            return;
        }
        _gameField = gameFieldObj.GetComponent<GameField>();
        _gameField.Init();
        
        ResetUnlockEntityIdList();
    }

    void ResetUnlockEntityIdList()
    {
        // TODO: 임시로 넣었지만, 시작 엔티티 데이터를 만들어서 구성해야 할듯?
        _unlockEntityIDList.Add(PrefabID.Pioneer);
        _unlockEntityIDList.Add(PrefabID.Alien);
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

    public IEnumerable<PrefabID> GetUnlockEntityIDList()
    {
        return _unlockEntityIDList;
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
        ResetUnlockEntityIdList();
        _gameField.ResetField();
        _gameField.Init();
    }
}
