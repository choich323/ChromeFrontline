using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataManager : MonoBehaviour
{
    private const string DEFAULT_AI_SCHEDULEINFO = "normal";
    
    [SerializeField] private List<APrefabData> _dataList;
    [SerializeField] private StringData _stringData;
    [SerializeField] private AIScheduleData _aiScheduleData;
    [SerializeField] private PlayerCurrencyData _playerCurrencyData;
    [SerializeField] private HeadQuarterUpgradeData _hqUpgradeData;
    [SerializeField] private AddSlotCostData _addSlotCostData;
    [SerializeField] private GradeData _gradeData;
    [SerializeField] private GameSpeedData _gameSpeedData;
    [SerializeField] private WorldCatalog _worldCatalog;
    
    private Dictionary<int, APrefabInfo> _prefabInfoDict = new Dictionary<int, APrefabInfo>();
    private Dictionary<int, LocalizationText> _stringInfoDict = new Dictionary<int, LocalizationText>();
    private List<AIScheduleInfo> _aiScheduleInfoList = new List<AIScheduleInfo>();
    private List<EntityInfo> _pioneerInfoList = new List<EntityInfo>();
    private List<EntityInfo> _revoltInfoList = new List<EntityInfo>();
    private List<HeadQuarterUpgradeInfo> _hqUpgradeInfoList = new List<HeadQuarterUpgradeInfo>();
    private List<AddSlotCostInfo> _addSlotCostInfoList = new List<AddSlotCostInfo>();
    private List<GradeInfo> _gradeInfoList = new List<GradeInfo>();
    private List<GameSpeedInfo> _gameSpeedInfoList = new List<GameSpeedInfo>();

    // stage data
    private StageData _curWorldData = null;
    private AsyncOperationHandle _worldDataHandle;
    
    // story data
    private StoryData _curStoryData = null;
    private string _curStoryWorldId = string.Empty;
    private AsyncOperationHandle<StoryData> _storyDataHandle;

    public int StartGold => _playerCurrencyData.startGold;
    public StageData CurWorldData => _curWorldData;
    public WorldCatalog WorldCatalog => _worldCatalog;
    
    public void Init()
    {
        InitialMapping();
    }

    void InitialMapping()
    {
        foreach (var data in _dataList)
        {
            if (data == null) continue;
            foreach (var info in data.GetInfoList())
            {
                if (info == null) continue;
                
                var entityInfo = info as EntityInfo;
                var isEntityInfo = entityInfo != null;
                var key = isEntityInfo ? entityInfo.GetEntityID() : info.GetPrefabID();
                if (key.Equals(PrefabID.None)) continue;
                
                _prefabInfoDict.TryAdd((int)key, info);

                if (isEntityInfo)
                {
                    if (entityInfo.camp == CampType.Pioneer)
                    {
                        _pioneerInfoList.Add(entityInfo);
                    }
                    else if (entityInfo.camp == CampType.Revolt)
                    {
                        _revoltInfoList.Add(entityInfo);
                    }
                }
            }
        }

        foreach (var info in _stringData.GetInfoList())
        {
            var id = (int)ConvertStringToStringID(info.id);
            _stringInfoDict.TryAdd(id, info.value);
        }

        foreach (var info in _aiScheduleData.GetScheduleInfoList())
        {
            _aiScheduleInfoList.Add(info);
        }

        foreach (var info in _hqUpgradeData.GetInfoList())
        {
            _hqUpgradeInfoList.Add(info);
        }

        foreach (var info in _addSlotCostData.GetInfoList())
        {
            _addSlotCostInfoList.Add(info);
        }

        foreach (var info in _gradeData.GetInfoList())
        {
            _gradeInfoList.Add(info);
        }

        foreach (var info in _gameSpeedData.GetInfoList())
        {
            _gameSpeedInfoList.Add(info);
        }
    }

    public PrefabID ConvertStringToPrefabID(string argPrefabId)
    {
        if (Enum.TryParse(argPrefabId, true, out PrefabID prefabId))
        {
            return prefabId;
        }
        Debug.LogError($"Invalid prefab ID. ID:{argPrefabId}");
        return PrefabID.None;
    }

    public StringID ConvertStringToStringID(string argStringId)
    {
        if (Enum.TryParse(argStringId, true, out StringID stringId))
        {
            return stringId;
        }
        Debug.LogError($"Invalid string ID. ID:{argStringId}");
        return StringID.None;
    }
    
    public bool TryGetPrefabInfo(int argId, out APrefabInfo outInfo)
    {
        return _prefabInfoDict.TryGetValue(argId, out outInfo);
    }

    public bool TryGetString(string argId, out string outString)
    {
        var id = (int)ConvertStringToStringID(argId);
        return TryGetString(id, out outString);
    }
    
    public bool TryGetString(int argId, out string outString)
    {
        outString = string.Empty;
        bool isFind = _stringInfoDict.TryGetValue(argId, out var info);
        if (info != null)
        {
            outString = Managers.Language.GetLocalizedString(info);
        }
        return isFind;
    }

    public AIScheduleInfo GetRandomAIScheduleInfo()
    {
        int randIndex = UnityEngine.Random.Range(0, _aiScheduleInfoList.Count);
        return _aiScheduleInfoList[randIndex];
    }

    public AIScheduleInfo GetAIScheduleInfo(string argScheduleId)
    {
        if (string.IsNullOrEmpty(argScheduleId))
        {
            argScheduleId = DEFAULT_AI_SCHEDULEINFO;
        }
        return _aiScheduleInfoList.Find(info => info.id == argScheduleId);
    }
    
    public IEnumerable<EntityInfo> GetPioneerInfoList()
    {
        return _pioneerInfoList;
    }

    public IEnumerable<EntityInfo> GetRevoltInfoList()
    {
        return _revoltInfoList;
    }

    public List<EntityInfo> GetRevoltInfoList(int argTpAmount)
    {
        return _revoltInfoList.FindAll(entity => entity.goldCost > 0 && entity.goldCost <= argTpAmount);
    }

    public HeadQuarterUpgradeInfo GetHeadQuarterUpgradeInfo(int argTier)
    {
        if (argTier > _hqUpgradeInfoList.Count)
        {
            return null;
        }
        return _hqUpgradeInfoList[argTier - 1];
    }

    public List<PrefabID> GetPrefabIdList(int argTier)
    {
        return _pioneerInfoList.Where(entity => entity.tier == argTier).Select(item => item.GetEntityID()).ToList(); 
    }

    public int GetAddSlotCost(int argSlotNumber)
    {
        return _addSlotCostInfoList[argSlotNumber].cost;
    }

    public GradeInfo GetGradeInfo(Grade argGrade)
    {
        return _gradeInfoList.Find(entity => entity.grade == argGrade);
    }

    public IEnumerable<GradeInfo> GetGradeInfoList()
    {
        return _gradeInfoList;
    }

    public GameSpeedInfo GetGameSpeedInfo(int argIndex)
    {
        if (argIndex >= _gameSpeedInfoList.Count)
        {
            argIndex = 0;
        }
        return _gameSpeedInfoList[argIndex];
    }

    /// <summary>
    /// 로비 매니저 등에서 호출하여 현재 필요한 월드 데이터를 어드레서블로 로드합니다.
    /// </summary>
    public void LoadWorldData(string argWorldId, Action<StageData> argOnComplete)
    {
        // 1. 이미 요청한 월드가 로드되어 있다면 즉시 반환
        if (_curWorldData != null && _curWorldData.worldId == argWorldId)
        {
            argOnComplete?.Invoke(_curWorldData);
            return;
        }

        // 2. 다른 월드 데이터가 메모리에 남아있다면 깔끔하게 언로드(해제)
        UnloadWorldData();

        // 3. 어드레서블 비동기 로드 실행
        Addressables.LoadAssetAsync<StageData>(argWorldId).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _worldDataHandle = handle;
                _curWorldData = handle.Result;
                
                Debug.Log($"[{argWorldId}] World Data Load Complete.");
                argOnComplete?.Invoke(_curWorldData);
            }
            else
            {
                Debug.LogError($"World Data Load Failed.: {argWorldId}");
                argOnComplete?.Invoke(null);
            }
        };
    }

    /// <summary>
    /// 현재 활성화된 월드 데이터를 메모리에서 안전하게 해제합니다.
    /// </summary>
    public void UnloadWorldData()
    {
        if (_worldDataHandle.IsValid())
        {
            Addressables.Release(_worldDataHandle);
            _curWorldData = null;
            Debug.Log("World Data Unload Complete.");
        }
        
        if (_storyDataHandle.IsValid())
        {
            Addressables.Release(_storyDataHandle);
            _curStoryData = null;
            _curStoryWorldId = string.Empty;
            Debug.Log("Story Data Unload Complete.");
        }
    }

    /// <summary>
    /// 로드된 현재 월드 데이터를 바탕으로 특정 스테이지의 해금 여부를 판별합니다.
    /// </summary>
    public bool IsStageUnlocked(StageInfo argStageInfo, UserRecord argUserRecord)
    {
        if (argUserRecord == null || _curWorldData == null) return false;

        // 1. 현재 월드의 1번 스테이지는 무조건 열어둠
        if (argStageInfo.stageIndex == 1) return true;

        // 2. 이미 클리어한 스테이지라면 무조건 해금
        var mySave = argUserRecord.GetStageSaveInfo(argStageInfo.stageIndex);
        if (mySave != null && mySave.isCleared) return true;

        // 3. 직전 스테이지(index - 1)를 클리어했는지 검사
        var prevStage = _curWorldData.GetStageInfoList()
            .FirstOrDefault(s => s.stageIndex == argStageInfo.stageIndex - 1);

        if (prevStage != null)
        {
            var prevSave = argUserRecord.GetStageSaveInfo(prevStage.stageIndex);
            return prevSave != null && prevSave.isCleared;
        }

        return false;
    }

    public int GetWorldIndex(string argWorldId)
    {
        return _worldCatalog.GetWorldIndex(argWorldId);
    }
    
    /// <summary>
    /// 내부용: 스토리 데이터를 가져오거나, 다른 월드면 교체 로드합니다.
    /// </summary>
    public StoryData GetOrLoadStoryData(string argWorldId)
    {
        // 1. 현재 로드된 월드의 데이터라면 즉시 반환 (성능 최적화)
        if (_curStoryData != null && _curStoryWorldId == argWorldId)
        {
            return _curStoryData;
        }

        // 2. 다른 월드 스토리가 메모리에 있다면 깔끔하게 해제
        if (_storyDataHandle.IsValid())
        {
            Addressables.Release(_storyDataHandle);
        }

        // 3. 새 월드 스토리 동기 로드
        string address = $"StoryData_{argWorldId}";
        _storyDataHandle = Addressables.LoadAssetAsync<StoryData>(address);
        _curStoryData = _storyDataHandle.WaitForCompletion();
        _curStoryWorldId = argWorldId;

        return _curStoryData;
    }
}
