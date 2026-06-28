using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WorldMetaData
{
    public string worldId;
    public LocalizationText worldName;
}

[CreateAssetMenu(fileName = "WorldCatalog", menuName = "Custom/WorldCatalog")]
public class WorldCatalog : ScriptableObject
{
    public List<WorldMetaData> _worldList = new List<WorldMetaData>();

    public int GetWorldCount() => _worldList.Count;
    
    public string GetWorldId(int argIndex)
    {
        if (argIndex >= 0 && argIndex < _worldList.Count)
            return _worldList[argIndex].worldId;
        return string.Empty;
    }
    
    public string GetWorldName(int argIndex)
    {
        if (argIndex >= 0 && argIndex < _worldList.Count)
        {
            var worldName = _worldList[argIndex].worldName;
            return Managers.Language.GetLocalizedString(worldName);
        }

        return string.Empty;
    }

    public int GetWorldIndex(string argWorldId)
    {
        return _worldList.FindIndex(world => world.worldId == argWorldId);
    }
}