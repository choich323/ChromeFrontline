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

public class DialogDataImporter : EditorWindow
{
    private string sheetId = "YOUR_SPREADSHEET_ID_HERE";
    private string gid = "0";
    private string savePath = "Assets/Data/Dialog";
    private string stage;

    [MenuItem("Tools/Import Dialog Data")]
    public static void ShowWindow()
    {
        GetWindow<DialogDataImporter>("Dialog Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheet Settings", EditorStyles.boldLabel);

        sheetId = EditorGUILayout.TextField("Spreadsheet ID", sheetId);
        gid = EditorGUILayout.TextField("Sheet GID", gid);
        stage = EditorGUILayout.TextField("Stage", stage);
        savePath = EditorGUILayout.TextField("Save Folder", savePath);

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
            Debug.Log("다이얼로그 데이터 다운로드 중...");

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

        int stageIndex =
            Array.IndexOf(headers, "stage");

        if (stageIndex == -1)
        {
            Debug.LogError(
                "[Import Error] 'stage' 컬럼을 찾을 수 없습니다.");
            return;
        }

        string assetPath =
            $"{savePath}/DialogData_{stage}.asset";

        DialogData dialogData =
            AssetDatabase.LoadAssetAtPath<DialogData>(
                assetPath);

        if (dialogData == null)
        {
            dialogData =
                CreateInstance<DialogData>();

            AssetDatabase.CreateAsset(
                dialogData,
                assetPath);
        }

        dialogData.stage = stage;
        dialogData.dialogInfoList =
            new List<DialogInfo>();

        Dictionary<string, DialogInfo> infoMap =
            new Dictionary<string, DialogInfo>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values =
                SplitCSVLine(lines[i]);

            if (stageIndex >= values.Length)
                continue;

            if (values[stageIndex].Trim() != stage)
                continue;

            string infoId =
                GetValue(
                    headers,
                    values,
                    "infoId");

            if (!infoMap.TryGetValue(
                    infoId,
                    out DialogInfo info))
            {
                info = new DialogInfo
                {
                    infoId = infoId
                };

                infoMap.Add(infoId, info);
            }

            Dialog dialog =
                new Dialog();

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
                    string.IsNullOrEmpty(value) ||
                    header == "infoId")
                    continue;

                value =
                    value.Replace("\\n", "\n");

                ApplyValueViaReflection(
                    dialog,
                    header,
                    value);
            }

            info.dialogList.Add(dialog);
        }

        dialogData.dialogInfoList.AddRange(
            infoMap.Values);

        EditorUtility.SetDirty(dialogData);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SetAssetAsAddressable(
            assetPath,
            $"DialogData_{stage}");

        Debug.Log(
            $"[Dialog Import] {stage} : " +
            $"{dialogData.dialogInfoList.Count}" +
            "개의 DialogInfo가 갱신되었습니다.");
    }

    private string GetValue(
        string[] headers,
        string[] values,
        string columnName)
    {
        int index =
            Array.IndexOf(
                headers,
                columnName);

        return index >= 0 &&
               index < values.Length
            ? values[index].Trim()
            : string.Empty;
    }

    private void ApplyValueViaReflection(
        object argTargetObj,
        string argHeaderPath,
        string argValue)
    {
        Type targetType =
            argTargetObj.GetType();

        if (argHeaderPath.Contains("."))
        {
            string[] pathParts =
                argHeaderPath.Split('.');

            FieldInfo parentField =
                targetType.GetField(
                    pathParts[0],
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            if (parentField == null)
                return;

            object parentInstance =
                parentField.GetValue(argTargetObj);

            if (parentInstance == null)
            {
                parentInstance =
                    Activator.CreateInstance(
                        parentField.FieldType);

                parentField.SetValue(
                    argTargetObj,
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
                try
                {
                    childField.SetValue(
                        parentInstance,
                        Convert.ChangeType(
                            argValue,
                            childField.FieldType));
                }
                catch
                {
                    Debug.LogWarning(
                        $"[Parse Warning] " +
                        $"{argHeaderPath} 변환 실패: {argValue}");
                }
            }
        }
        else
        {
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
                field.SetValue(
                    argTargetObj,
                    Convert.ChangeType(
                        argValue,
                        field.FieldType));
            }
            catch
            {
                Debug.LogWarning(
                    $"[Parse Warning] " +
                    $"{argHeaderPath} 변환 실패: {argValue}");
            }
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