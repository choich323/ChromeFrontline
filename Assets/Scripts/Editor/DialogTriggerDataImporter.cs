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

public class DialogTriggerDataImporter : EditorWindow
{
    private string sheetId = "YOUR_SPREADSHEET_ID_HERE";
    private string gid = "0";
    private string savePath = "Assets/Data/Dialog";
    private string stage;

    [MenuItem("Tools/Import Dialog Trigger Data")]
    public static void ShowWindow()
    {
        GetWindow<DialogTriggerDataImporter>(
            "Dialog Trigger Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label(
            "Google Sheet Settings",
            EditorStyles.boldLabel);

        sheetId = EditorGUILayout.TextField(
            "Spreadsheet ID",
            sheetId);

        gid = EditorGUILayout.TextField(
            "Sheet GID",
            gid);

        stage = EditorGUILayout.TextField(
            "Stage",
            stage);

        savePath = EditorGUILayout.TextField(
            "Save Folder",
            savePath);

        if (GUILayout.Button("Import & Parse"))
            ImportData();
    }

    private async void ImportData()
    {
        string url =
            $"https://docs.google.com/spreadsheets/d/{sheetId}" +
            $"/export?format=csv&gid={gid}";

        try
        {
            Debug.Log(
                "다이얼로그 트리거 데이터 다운로드 중...");

            string csvContent =
                await DownloadCSVAsync(url);

            ParseCSVAndCreateSO(csvContent);
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"다운로드 실패: {e.Message}");
        }
    }

    private async Task<string> DownloadCSVAsync(
        string url)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response =
                await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
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
                $"[Addressable] '{argAssetPath}'의 " +
                "GUID를 찾을 수 없습니다.");
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
                ? System.IO.Path
                    .GetFileNameWithoutExtension(
                        argAssetPath)
                : argCustomAddress;

        settings.SetDirty(
            AddressableAssetSettings.ModificationEvent.EntryMoved,
            entry,
            true);

        Debug.Log(
            $"[Addressable] '{entry.address}'가 " +
            $"'{group.Name}' 그룹에 등록되었습니다.");
    }

    private void ParseCSVAndCreateSO(
        string csvContent)
    {
        string[] lines =
            csvContent.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
            return;

        string[] headers =
            SplitCSVLine(lines[0]);

        int stageIndex =
            Array.IndexOf(headers, "stage");

        if (stageIndex == -1)
        {
            Debug.LogError(
                "[Import Error] 'stage' 컬럼을 찾을 수 없습니다.");
            return;
        }

        string assetPath =
            $"{savePath}/DialogTriggerData_{stage}.asset";

        DialogTriggerData triggerData =
            AssetDatabase.LoadAssetAtPath<DialogTriggerData>(
                assetPath);

        if (triggerData == null)
        {
            triggerData =
                CreateInstance<DialogTriggerData>();

            AssetDatabase.CreateAsset(
                triggerData,
                assetPath);
        }

        triggerData.stage = stage;

        triggerData.triggerInfoList = new List<DialogTriggerInfo>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values =
                SplitCSVLine(lines[i]);

            if (stageIndex >= values.Length)
                continue;

            if (values[stageIndex].Trim() != stage)
                continue;

            DialogTriggerInfo trigger =
                new DialogTriggerInfo();

            for (int col = 0;
                 col < headers.Length;
                 col++)
            {
                if (col >= values.Length ||
                    col == stageIndex)
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
                    trigger,
                    header,
                    value);
            }

            triggerData.triggerInfoList.Add(
                trigger);
        }

        EditorUtility.SetDirty(triggerData);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SetAssetAsAddressable(
            assetPath,
            $"DialogTriggerData_{stage}");

        Debug.Log(
            $"[Dialog Trigger Import] {stage} : " +
            $"{triggerData.triggerInfoList.Count}" +
            "개의 Trigger가 갱신되었습니다.");
    }

    private void ApplyValueViaReflection(
        object argTargetObj,
        string argHeaderPath,
        string argValue)
    {
        Type targetType =
            argTargetObj.GetType();

        FieldInfo field =
            targetType.GetField(
                argHeaderPath,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.NonPublic);

        if (field == null)
        {
            Debug.LogWarning(
                $"[Parse Warning] " +
                $"{targetType.Name}에 " +
                $"'{argHeaderPath}' 변수가 없습니다.");
            return;
        }

        try
        {
            if (field.FieldType.IsEnum)
            {
                field.SetValue(
                    argTargetObj,
                    Enum.Parse(
                        field.FieldType,
                        argValue));
            }
            else
            {
                field.SetValue(
                    argTargetObj,
                    Convert.ChangeType(
                        argValue,
                        field.FieldType));
            }
        }
        catch
        {
            Debug.LogWarning(
                $"[Parse Warning] " +
                $"{argHeaderPath} 변환 실패: {argValue}");
        }
    }

    private string[] SplitCSVLine(string argLine)
    {
        Regex csvRegex =
            new Regex(
                ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        string[] result =
            csvRegex.Split(argLine);

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