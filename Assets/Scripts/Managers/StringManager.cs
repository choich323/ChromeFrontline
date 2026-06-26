using System;
using System.Linq;
using UnityEngine;

public class StringManager : MonoBehaviour
{
    public void Init()
    {
        
    }

    public string GetString(string argStringId)
    {
        if (Managers.Data.TryGetString(argStringId, out string str))
        {
            return str;
        }
        Debug.LogError($"No string found for ID. ID:{argStringId}");
        return string.Empty;
    }
    
    public string GetString(StringID argStringId)
    {
        var id = (int)argStringId;
        if (Managers.Data.TryGetString(id, out string str))
        {
            return str;
        }
        Debug.LogError($"No string found for ID. ID:{id}");
        return string.Empty;
    }

    public string GetString(StringID argStringId, params object[] argArguments)
    {
        string str = GetString(argStringId);
        try
        {
            return string.Format(str, argArguments);
        }
        catch (FormatException)
        {
            Debug.LogError($"String not found for ID. ID:{argStringId}");
            return string.Empty;
        }
    }
    
    /// <summary>
    /// 지정한 월드와 스테이지의 타이틀을 반환합니다.
    /// </summary>
    public string GetStageTitle(string argWorldId, int argStage)
    {
        var data = Managers.Data.GetOrLoadStoryData(argWorldId);
        var info = data.storyInfoList.FirstOrDefault(info => info.stage == argStage);
        if (info == null) return "";

        // 언어 설정 매니저와 연동
        var lang = Managers.Language.CurrentLanguage;
        return lang == Language.Korean ? info.title.kr : info.title.en;
    }

    /// <summary>
    /// 지정한 월드와 스테이지의 설명을 반환합니다.
    /// </summary>
    public string GetStageDesc(string argWorldId, int argStage)
    {
        var data = Managers.Data.GetOrLoadStoryData(argWorldId);
        var info = data.storyInfoList.FirstOrDefault(info => info.stage == argStage);
        if (info == null) return "";

        var lang = Managers.Language.CurrentLanguage;
        return lang == Language.Korean ? info.desc.kr : info.desc.en;
    }
}
