using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private List<APrefabData> _dataList;
    [SerializeField] private StringData _stringData;
    
    private Dictionary<int, APrefabInfo> _prefabDataDict = new Dictionary<int, APrefabInfo>();
    private Dictionary<int, LocalizationText> _stringDataDict = new Dictionary<int, LocalizationText>();
    
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
                
                if (!_prefabDataDict.ContainsKey(key))
                {
                    _prefabDataDict[key] = info;
                }
            }
        }

        foreach (var info in _stringData.GetInfoList())
        {
            var id = (int)ConvertStringToStringID(info.id);
            _stringDataDict.Add(id, info.value);
        }
    }

    PrefabID ConvertStringToPrefabID(string argPrefabId)
    {
        if (Enum.TryParse(argPrefabId, true, out PrefabID prefabId))
        {
            return prefabId;
        }
        Debug.LogError("Invalid prefab ID");
        return PrefabID.None;
    }

    StringID ConvertStringToStringID(string argStringId)
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
        return _prefabDataDict.TryGetValue(argId, out outInfo);
    }

    public bool TryGetString(string argId, out string outString)
    {
        var id = (int)ConvertStringToStringID(argId);
        return TryGetString(id, out outString);
    }
    
    public bool TryGetString(int argId, out string outString)
    {
        outString = string.Empty;
        bool isFind = _stringDataDict.TryGetValue(argId, out var data);
        if (data != null)
        {
            var lang = Managers.Language.CurrentLanguage;
            switch (lang)
            {
                default:
                case Language.English:
                    outString = data.en;
                    break;
                case Language.Korean:
                    outString = data.kr;
                    break;
            }
        }
        return isFind;
    }
}
