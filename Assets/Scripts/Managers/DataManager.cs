using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private List<APrefabData> _dataList;
    [SerializeField] private StringData _stringData;
    [SerializeField] private AIScheduleData _aiScheduleData;
    [SerializeField] private PlayerCurrencyData _playerCurrencyData;
    [SerializeField] private HeadQuarterUpgradeData _hqUpgradeData;
    [SerializeField] private AddSlotCostData _addSlotCostData;
    [SerializeField] private GradeData _gradeData;
    [SerializeField] private GameSpeedData _gameSpeedData;
    
    private Dictionary<int, APrefabInfo> _prefabInfoDict = new Dictionary<int, APrefabInfo>();
    private Dictionary<int, LocalizationText> _stringInfoDict = new Dictionary<int, LocalizationText>();
    private List<AIScheduleInfo> _aiScheduleInfoList = new List<AIScheduleInfo>();
    private List<EntityInfo> _pioneerInfoList = new List<EntityInfo>();
    private List<EntityInfo> _revoltInfoList = new List<EntityInfo>();
    private List<HeadQuarterUpgradeInfo> _hqUpgradeInfoList = new List<HeadQuarterUpgradeInfo>();
    private List<AddSlotCostInfo> _addSlotCostInfoList = new List<AddSlotCostInfo>();
    private List<GradeInfo> _gradeInfoList = new List<GradeInfo>();
    private List<GameSpeedInfo> _gameSpeedInfoList = new List<GameSpeedInfo>();
    
    public int StartGold => _playerCurrencyData.startGold;
    
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
            var lang = Managers.Language.CurrentLanguage;
            switch (lang)
            {
                default:
                case Language.English:
                    outString = info.en;
                    break;
                case Language.Korean:
                    outString = info.kr;
                    break;
            }
        }
        return isFind;
    }

    public AIScheduleInfo GetRandomAIScheduleInfo()
    {
        int randIndex = UnityEngine.Random.Range(0, _aiScheduleInfoList.Count);
        return _aiScheduleInfoList[randIndex];
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
}
