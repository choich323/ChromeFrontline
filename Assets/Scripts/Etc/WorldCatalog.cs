using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldCatalog", menuName = "Custom/WorldCatalog")]
public class WorldCatalog : ScriptableObject
{
    [Tooltip("게임에 존재하는 월드 ID들을 순서대로 배치합니다.")]
    public List<string> availableWorldIds = new List<string>();

    public int GetWorldCount() => availableWorldIds.Count;
    public string GetWorldId(int argIndex)
    {
        if (argIndex >= 0 && argIndex < availableWorldIds.Count)
            return availableWorldIds[argIndex];
        return string.Empty;
    }
    public int GetWorldIndex(string worldId) => availableWorldIds.IndexOf(worldId);
}