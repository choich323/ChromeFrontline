#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class WorldStageDataImporter : EditorWindow
{
    //[cite: 3] 구글 시트 다운로드를 위한 변수들 세팅
    private string sheetId = "YOUR_SPREADSHEET_ID_HERE";
    private string gid = "0";
    private string savePath = "Assets/Data/World/world.asset";
    private string worldId = "world";

    [MenuItem("Tools/Import World-Stage Data")]
    public static void ShowWindow()
    {
        GetWindow<WorldStageDataImporter>("World-Stage Importer");
    }

    private void OnGUI() //[cite: 3]
    {
        GUILayout.Label("Google Sheet Settings", EditorStyles.boldLabel);
        sheetId = EditorGUILayout.TextField("Spreadsheet ID", sheetId);
        gid = EditorGUILayout.TextField("Sheet GID", gid);
        worldId = EditorGUILayout.TextField("World ID", worldId);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if (GUILayout.Button("Import & Parse (Dynamic)"))
        {
            ImportData();
        }
    }

    private async void ImportData() //[cite: 3]
    {
        string url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gid}";

        try
        {
            Debug.Log("스테이지 데이터 다운로드 중...");
            string csvContent = await DownloadCSVAsync(url);
            ParseCSVAndCreateSO(csvContent);
        }
        catch (Exception e)
        {
            Debug.LogError($"다운로드 실패: {e.Message}");
        }
    }

    private async Task<string> DownloadCSVAsync(string url) //[cite: 3]
    {
        using (HttpClient client = new HttpClient())
        {
            // 백그라운드 비동기 다운로드 처리
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); 
            return await response.Content.ReadAsStringAsync();
        }
    }

    private void SetAssetAsAddressable(string argAssetPath, string argCustomAdress = "", string argGroupName = "Default Local Group")
    {
        // 현재 프로젝트의 어드레서블 세팅 데이터 가져오기
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogWarning("[Addressable] Addressable 세팅을 찾을 수 없습니다. 창을 한 번 열어 초기화해주세요.");
            return;
        }

        // 에셋 경로를 통해 GUID 추출
        string guid = AssetDatabase.AssetPathToGUID(argAssetPath);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogWarning($"[Addressable] '{argAssetPath}'의 GUID를 찾을 수 없습니다.");
            return;
        }

        // 지정한 이름의 그룹을 찾고, 없으면 기본 그룹 사용
        AddressableAssetGroup group = settings.FindGroup(argGroupName);
        if (group == null)
        {
            group = settings.DefaultGroup;
        }

        // 에셋을 해당 그룹에 등록 (또는 이동)
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
        if (entry != null)
        {
            if (!string.IsNullOrEmpty(argCustomAdress))
            {
                entry.address = argCustomAdress; // 에디터에서 입력한 이름 적용
            }
            else
            {
                // 입력값이 없으면 기존처럼 파일 이름 사용
                entry.address = System.IO.Path.GetFileNameWithoutExtension(argAssetPath);
            }
        
            // 변경 사항 저장
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            Debug.Log($"[Addressable] '{entry.address}'가 '{group.Name}' 그룹에 성공적으로 등록되었습니다.");
        }
    }
    
    private void ParseCSVAndCreateSO(string argCsvContent)
    {
        // 1. CSV 라인 분리[cite: 3]
        string[] lines = argCsvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;

        // 2. 헤더 파싱[cite: 3]
        string[] headers = SplitCSVLine(lines[0]);

        // worldId 컬럼 찾기 (시트에 존재할 경우 필터링용)
        int worldIdColIndex = Array.IndexOf(headers, "worldId");
        
        // 3. ScriptableObject 로드 또는 생성[cite: 3]
        WorldData worldData = AssetDatabase.LoadAssetAtPath<WorldData>(savePath);
        
        if (worldData == null)
        {
            worldData = CreateInstance<WorldData>();
            worldData.worldId = worldId; //[cite: 1, 3]
            AssetDatabase.CreateAsset(worldData, savePath);
        }
        else
        {
            // 잘못된 경로 덮어쓰기 방지 검증[cite: 3]
            if (!string.IsNullOrEmpty(worldData.worldId) && worldData.worldId != worldId)
            {
                Debug.LogError($"[Import Error] 타겟 에셋의 World ID({worldData.worldId})와 입력한 World ID({worldId})가 다릅니다!");
                return; 
            }
        }

        // === 핵심: uiPosition 백업 로직 ===
        Dictionary<int, Vector2> existingPositions = new Dictionary<int, Vector2>();
        
        // 기존 리스트에서 stage 번호를 키로 uiPosition을 딕셔너리에 저장[cite: 1]
        if (worldData.stageInfoList != null) 
        {
            foreach (var info in worldData.stageInfoList)
            {
                existingPositions[info.stage] = info.uiPosition; 
            }
        }

        // 데이터 리스트 초기화 
        worldData.stageInfoList = new List<StageInfo>(); //[cite: 1]

        // 4. 데이터 행(Row) 파싱 및 리플렉션 적용[cite: 3]
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = SplitCSVLine(lines[i]); 
            
            // 시트에 worldId가 있다면 필터링[cite: 3]
            if (worldIdColIndex != -1 && worldIdColIndex < values.Length)
            {
                string rowWorldId = values[worldIdColIndex].Trim();
                if (rowWorldId != worldId) continue;
            }
            
            StageInfo newStageInfo = new StageInfo(); //[cite: 1]

            for (int col = 0; col < headers.Length; col++)
            {
                if (col >= values.Length || col == worldIdColIndex) continue;
                
                string header = headers[col].Trim();
                string value = values[col].Trim();
                
                // 빈 값 스킵 및 개행 문자 처리[cite: 3]
                if (string.IsNullOrEmpty(header) || string.IsNullOrEmpty(value)) continue; 
                value = value.Replace("\\n", "\n"); 
                
                // 시트 어딘가에 uiPosition 열이 있더라도 파싱에서 제외시킴
                if (header == "uiPosition") continue;

                ApplyValueViaReflection(newStageInfo, header, value); //[cite: 3]
            }

            // 백업해둔 uiPosition 복원 
            if (existingPositions.TryGetValue(newStageInfo.stage, out Vector2 pos)) //[cite: 1]
            {
                newStageInfo.uiPosition = pos; //[cite: 1]
            }

            worldData.stageInfoList.Add(newStageInfo); //[cite: 1]
        }

        // 에셋 저장 및 갱신[cite: 3]
        EditorUtility.SetDirty(worldData); 
        AssetDatabase.SaveAssets(); 
        AssetDatabase.Refresh();

        SetAssetAsAddressable(savePath, worldId);
        
        Debug.Log($"[리플렉션 임포트 완료] {worldData.stageInfoList.Count}개의 스테이지 데이터가 갱신되었습니다.");
    }

    private void ApplyValueViaReflection(object argTargetObj, string argHeaderPath, string argValue) //[cite: 3]
    {
        Type targetType = argTargetObj.GetType();

        // 중첩 클래스 처리 (예: title.kr)[cite: 3]
        if (argHeaderPath.Contains("."))
        {
            string[] pathParts = argHeaderPath.Split('.');
            string parentFieldName = pathParts[0];
            string childFieldName = pathParts[1];

            FieldInfo parentField = targetType.GetField(parentFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (parentField == null) return;

            object parentInstance = parentField.GetValue(argTargetObj);
            if (parentInstance == null)
            {
                parentInstance = Activator.CreateInstance(parentField.FieldType);
                parentField.SetValue(argTargetObj, parentInstance);
            }

            FieldInfo childField = parentField.FieldType.GetField(childFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (childField != null)
            {
                try 
                {
                    childField.SetValue(parentInstance, Convert.ChangeType(argValue, childField.FieldType));
                } 
                catch 
                { 
                    Debug.LogWarning($"[Parse Warning] {childFieldName} 변환 실패: {argValue}"); 
                }
            }
        }
        // 단일 변수 처리 (비트마스크 ulong 포함)[cite: 3]
        else
        {
            FieldInfo field = targetType.GetField(argHeaderPath, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                try 
                {
                    // Convert.ChangeType은 ulong 타입도 정상적으로 변환 처리함
                    field.SetValue(argTargetObj, Convert.ChangeType(argValue, field.FieldType));
                } 
                catch 
                { 
                    Debug.LogWarning($"[Parse Warning] {argHeaderPath} 변환 실패: {argValue}"); 
                }
            }
            else
            {
                Debug.LogWarning($"[Parse Warning] {targetType.Name}에 '{argHeaderPath}' 변수가 없습니다.");
            }
        }
    }

    // 대사 안의 쉼표(,)를 무시하고 컬럼을 정확히 나누는 CSV 정규식 스플리터[cite: 3]
    private string[] SplitCSVLine(string argLine)
    {
        Regex csvRegex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        string[] result = csvRegex.Split(argLine);
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = result[i].Replace("\"\"", "\"").Trim('\"');
        }
        return result;
    }
}
#endif