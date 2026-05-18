using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSpeedInfo
{
    public int id;
    public float speed = 1;
    public Color color;
}

[CreateAssetMenu(fileName = "GameSpeedData", menuName = "Custom/GameSpeedData")]
public class GameSpeedData : ScriptableObject
{
    public List<GameSpeedInfo> speedInfoList;

    public IEnumerable<GameSpeedInfo> GetInfoList()
    {
        return speedInfoList;
    }
}
