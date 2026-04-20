using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class GameFieldInfo : APrefabInfo
{
    
}

[CreateAssetMenu(fileName = "GameFieldData", menuName = "Custom/GameFieldData")]
public class GameFieldData : APrefabData
{
    public List<GameFieldInfo> GameFieldInfoList = new List<GameFieldInfo>();

    public override IEnumerable<APrefabInfo> GetInfoList()
    {
        return GameFieldInfoList;
    }
}
