using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStageNode : MonoBehaviour
{
    [Header("=== Editor Baking Info ===")]
    [Tooltip("이 노드에 연결될 스테이지 ID (예: STG_1_1)")]
    [SerializeField] private string _targetStageId; 

    [Header("=== UI References ===")]
    [SerializeField] private TextMeshProUGUI _stageNameText;
    [SerializeField] private Button _nodeButton;
    [SerializeField] private GameObject[] _starIcons; // 달성도에 따라 켜고 끌 별점 이미지 3개
    
    public string TargetStageId => _targetStageId;
    
    public void Init(StageInfo argStageInfo, StageSaveInfo argSaveInfo, System.Action<StageInfo> argOnClick)
    {
        _targetStageId = argStageInfo.stageId;
        _stageNameText.text = argStageInfo.stageName;

        // 별점 세팅
        int starCount = argSaveInfo?.starCount ?? 0;
        for (int i = 0; i < _starIcons.Length; i++)
        {
            _starIcons[i].SetActive(i < starCount);
        }

        // 버튼 이벤트 연결
        _nodeButton.onClick.RemoveAllListeners();
        _nodeButton.onClick.AddListener(() => argOnClick?.Invoke(argStageInfo));
    }
}
