using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryInfo
{
    public int stage;
    public LocalizationText title;
    public LocalizationText desc;
}

[CreateAssetMenu(fileName = "StoryData", menuName = "Custom/StoryData")]
public class StoryData : ScriptableObject
{
    public string worldId;
    public List<StoryInfo> storyInfoList = new List<StoryInfo>();

    public IEnumerable<StoryInfo> GetInfoList()
    {
        return storyInfoList;
    }
}
