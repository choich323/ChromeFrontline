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

public class StoryDataImporter : EditorWindow
{
    private string sheetId = "YOUR_SPREADSHEET_ID_HERE";
    private string gid = "0";
    private string savePath = "Assets/Data/Story";
    private string worldId;

    [MenuItem("Tools/Import Story Data")]
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
            ImportData();
    }

    private async void ImportData()
    {
        string url =
            $"https://docs.google.com/spreadsheets/d/{sheetId}" +
            $"/export?format=csv&gid={gid}";

        try
        {
            Debug.Log("데이터 다운로드 중...");

            string csvContent =
                await DownloadCSVAsync(url);

            ParseCSVAndCreateSO(csvContent);
        }
        catch (Exception e)
        {
            Debug.LogError($"다운로드 실패: {e.Message}");
        }
    }

    private async Task<string> DownloadCSVAsync(string url)
    {
        using HttpClient client = new HttpClient();

        HttpResponseMessage response =
            await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private void SetAssetAsAddressable(
        string argAssetPath,
        string argCustomAddress = "",
        string argGroupName = "Default Local Group")
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            Debug.LogWarning(
                "[Addressable] Addressable 세팅을 찾을 수 없습니다.");
            return;
        }

        string guid =
            AssetDatabase.AssetPathToGUID(argAssetPath);

        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogWarning(
                $"[Addressable] '{argAssetPath}'의 GUID를 찾을 수 없습니다.");
            return;
        }

        AddressableAssetGroup group =
            settings.FindGroup(argGroupName);

        if (group == null)
            group = settings.DefaultGroup;

        AddressableAssetEntry entry =
            settings.CreateOrMoveEntry(
                guid,
                group,
                readOnly: false,
                postEvent: false);

        if (entry == null)
            return;

        entry.address =
            string.IsNullOrEmpty(argCustomAddress)
                ? System.IO.Path.GetFileNameWithoutExtension(argAssetPath)
                : argCustomAddress;

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.EntryMoved,
            entry,
            true);

        Debug.Log(
            $"[Addressable] '{entry.address}'가 " +
            $"'{group.Name}' 그룹에 등록되었습니다.");
    }

    private void ParseCSVAndCreateSO(string csvContent)
    {
        string[] lines =
            csvContent.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
            return;

        string[] headers =
            SplitCSVLine(lines[0]);

        int worldIdColIndex =
            Array.IndexOf(headers, "worldId");

        if (worldIdColIndex == -1)
        {
            Debug.LogError(
                "[Import Error] 'worldId' 컬럼을 찾을 수 없습니다.");
            return;
        }

        string assetPath =
            $"{savePath}/StoryData_{worldId}.asset";
        
        StoryData storyData =
            AssetDatabase.LoadAssetAtPath<StoryData>(assetPath);

        if (storyData == null)
        {
            storyData =
                CreateInstance<StoryData>();

            storyData.worldId = worldId;

            AssetDatabase.CreateAsset(
                storyData,
                assetPath);
        }
        else if (!string.IsNullOrEmpty(storyData.worldId) &&
                 storyData.worldId != worldId)
        {
            Debug.LogError(
                $"[Import Error] 타겟 에셋의 World ID" +
                $"({storyData.worldId})와 입력한 World ID" +
                $"({worldId})가 다릅니다.");

            return;
        }

        storyData.storyInfoList =
            new List<StoryInfo>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values =
                SplitCSVLine(lines[i]);

            if (worldIdColIndex >= values.Length)
                continue;

            string rowWorldId =
                values[worldIdColIndex].Trim();

            if (rowWorldId != worldId)
                continue;

            StoryInfo newStoryInfo =
                new StoryInfo();

            for (int col = 0;
                 col < headers.Length;
                 col++)
            {
                if (col >= values.Length ||
                    col == worldIdColIndex)
                    continue;

                string header =
                    headers[col].Trim();

                string value =
                    values[col].Trim();

                if (string.IsNullOrEmpty(header) ||
                    string.IsNullOrEmpty(value))
                    continue;

                value =
                    value.Replace("\\n", "\n");

                ApplyValueViaReflection(
                    newStoryInfo,
                    header,
                    value);
            }

            storyData.storyInfoList.Add(
                newStoryInfo);
        }

        EditorUtility.SetDirty(storyData);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Addressable 등록
        SetAssetAsAddressable(
            assetPath,
            $"StoryData_{worldId}");

        Debug.Log(
            $"[리플렉션 임포트 완료] " +
            $"{storyData.storyInfoList.Count}" +
            $"개의 스토리 데이터가 갱신되었습니다.");
    }

    private void ApplyValueViaReflection(
        object targetObj,
        string headerPath,
        string value)
    {
        Type targetType =
            targetObj.GetType();

        if (headerPath.Contains("."))
        {
            string[] pathParts =
                headerPath.Split('.');

            FieldInfo parentField =
                targetType.GetField(
                    pathParts[0],
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            if (parentField == null)
                return;

            object parentInstance =
                parentField.GetValue(targetObj);

            if (parentInstance == null)
            {
                parentInstance =
                    Activator.CreateInstance(
                        parentField.FieldType);

                parentField.SetValue(
                    targetObj,
                    parentInstance);
            }

            FieldInfo childField =
                parentField.FieldType.GetField(
                    pathParts[1],
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            if (childField != null)
            {
                childField.SetValue(
                    parentInstance,
                    Convert.ChangeType(
                        value,
                        childField.FieldType));
            }
        }
        else
        {
            FieldInfo field =
                targetType.GetField(
                    headerPath,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(
                    targetObj,
                    Convert.ChangeType(
                        value,
                        field.FieldType));
            }
        }
    }

    private string[] SplitCSVLine(string line)
    {
        Regex csvRegex =
            new Regex(
                ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        string[] result =
            csvRegex.Split(line);

        for (int i = 0; i < result.Length; i++)
        {
            result[i] =
                result[i]
                .Replace("\"\"", "\"")
                .Trim('"');
        }

        return result;
    }
}

#endif