using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private List<APrefabData> _dataList;
    [SerializeField] private StringData _stringData;
    [SerializeField] private AIScheduleData _aiScheduleData;
    [SerializeField] private PlayerCurrencyData _playerCurrencyData;
    
    private Dictionary<int, APrefabInfo> _prefabInfoDict = new Dictionary<int, APrefabInfo>();
    private Dictionary<int, LocalizationText> _stringInfoDict = new Dictionary<int, LocalizationText>();
    private List<AIScheduleInfo> _aiScheduleInfoList = new List<AIScheduleInfo>();
    private List<EntityInfo> _pioneerInfoList = new List<EntityInfo>();
    private List<EntityInfo> _revoltInfoList = new List<EntityInfo>();
    private int _curGoldPerSecond;

    public int StartGold => _playerCurrencyData.startGold;
    public int CurGoldPerSecond => _curGoldPerSecond;
    
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
                if (info.prefab == null) continue;
                var key = (int)info.GetPrefabID();
                if (key == (int)PrefabID.None) continue;
                
                _prefabInfoDict.TryAdd(key, info);

                if (info is EntityInfo entityInfo)
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

        _curGoldPerSecond = _playerCurrencyData.goldPerSecond;
    }

    public PrefabID ConvertStringToPrefabID(string argPrefabId)
    {
        if (Enum.TryParse(argPrefabId, true, out PrefabID prefabId))
        {
            return prefabId;
        }
        Debug.LogError("Invalid prefab ID");
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

    public AIScheduleInfo GetAIScheduleInfo()
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
        return _revoltInfoList.FindAll(entity => entity.goldCost <= argTpAmount);
    }
}
