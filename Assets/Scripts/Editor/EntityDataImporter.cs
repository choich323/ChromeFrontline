using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

[Serializable]
public class SheetLinkData
{
    public bool isSelected = true; // ★ 체크박스 상태 저장용 변수 추가
    public string name;
    public string url;
    public string targetSoGuid; 
}

[Serializable]
public class SheetLinkWrapper
{
    public List<SheetLinkData> links = new List<SheetLinkData>();
}

public class EntityDataImporter : EditorWindow
{
    private SheetLinkWrapper linkWrapper = new SheetLinkWrapper();
    
    // UI 스크롤용 변수
    private Vector2 scrollPosition;

    private string newLinkName = "";
    private string newLinkUrl = "";
    private EntityData newTargetData = null;

    private const string PREFS_KEY = "EntityDataImporter_SavedProfiles";

    [MenuItem("Tools/Import Entity Data")]
    public static void ShowWindow()
    {
        GetWindow<EntityDataImporter>("Entity Data Importer");
    }

    private void OnEnable()
    {
        string jsonData = EditorPrefs.GetString(PREFS_KEY, "");
        if (!string.IsNullOrEmpty(jsonData))
        {
            linkWrapper = JsonUtility.FromJson<SheetLinkWrapper>(jsonData);
        }
    }

    private void SaveLinks()
    {
        string jsonData = JsonUtility.ToJson(linkWrapper);
        EditorPrefs.SetString(PREFS_KEY, jsonData);
    }

    private string GetGuidFromSO(EntityData so)
    {
        if (so == null) return "";
        string path = AssetDatabase.GetAssetPath(so);
        return AssetDatabase.AssetPathToGUID(path);
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheets to ScriptableObject", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // --- [저장된 프로필 목록 영역 (체크박스 리스트)] ---
        GUILayout.Label("🔗 저장된 프로필 목록", EditorStyles.boldLabel);
        
        if (linkWrapper.links.Count > 0)
        {
            // 리스트가 길어질 것을 대비해 스크롤 뷰 적용
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));
            
            for (int i = 0; i < linkWrapper.links.Count; i++)
            {
                var link = linkWrapper.links[i];
                
                EditorGUILayout.BeginVertical("helpbox");
                
                // 첫 번째 줄: 체크박스 + 이름 + 삭제 버튼
                EditorGUILayout.BeginHorizontal();
                
                EditorGUI.BeginChangeCheck();
                link.isSelected = EditorGUILayout.Toggle(link.isSelected, GUILayout.Width(20));
                
                // 이름 수정 가능하도록 TextField 적용
                string prevName = link.name;
                link.name = EditorGUILayout.TextField(link.name, EditorStyles.boldLabel);
                
                if (EditorGUI.EndChangeCheck())
                {
                    SaveLinks(); // 체크박스나 이름이 바뀌면 즉시 저장
                }
                
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
                if (GUILayout.Button("삭제", GUILayout.Width(50)))
                {
                    linkWrapper.links.RemoveAt(i);
                    SaveLinks();
                    GUI.backgroundColor = Color.white;
                    GUIUtility.ExitGUI(); // UI 레이아웃 에러 방지
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                // 두 번째 줄: 데이터 세팅 (URL & SO)
                EditorGUI.BeginChangeCheck();
                string editedUrl = EditorGUILayout.TextField("CSV URL", link.url);
                
                // GUID로 SO 로드
                EntityData currentSO = null;
                if (!string.IsNullOrEmpty(link.targetSoGuid))
                {
                    string path = AssetDatabase.GUIDToAssetPath(link.targetSoGuid);
                    currentSO = AssetDatabase.LoadAssetAtPath<EntityData>(path);
                }
                
                EntityData editedSO = (EntityData)EditorGUILayout.ObjectField("Target SO", currentSO, typeof(EntityData), false);
                
                if (EditorGUI.EndChangeCheck())
                {
                    link.url = editedUrl;
                    link.targetSoGuid = GetGuidFromSO(editedSO);
                    SaveLinks();
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            EditorGUILayout.EndScrollView();
            
            // 전체 선택 / 해제 버튼
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("전체 선택"))
            {
                linkWrapper.links.ForEach(l => l.isSelected = true);
                SaveLinks();
            }
            if (GUILayout.Button("전체 해제"))
            {
                linkWrapper.links.ForEach(l => l.isSelected = false);
                SaveLinks();
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("저장된 프로필이 없습니다. 아래에서 새 프로필을 추가해주세요.", MessageType.Info);
        }

        EditorGUILayout.Space(15);

        // --- [프로필 추가 영역] ---
        GUILayout.Label("➕ 새 프로필 추가 (이름, 링크, SO 묶음)", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        newLinkName = EditorGUILayout.TextField("시트 이름 (예: 본섭)", newLinkName);
        newLinkUrl = EditorGUILayout.TextField("CSV URL", newLinkUrl);
        newTargetData = (EntityData)EditorGUILayout.ObjectField("Target SO", newTargetData, typeof(EntityData), false);
        
        if (GUILayout.Button("목록에 추가"))
        {
            if (!string.IsNullOrEmpty(newLinkName) && !string.IsNullOrEmpty(newLinkUrl))
            {
                linkWrapper.links.Add(new SheetLinkData { 
                    name = newLinkName, 
                    url = newLinkUrl,
                    targetSoGuid = GetGuidFromSO(newTargetData),
                    isSelected = true // 새로 추가된 건 기본으로 체크되도록
                });

                newLinkName = ""; newLinkUrl = ""; newTargetData = null;
                SaveLinks();
                GUI.FocusControl(null); 
            }
            else
            {
                Debug.LogWarning("프로필 이름과 URL을 모두 입력해야 합니다.");
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20);

        // --- [임포트 실행 버튼] ---
        GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
        if (GUILayout.Button("체크된 항목 모두 임포트 (Import Selected)", GUILayout.Height(40)))
        {
            _ = ImportSelectedDataAsync();
        }
        GUI.backgroundColor = Color.white;
    }

    // ★ 체크된 항목만 모아서 순차적으로 실행하는 매니저 함수
    private async Task ImportSelectedDataAsync()
    {
        var selectedLinks = linkWrapper.links.Where(l => l.isSelected).ToList();
        
        if (selectedLinks.Count == 0)
        {
            Debug.LogWarning("선택된 프로필이 없습니다. 임포트할 시트의 체크박스를 확인해주세요.");
            return;
        }

        int successCount = 0;
        
        foreach (var link in selectedLinks)
        {
            EntityData targetSO = null;
            if (!string.IsNullOrEmpty(link.targetSoGuid))
            {
                string path = AssetDatabase.GUIDToAssetPath(link.targetSoGuid);
                targetSO = AssetDatabase.LoadAssetAtPath<EntityData>(path);
            }

            if (targetSO == null)
            {
                Debug.LogError($"[{link.name}] 연결된 Target SO가 없거나 삭제되었습니다. 스킵합니다.");
                continue;
            }

            bool success = await ImportDataAsync(link.url, targetSO, link.name);
            if (success) successCount++;
        }
        
        Debug.Log($"🚀 전체 임포트 작업 완료! (성공: {successCount} / 시도: {selectedLinks.Count})");
    }

    // 단일 시트 임포트 함수 (성공 여부를 반환하도록 수정)
    private async Task<bool> ImportDataAsync(string url, EntityData targetSO, string profileName)
    {
        Debug.Log($"[{profileName}] 데이터 다운로드 시작...");
        
        string separator = url.Contains("?") ? "&" : "?";
        string noCacheUrl = url + separator + "t=" + DateTime.Now.Ticks;

        using (UnityWebRequest www = UnityWebRequest.Get(noCacheUrl))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[{profileName}] 데이터 다운로드 실패: {www.error}");
                return false;
            }

            ParseCSVAndApply(www.downloadHandler.text, targetSO, profileName);
            return true;
        }
    }

    private string[] SplitCsvLine(string line)
    {
        return Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
    }

    private void ParseCSVAndApply(string csvText, EntityData targetSO, string profileName)
    {
        string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return;

        string[] headers = SplitCsvLine(lines[0]);
        Dictionary<string, int> headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            string headerName = headers[i].Replace("\uFEFF", "").Replace("\u200B", "").Replace("\"", "").Trim();
            headerIndex[headerName] = i;
        }

        Dictionary<string, EntityInfo> existingEntities = new Dictionary<string, EntityInfo>();
        foreach (var info in targetSO.entityInfoList)
        {
            if (!string.IsNullOrEmpty(info.id) && !existingEntities.ContainsKey(info.id))
            {
                existingEntities.Add(info.id, info);
            }
        }

        targetSO.entityInfoList.Clear();
        int importedCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = SplitCsvLine(lines[i]);
            string id = GetValueOrNull(row, headerIndex, "id");
            
            if (string.IsNullOrEmpty(id)) 
            {
                continue;
            }

            EntityInfo entity = existingEntities.ContainsKey(id) ? existingEntities[id] : new EntityInfo();
            entity.id = id;

            AssignBool(row, headerIndex, "isOriginalSpriteFacingLeft", ref entity.isOriginalSpriteFacingLeft);
            AssignFloat(row, headerIndex, "dieAnimDuration", ref entity.dieAnimDuration);
            AssignFloat(row, headerIndex, "attackAnimDuration", ref entity.attackAnimDuration);
            AssignFloat(row, headerIndex, "attackHitTiming", ref entity.attackHitTiming, true); 
            
            AssignEnum(row, headerIndex, "attackAreaType", ref entity.attackAreaType);
            AssignEnum(row, headerIndex, "camp", ref entity.camp);
            
            AssignInt(row, headerIndex, "tier", ref entity.tier);
            AssignInt(row, headerIndex, "level", ref entity.level);
            AssignInt(row, headerIndex, "hp", ref entity.hp);
            AssignFloat(row, headerIndex, "armor", ref entity.armor);

            AssignFloat(row, headerIndex, "attack", ref entity.attack);
            AssignFloat(row, headerIndex, "attackSpeed", ref entity.attackSpeed);
            AssignFloat(row, headerIndex, "attackRange", ref entity.attackRange);
            AssignFloat(row, headerIndex, "criticalChance", ref entity.criticalChance, true); 
            
            AssignFloat(row, headerIndex, "moveSpeed", ref entity.moveSpeed);
            AssignFloat(row, headerIndex, "productionTime", ref entity.productionTime);
            AssignInt(row, headerIndex, "goldCost", ref entity.goldCost);
            
            targetSO.entityInfoList.Add(entity);
            
            if (!existingEntities.ContainsKey(id))
            {
                existingEntities.Add(id, entity);
            }
            importedCount++;
        }

        EditorUtility.SetDirty(targetSO);
        AssetDatabase.SaveAssets();
        Debug.Log($"🎉 [{profileName}] 총 {importedCount}개의 데이터 임포트가 완료되었습니다.");
    }

    #region 파싱 헬퍼 함수들
    private string GetValueOrNull(string[] row, Dictionary<string, int> headerIndex, string columnName)
    {
        if (headerIndex.TryGetValue(columnName, out int index) && index < row.Length)
        {
            string val = row[index].Replace("\"", "").Trim();
            return string.IsNullOrEmpty(val) ? null : val;
        }
        return null;
    }

    private void AssignString(string[] row, Dictionary<string, int> headerIndex, string col, ref string target)
    {
        string val = GetValueOrNull(row, headerIndex, col);
        if (val != null) target = val;
    }

    private void AssignInt(string[] row, Dictionary<string, int> headerIndex, string col, ref int target)
    {
        string val = GetValueOrNull(row, headerIndex, col);
        if (!string.IsNullOrEmpty(val))
        {
            string cleanedVal = Regex.Replace(val, @"[^0-9\.\-]", "");
            if (float.TryParse(cleanedVal, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result)) 
            {
                target = Mathf.RoundToInt(result);
            }
        }
    }

    private void AssignFloat(string[] row, Dictionary<string, int> headerIndex, string col, ref float target, bool isProbability = false)
    {
        string val = GetValueOrNull(row, headerIndex, col);
        if (!string.IsNullOrEmpty(val))
        {
            bool hasPercentSign = val.Contains("%");
            string cleanedVal = Regex.Replace(val, @"[^0-9\.\-]", "");
            
            if (float.TryParse(cleanedVal, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result)) 
            {
                if (hasPercentSign || (isProbability && result > 1f))
                {
                    result /= 100f;
                }
                target = result;
            }
            else
            {
                Debug.LogWarning($"[{col}] 실수 파싱 실패 - 읽어온 원본 값: '{val}'");
            }
        }
    }

    private void AssignBool(string[] row, Dictionary<string, int> headerIndex, string col, ref bool target)
    {
        string val = GetValueOrNull(row, headerIndex, col);
        if (!string.IsNullOrEmpty(val) && bool.TryParse(val, out bool result)) 
        {
            target = result;
        }
    }

    private void AssignEnum<T>(string[] row, Dictionary<string, int> headerIndex, string col, ref T target) where T : struct
    {
        string val = GetValueOrNull(row, headerIndex, col);
        if (!string.IsNullOrEmpty(val) && Enum.TryParse(val, true, out T result)) 
        {
            target = result;
        }
    }
    #endregion
}