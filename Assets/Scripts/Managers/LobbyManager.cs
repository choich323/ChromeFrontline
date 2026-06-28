using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    private UserRecord _userRecord;

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
    
    public void RefreshLobbyMap(int argPlayedStageIndex = -1, bool argIsNewStageUnlocked = false)
    {
        ToggleLobby(true);
        
        _userRecord = Managers.Save.LoadRecord();
        if (_userRecord == null) return;

        string targetWorldId = _userRecord.CurrentWorldId;
        // UpdateWorldButtons(targetWorldId);

        // 어드레서블로 맵 데이터 비동기 로드
        Managers.Data.LoadWorldData(targetWorldId, (worldData) =>
        {
            if (worldData == null) return;

            SetMapBackground(worldData);
            
            // 노드 쫙 깔고, 카메라가 쳐다볼 타겟 노드 가져오기
            GenerateNodesAndFindTarget(worldData, _userRecord, argPlayedStageIndex, out RectTransform playedTarget, out RectTransform newTarget);

            if (playedTarget != null)
            {
                Canvas.ForceUpdateCanvases();
                
                // 새 스테이지가 열렸고, 그 노드가 존재한다면 연출 코루틴 시작!
                if (argIsNewStageUnlocked && newTarget != null)
                {
                    StartCoroutine(CoPanToNewStage(playedTarget, newTarget));
                }
                else
                {
                    // 평범한 상황 (반복 클리어, 패배, 나가기 등) -> 즉시 포커스
                    FocusOnNode(playedTarget, true);
                }
            }
        });
    }

    void SetMapBackground(StageData argWorldData)
    {
        _bgImage.sprite = argWorldData.bg;
    }
    
    void GenerateNodesAndFindTarget(StageData argWorldData, UserRecord argUserRecord, int argPlayedStageIndex, out RectTransform argPlayedTarget, out RectTransform argNewTarget)
    {
        // 1. 기존에 있던 노드들 청소
        foreach (var stageNode in _nodeList)
        {
            Managers.Pool.Destroy(stageNode, PrefabID.UIStageNode);
        }
        _nodeList.Clear();

        argPlayedTarget = null;
        argNewTarget = null;
        RectTransform highTarget = null;
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
            StageSaveInfo saveInfo = argUserRecord.GetStageSaveInfo(stageInfo.stageIndex);
            newNode.Init(stageInfo, saveInfo, OnStageNodeClicked);

            // 직전 플레이 노드
            if (stageInfo.stageIndex == argPlayedStageIndex)
            {
                argPlayedTarget = rect;
            }

            // 새로 해금된 경우 다음 노드
            if (stageInfo.stageIndex == argPlayedStageIndex + 1)
            {
                argNewTarget = rect;
            }
            
            // 최고 진척도 노드
            if (stageInfo.stageIndex > highestIndex)
            {
                highestIndex = stageInfo.stageIndex;
                highTarget = rect;
            }
        }

        if (argPlayedTarget == null) argPlayedTarget = highTarget;
    }

    Tween FocusOnNode(RectTransform targetNode, bool argIsInstant)
    {
        // 화면 중앙에 노드가 오도록 Content 위치 이동 (여백 클램핑 포함)
        Vector2 targetPos = -targetNode.anchoredPosition;
        RectTransform contentRect = _mapScrollRect.content;
        RectTransform viewportRect = _mapScrollRect.viewport;

        float xBound = Mathf.Max(0, (contentRect.rect.width - viewportRect.rect.width) / 2f);
        float yBound = Mathf.Max(0, (contentRect.rect.height - viewportRect.rect.height) / 2f);

        targetPos.x = Mathf.Clamp(targetPos.x, -xBound, xBound);
        targetPos.y = Mathf.Clamp(targetPos.y, -yBound, yBound);

        if (argIsInstant)
        {
            contentRect.anchoredPosition = targetPos;
            return null;
        }

        // 1초동안 부드럽게 맵 스크롤
        return contentRect.DOAnchorPos(targetPos, 1f).SetEase(Ease.InOutCubic);
    }
    
    IEnumerator CoPanToNewStage(RectTransform argPlayedTarget, RectTransform argNewTarget)
    {
        Managers.UI.ActiveInputBlocker(true);

        FocusOnNode(argPlayedTarget, true);

        yield return new WaitForSeconds(0.5f);

        yield return FocusOnNode(argNewTarget, false).WaitForCompletion();
        
        Managers.UI.ActiveInputBlocker(false);
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
        Managers.Sound.PlaySelectSfx();
        
        var popup = Managers.UI.PopupHandler.OpenPopup<UIStageInfo>(PrefabID.UIStageInfo);
        if (popup == null)
        {
            return;
        }
        
        popup.Init();
        popup.SetData(argStageInfo, _userRecord);
        popup.SetOnClose(OnClose);

        void OnClose()
        {
            Managers.Sound.PlaySelectSfx();
            Managers.UI.PopupHandler.ClosePopup();
        }
    }

    public void ToggleLobby(bool argIsVisible)
    {
        gameObject.SetActive(argIsVisible);
    }
}