using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIStageMapBaker : MonoBehaviour
{
    [Header("저장할 월드 데이터 에셋")]
    public StageData targetWorldData;

    [ContextMenu("Bake Stage Positions")]
    public void BakePositions()
    {
#if UNITY_EDITOR
        if (targetWorldData == null)
        {
            Debug.LogError("타겟 World Data가 연결되지 않았습니다!");
            return;
        }

        // 자식으로 배치된 모든 노드 UI를 찾습니다.
        UIStageNode[] nodes = GetComponentsInChildren<UIStageNode>();
        int bakeCount = 0;

        foreach (var node in nodes)
        {
            var stageInfo = targetWorldData.GetStageInfo(node.TargetStage);
            if (stageInfo != null)
            {
                // UI의 현재 좌표(anchoredPosition)를 추출해서 SO에 저장
                RectTransform rect = node.GetComponent<RectTransform>();
                stageInfo.BakePosition(rect.anchoredPosition);
                bakeCount++;
            }
            else
            {
                Debug.LogWarning($"SO에 [{node.TargetStage}] ID를 가진 스테이지 정보가 없습니다.");
            }
        }

        // 변경된 SO 데이터를 저장(Dirty)하여 파일에 반영합니다.
        EditorUtility.SetDirty(targetWorldData);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[{targetWorldData.worldId}] 총 {bakeCount}개의 스테이지 좌표가 성공적으로 구워졌습니다!");
#endif
    }
}