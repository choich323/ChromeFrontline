using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class DialogDataImporter : EditorWindow
{
    private string sheetId = "YOUR_SPREADSHEET_ID_HERE";
    private string gid = "0";
    private string savePath = "Assets/Data/Dialog/DialogData.asset";
    private string dialogDataId;

    private const string AddressableGroupName = "DialogData";

    [MenuItem("Tools/Import Dialog Data")]
    public static void ShowWindow()
    {
        GetWindow<DialogDataImporter>("Dialog Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheet Settings", EditorStyles.boldLabel);

        sheetId = EditorGUILayout.TextField(
            "Spreadsheet ID", sheetId);

        gid = EditorGUILayout.TextField(
            "Sheet GID", gid);

        dialogDataId = EditorGUILayout.TextField(
            "Dialog Data ID", dialogDataId);

        savePath = EditorGUILayout.TextField(
            "Save Path", savePath);

        if (GUILayout.Button("Import"))
        {
            ImportData();
        }
    }

    private async void ImportData()
    {
        string url =
            $"https://docs.google.com/spreadsheets/d/{sheetId}" +
            $"/export?format=csv&gid={gid}";

        try
        {
            string csvContent =
                await DownloadCSVAsync(url);

            ParseCSVAndCreateSO(csvContent);
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[Dialog Import Error] {e.Message}");
        }
    }

    private async Task<string> DownloadCSVAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response =
                await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadAsStringAsync();
        }
    }

    private void ParseCSVAndCreateSO(string csvContent)
    {
        string[] lines = csvContent.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
            return;

        string[] headers =
            SplitCSVLine(lines[0]);

        int dialogDataIdIndex = -1;

        for (int col = 0; col < headers.Length; col++)
        {
            if (headers[col].Trim() == "dialogDataId")
            {
                dialogDataIdIndex = col;
                break;
            }
        }

        if (dialogDataIdIndex == -1)
        {
            Debug.LogError(
                "[Dialog Import] " +
                "'dialogDataId' 컬럼을 찾을 수 없습니다.");

            return;
        }

        // SO 로드 / 생성
        DialogData dialogData =
            AssetDatabase.LoadAssetAtPath<DialogData>(
                savePath);

        if (dialogData == null)
        {
            dialogData =
                CreateInstance<DialogData>();

            dialogData.dialogDataId =
                dialogDataId;

            AssetDatabase.CreateAsset(
                dialogData,
                savePath);
        }
        else
        {
            dialogData.dialogDataId =
                dialogDataId;
        }

        // 기존 데이터 초기화
        dialogData.dialogInfoList =
            new List<DialogInfo>();

        // Row 파싱
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values =
                SplitCSVLine(lines[i]);

            if (dialogDataIdIndex >= values.Length)
                continue;

            string rowDialogDataId =
                values[dialogDataIdIndex].Trim();

            // 입력한 DialogDataId에 해당하는 데이터만 사용
            if (rowDialogDataId != dialogDataId)
                continue;

            DialogInfo dialogInfo =
                new DialogInfo();

            for (int col = 0;
                 col < headers.Length;
                 col++)
            {
                if (col >= values.Length)
                    continue;

                if (col == dialogDataIdIndex)
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
                    dialogInfo,
                    header,
                    value);
            }

            dialogData.dialogInfoList.Add(
                dialogInfo);
        }

        EditorUtility.SetDirty(dialogData);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RegisterAddressable(
            dialogData,
            savePath);

        Debug.Log(
            $"[Dialog Import 완료] " +
            $"{dialogData.dialogInfoList.Count}개의 " +
            $"DialogInfo가 갱신되었습니다.");
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

            childField?.SetValue(
                parentInstance,
                Convert.ChangeType(
                    value,
                    childField.FieldType));
        }
        else
        {
            FieldInfo field =
                targetType.GetField(
                    headerPath,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            field?.SetValue(
                targetObj,
                Convert.ChangeType(
                    value,
                    field.FieldType));
        }
    }

    private void RegisterAddressable(
        DialogData dialogData,
        string assetPath)
    {
        AddressableAssetSettings settings =
            AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            Debug.LogError(
                "[Dialog Import] " +
                "AddressableAssetSettings가 없습니다.");

            return;
        }

        AddressableAssetGroup group =
            settings.FindGroup(
                AddressableGroupName);

        if (group == null)
        {
            group = settings.CreateGroup(
                AddressableGroupName,
                false,
                false,
                false,
                null);
        }

        string guid =
            AssetDatabase.AssetPathToGUID(
                assetPath);

        AddressableAssetEntry entry =
            settings.CreateOrMoveEntry(
                guid,
                group);

        entry.address =
            dialogData.dialogDataId;

        EditorUtility.SetDirty(settings);
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