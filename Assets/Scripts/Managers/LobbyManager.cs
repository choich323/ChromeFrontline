using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("=== UI References ===")]
    [SerializeField] private ScrollRect _mapScrollRect;
    [SerializeField] private Transform _nodeContainer;       // Content 객체
    [SerializeField] private Image _bgImage;

    [Header("=== World Switching ===")]
    [SerializeField] private Button _btnPrevWorld;
    [SerializeField] private Button _btnNextWorld;
    [SerializeField] private TextMeshProUGUI _worldNameText;
    
    private List<UIStageNode> _nodeList = new List<UIStageNode>();

    void OnClickPrevWorld()
    {
        SwitchWorld(-1);
    }

    void OnClickNextWorld()
    {
        SwitchWorld(1);
    }

    public void Init()
    {
        // _btnPrevWorld.onClick.AddListener(OnClickPrevWorld);
        // _btnNextWorld.onClick.AddListener(OnClickNextWorld);
        
        RefreshLobbyMap();
    }
    
    public void RefreshLobbyMap()
    {
        UserRecord userRecord = Managers.Save.LoadRecord();
        if (userRecord == null) return;

        string targetWorldId = userRecord.CurrentWorldId;
        // UpdateWorldButtons(targetWorldId);

        // 어드레서블로 맵 데이터 비동기 로드
        Managers.Data.LoadWorldData(targetWorldId, (worldData) =>
        {
            if (worldData == null) return;

            SetMapBackgroundAndSize(worldData);
            
            // 노드 쫙 깔고, 카메라가 쳐다볼 타겟 노드 가져오기
            RectTransform targetNode = GenerateNodesAndFindTarget(worldData, userRecord);

            if (targetNode != null)
            {
                Canvas.ForceUpdateCanvases();
                FocusOnNode(targetNode);
            }
        });
    }

    /// <summary>
    /// Content의 Image 컴포넌트에 맵 스프라이트를 씌우고, 크기를 원본 해상도와 1:1로 맞춥니다.
    /// </summary>
    private void SetMapBackgroundAndSize(StageData argWorldData)
    {
        // StageData 안에 있는 맵 이미지(Sprite)를 할당합니다.
        // (StageData.cs에 public Sprite mapSprite; 필드가 있다고 가정합니다)
        _bgImage.sprite = argWorldData.bg;

        // ★ 유니티 마법의 함수: 현재 들어간 스프라이트의 픽셀 원본 사이즈로 RectTransform을 변경합니다. ★
        _bgImage.SetNativeSize();
    }
    
    RectTransform GenerateNodesAndFindTarget(StageData argWorldData, UserRecord argUserRecord)
    {
        // 1. 기존에 있던 노드들 청소
        foreach (var stageNode in _nodeList)
        {
            Managers.Pool.Destroy(stageNode, PrefabID.UIStageNode);
        }
        _nodeList.Clear();

        RectTransform focusTarget = null;
        int highestIndex = -1;
        
        // 2. 현재 월드의 데이터 순회
        foreach (var stageInfo in argWorldData.GetStageInfoList())
        {
            // 미해금 노드면 패스
            if (!Managers.Data.IsStageUnlocked(stageInfo, argUserRecord))
                continue;

            // 3. 노드 생성 및 구워둔 좌표 이식
            var obj = Managers.Pool.Instantiate(PrefabID.UIStageNode);
            var newNode = obj.GetComponent<UIStageNode>();
            _nodeList.Add(newNode);
            newNode.transform.SetParent(_nodeContainer);
            RectTransform rect = obj.transform as RectTransform;
            rect.anchoredPosition = stageInfo.uiPosition;

            // 4. 데이터 묶어주기 및 클릭 이벤트 연결
            StageSaveInfo saveInfo = argUserRecord.GetStageSaveInfo(stageInfo.stageId);
            newNode.Init(stageInfo, saveInfo, OnStageNodeClicked);

            // [포커싱 로직] 가장 최신 단계(index)의 노드를 타겟으로 갱신
            if (stageInfo.stageIndex > highestIndex)
            {
                highestIndex = stageInfo.stageIndex;
                focusTarget = rect;
            }
        }

        return focusTarget;
    }

    void FocusOnNode(RectTransform targetNode)
    {
        // 화면 중앙에 노드가 오도록 Content 위치 이동 (여백 클램핑 포함)
        Vector2 targetPos = -targetNode.anchoredPosition;
        RectTransform contentRect = _mapScrollRect.content;
        RectTransform viewportRect = _mapScrollRect.viewport;

        float xBound = Mathf.Max(0, (contentRect.rect.width - viewportRect.rect.width) / 2f);
        float yBound = Mathf.Max(0, (contentRect.rect.height - viewportRect.rect.height) / 2f);

        targetPos.x = Mathf.Clamp(targetPos.x, -xBound, xBound);
        targetPos.y = Mathf.Clamp(targetPos.y, -yBound, yBound);

        contentRect.anchoredPosition = targetPos;
    }

    void SwitchWorld(int direction)
    {
        UserRecord userRecord = Managers.Save.LoadRecord();
        var catalog = Managers.Data.WorldCatalog; // 카탈로그 호출
        
        int currentIndex = catalog.GetWorldIndex(userRecord.CurrentWorldId);
        if (currentIndex == -1) currentIndex = 0;

        int nextIndex = currentIndex + direction;
        
        // 카탈로그의 범위 내에 있는지 검사
        if (nextIndex >= 0 && nextIndex < catalog.GetWorldCount())
        {
            userRecord.SetCurrentWorldId(catalog.GetWorldId(nextIndex));
            Managers.Save.SaveRecord(userRecord); 
            RefreshLobbyMap(); 
        }
    }

    void UpdateWorldButtons(string currentWorldId)
    {
        var catalog = Managers.Data.WorldCatalog;
        int currentIndex = catalog.GetWorldIndex(currentWorldId);
        
        _btnPrevWorld.interactable = (currentIndex > 0);
        _btnNextWorld.interactable = (currentIndex < catalog.GetWorldCount() - 1);
        
        if (_worldNameText != null) _worldNameText.text = $"World {currentIndex + 1}";
    }

    void OnStageNodeClicked(StageInfo argStageInfo)
    {
        // 팝업 띄우고 데이터 넘기기
        
    }
}