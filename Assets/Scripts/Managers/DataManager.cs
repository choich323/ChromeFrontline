using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private List<APrefabData> _dataList;
    [SerializeField] private StringData _stringData;
    [SerializeField] private AIScheduleData _aiScheduleData;
    
    private Dictionary<int, APrefabInfo> _prefabInfoDict = new Dictionary<int, APrefabInfo>();
    private Dictionary<int, LocalizationText> _stringInfoDict = new Dictionary<int, LocalizationText>();
    private List<AIScheduleInfo> _aiScheduleInfoList = new List<AIScheduleInfo>();
    
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
                var key = (int)ConvertStringToPrefabID(info.prefab.name);
                if (key == (int)PrefabID.None) continue;
                
                _prefabInfoDict.TryAdd(key, info);
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
        int randIndex = UnityEngine.Random.Range(0, _aiScheduleInfoList.Count - 1);
        return _aiScheduleInfoList[randIndex];
    }
}
