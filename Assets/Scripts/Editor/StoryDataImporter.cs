using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class StoryDataImporter : EditorWindow
{
    private string sheetId = "YOUR_SPREADSHEET_ID_HERE";
    private string gid = "YOUR_GID_HERE";
    private string savePath = "Assets/StoryData.asset";
    private string worldId;

    [MenuItem("Tools/Import Story Data (Reflection)")]
    public static void ShowWindow()
    {
        GetWindow<StoryDataImporter>("Story Importer");
    }

    private void OnGUI()
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

    private async void ImportData()
    {
        string url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gid}";

        try
        {
            Debug.Log("데이터 다운로드 중...");
            string csvContent = await DownloadCSVAsync(url);
            ParseCSVAndCreateSO(csvContent);
        }
        catch (Exception e)
        {
            Debug.LogError($"다운로드 실패: {e.Message}");
        }
    }

    private async Task<string> DownloadCSVAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            // await를 사용해 에디터를 멈추지 않고 백그라운드에서 비동기 다운로드
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // 에러 발생 시 예외 던짐
            return await response.Content.ReadAsStringAsync();
        }
    }

    private void ParseCSVAndCreateSO(string csvContent)
    {
        // 1. CSV 라인 분리 (캐리지 리턴 제거)
        string[] lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;

        // 2. 헤더 파싱
        string[] headers = SplitCSVLine(lines[0]);

        int worldIdColIndex = -1;
        for (int col = 0; col < headers.Length; col++)
        {
            if (headers[col].Trim() == "worldId")
            {
                worldIdColIndex = col;
                break;
            }
        }

        if (worldIdColIndex == -1)
        {
            Debug.LogError("[Import Error] 시트 헤더에서 'worldId' 컬럼을 찾을 수 없습니다.");
            return;
        }
        
        // 3. ScriptableObject 로드 또는 생성
        StoryData storyData = AssetDatabase.LoadAssetAtPath<StoryData>(savePath);
        
        if (storyData == null)
        {
            // 에셋이 없을 때는 새로 만들고 최초로 ID를 부여함
            storyData = CreateInstance<StoryData>();
            storyData.worldId = worldId; 
            AssetDatabase.CreateAsset(storyData, savePath);
        }
        else
        {
            // 기존 에셋이 존재한다면, 에디터에 입력한 ID와 에셋의 ID가 같은지 비교!
            if (!string.IsNullOrEmpty(storyData.worldId) && storyData.worldId != worldId)
            {
                Debug.LogError($"[Import Error] 타겟 에셋의 World ID({storyData.worldId})와 입력한 World ID({worldId})가 다릅니다! 잘못된 경로에 덮어쓰기를 시도하고 있습니다.");
                return; // 덮어쓰기 즉시 중단
            }
        }

        // 데이터 리스트 초기화 (이건 무조건 덮어써야 하니 밖으로 빼는 게 맞음)
        storyData.storyInfoList = new List<StoryInfo>();

        // 4. 데이터 행(Row) 파싱 및 리플렉션 적용
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = SplitCSVLine(lines[i]);
            
            if (worldIdColIndex >= values.Length)
                continue;

            string rowWorldId = values[worldIdColIndex].Trim();
            if (rowWorldId != worldId)
                continue;
            
            StoryInfo newStoryInfo = new StoryInfo();

            for (int col = 0; col < headers.Length; col++)
            {
                if (col >= values.Length) continue;

                if (col == worldIdColIndex) continue;
                
                string header = headers[col].Trim();
                string value = values[col].Trim();
                
                if (string.IsNullOrEmpty(header) || string.IsNullOrEmpty(value)) continue;
                
                // 개행 문자 적용
                value = value.Replace("\\n", "\n");
                
                ApplyValueViaReflection(newStoryInfo, header, value);
            }

            storyData.storyInfoList.Add(newStoryInfo);
        }

        EditorUtility.SetDirty(storyData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[리플렉션 임포트 완료] {storyData.storyInfoList.Count}개의 스토리 데이터가 갱신되었습니다.");
    }

    private void ApplyValueViaReflection(object targetObj, string headerPath, string value)
    {
        Type targetType = targetObj.GetType();

        // 중첩 클래스 처리 (예: title.kr)
        if (headerPath.Contains("."))
        {
            string[] pathParts = headerPath.Split('.');
            string parentFieldName = pathParts[0];
            string childFieldName = pathParts[1];

            FieldInfo parentField = targetType.GetField(parentFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (parentField == null)
            {
                Debug.LogWarning($"[Parse Warning] {targetType.Name}에 '{parentFieldName}' 변수가 없습니다.");
                return;
            }

            // 부모 객체(LocalizationText)가 null이면 인스턴스 생성
            object parentInstance = parentField.GetValue(targetObj);
            if (parentInstance == null)
            {
                parentInstance = Activator.CreateInstance(parentField.FieldType);
                parentField.SetValue(targetObj, parentInstance);
            }

            // 자식 필드(en, kr 등)에 값 할당
            FieldInfo childField = parentField.FieldType.GetField(childFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (childField != null)
            {
                childField.SetValue(parentInstance, Convert.ChangeType(value, childField.FieldType));
            }
        }
        // 단일 변수 처리 (예: stage)
        else
        {
            FieldInfo field = targetType.GetField(headerPath, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                // int 등 기본 타입으로 안전하게 변환
                field.SetValue(targetObj, Convert.ChangeType(value, field.FieldType));
            }
            else
            {
                Debug.LogWarning($"[Parse Warning] {targetType.Name}에 '{headerPath}' 변수가 없습니다.");
            }
        }
    }

    // 대사 안의 쉼표(,)를 무시하고 컬럼을 정확히 나누는 CSV 정규식 스플리터
    private string[] SplitCSVLine(string line)
    {
        Regex csvRegex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        string[] result = csvRegex.Split(line);
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = result[i].Replace("\"\"", "\"").Trim('\"');
        }
        return result;
    }
}