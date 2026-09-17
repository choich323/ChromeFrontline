#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class SkillDataImporter : EditorWindow
{
    [SerializeField] private string _sheetId = "YOUR_SPREADSHEET_ID";
    [SerializeField] private string _gid = "0";
    [SerializeField] private string _savePath = "Assets/Data/SkillData.asset";

    [MenuItem("Tools/Import Skill Data")]
    public static void ShowWindow()
    {
        GetWindow<SkillDataImporter>("Skill Data Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheet Settings", EditorStyles.boldLabel);

        _sheetId = EditorGUILayout.TextField("Spreadsheet ID", _sheetId);
        _gid = EditorGUILayout.TextField("Sheet GID", _gid);
        _savePath = EditorGUILayout.TextField("Save Path", _savePath);

        if (GUILayout.Button("Import & Parse"))
        {
            ImportData();
        }
    }

    private async void ImportData()
    {
        string url =
            $"https://docs.google.com/spreadsheets/d/{_sheetId}/export?format=csv&gid={_gid}";

        try
        {
            Debug.Log("[SkillData Import] CSV 다운로드 중...");

            string csvContent = await DownloadCSVAsync(url);

            ParseCSVAndCreateSO(csvContent);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SkillData Import] 다운로드 실패: {e}");
        }
    }

    private async Task<string> DownloadCSVAsync(string argUrl)
    {
        using HttpClient client = new HttpClient();

        HttpResponseMessage response = await client.GetAsync(argUrl);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private void ParseCSVAndCreateSO(string argCsvContent)
    {
        string[] lines = argCsvContent.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            Debug.LogWarning("[SkillData Import] CSV 데이터가 없습니다.");
            return;
        }

        string[] headers = SplitCSVLine(lines[0]);

        for (int i = 0; i < headers.Length; i++)
        {
            headers[i] = headers[i].Trim();
        }

        int idIndex = Array.IndexOf(headers, "id");

        if (idIndex == -1)
        {
            Debug.LogError("[SkillData Import] id 컬럼을 찾을 수 없습니다.");
            return;
        }

        string assetPath = _savePath;

        SkillData skillData =
            AssetDatabase.LoadAssetAtPath<SkillData>(assetPath);

        if (skillData == null)
        {
            skillData = CreateInstance<SkillData>();

            AssetDatabase.CreateAsset(skillData, assetPath);
        }

        Dictionary<int, SkillInfo> existingInfoDict = new Dictionary<int, SkillInfo>();

        foreach (SkillInfo info in skillData.infoList)
        {
            existingInfoDict[info.id] = info;
        }

        List<SkillInfo> importedInfoList = new List<SkillInfo>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = SplitCSVLine(lines[i]);

            if (values.Length == 0)
                continue;

            if (idIndex >= values.Length ||
                string.IsNullOrWhiteSpace(values[idIndex]))
            {
                continue;
            }

            int id = int.Parse(
                values[idIndex].Trim(),
                CultureInfo.InvariantCulture);

            SkillInfo skillInfo;

            if (existingInfoDict.TryGetValue(id, out SkillInfo existingInfo))
            {
                skillInfo = existingInfo;
            }
            else
            {
                skillInfo = new SkillInfo();
            }

            for (int col = 0; col < headers.Length; col++)
            {
                if (col >= values.Length)
                    continue;

                string header = headers[col].Trim();
                string value = values[col].Trim();

                if (string.IsNullOrEmpty(header) ||
                    string.IsNullOrEmpty(value))
                {
                    continue;
                }

                try
                {
                    ApplyValueViaReflection(
                        skillInfo,
                        header,
                        value);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[SkillData Parse Error] " +
                        $"Column: {header}, Value: {value}\n{e}");
                }
            }

            importedInfoList.Add(skillInfo);
        }

        skillData.infoList.Clear();
        skillData.infoList.AddRange(importedInfoList);
        
        EditorUtility.SetDirty(skillData);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[SkillData Import 완료] " +
            $"{skillData.infoList.Count}개의 스킬 데이터를 갱신했습니다.");
    }

    private void ApplyValueViaReflection(
        object argTarget,
        string argPath,
        string argValue)
    {
        string[] fieldNames = argPath.Split('.');

        object currentObject = argTarget;
        Type currentType = currentObject.GetType();

        for (int i = 0; i < fieldNames.Length; i++)
        {
            FieldInfo field =
                currentType.GetField(
                    fieldNames[i],
                    BindingFlags.Public |
                    BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogWarning(
                    $"[SkillData Parse Warning] " +
                    $"{currentType.Name}에 '{fieldNames[i]}' 필드가 없습니다.");

                return;
            }

            bool isLastField = i == fieldNames.Length - 1;

            if (isLastField)
            {
                object convertedValue =
                    ConvertValue(field.FieldType, argValue);

                field.SetValue(currentObject, convertedValue);

                return;
            }

            object childObject = field.GetValue(currentObject);

            if (childObject == null)
            {
                childObject = Activator.CreateInstance(field.FieldType);
                field.SetValue(currentObject, childObject);
            }

            currentObject = childObject;
            currentType = field.FieldType;
        }
    }

    private object ConvertValue(Type argType, string argValue)
    {
        if (argType == typeof(string))
            return argValue;

        if (argType == typeof(int))
        {
            return int.Parse(
                argValue,
                CultureInfo.InvariantCulture);
        }

        if (argType == typeof(float))
        {
            return float.Parse(
                argValue,
                CultureInfo.InvariantCulture);
        }

        if (argType == typeof(double))
        {
            return double.Parse(
                argValue,
                CultureInfo.InvariantCulture);
        }

        if (argType == typeof(bool))
        {
            return bool.Parse(argValue);
        }

        if (argType.IsEnum)
        {
            return Enum.Parse(
                argType,
                argValue,
                true);
        }

        if (argType == typeof(Vector2))
        {
            return ParseVector2(argValue);
        }

        if (argType.IsGenericType &&
            argType.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type elementType = argType.GetGenericArguments()[0];

            return ParseList(elementType, argValue);
        }

        throw new NotSupportedException(
            $"지원하지 않는 타입입니다: {argType}");
    }

    private object ParseList(
        Type argElementType,
        string argValue)
    {
        if (argElementType == typeof(Vector2))
        {
            return ParseVertexList(argValue);
        }

        if (argElementType == typeof(SkillActionData))
        {
            return ParseActionList(argValue);
        }

        throw new NotSupportedException(
            $"지원하지 않는 List 타입입니다: List<{argElementType.Name}>");
    }

    private Vector2 ParseVector2(string argValue)
    {
        Match match = Regex.Match(
            argValue,
            @"^\s*\(\s*(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)\s*\)\s*$");

        if (!match.Success)
        {
            throw new FormatException(
                $"Vector2 형식이 올바르지 않습니다: {argValue}");
        }

        float x = float.Parse(
            match.Groups[1].Value,
            CultureInfo.InvariantCulture);

        float y = float.Parse(
            match.Groups[2].Value,
            CultureInfo.InvariantCulture);

        return new Vector2(x, y);
    }

    private List<Vector2> ParseVertexList(string argValue)
    {
        List<Vector2> vertexList = new List<Vector2>();

        MatchCollection matches = Regex.Matches(
            argValue,
            @"\(\s*(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)\s*\)");

        foreach (Match match in matches)
        {
            float x = float.Parse(
                match.Groups[1].Value,
                CultureInfo.InvariantCulture);

            float y = float.Parse(
                match.Groups[2].Value,
                CultureInfo.InvariantCulture);

            vertexList.Add(new Vector2(x, y));
        }

        return vertexList;
    }

    private List<SkillActionData> ParseActionList(string argValue)
    {
        List<SkillActionData> actionList =
            new List<SkillActionData>();

        MatchCollection actionMatches = Regex.Matches(
            argValue,
            @"\(([^()]*)\)");

        foreach (Match actionMatch in actionMatches)
        {
            string[] values =
                actionMatch.Groups[1].Value.Split(':');

            if (values.Length < 4)
            {
                Debug.LogWarning(
                    $"[SkillData Parse Warning] " +
                    $"Action 형식이 올바르지 않습니다: " +
                    $"{actionMatch.Value}");

                continue;
            }

            SkillActionData actionData =
                new SkillActionData();

            actionData.type =
                ParseEnum<SkillActionType>(values[0]);

            actionData.value =
                ParseFloat(values[1]);

            actionData.duration =
                ParseFloat(values[2]);

            actionData.interval =
                ParseFloat(values[3]);

            if (values.Length > 4)
            {
                actionData.statModifierList =
                    ParseStatModifierList(values);
            }

            actionList.Add(actionData);
        }

        return actionList;
    }

    private List<StatModifierData> ParseStatModifierList(
        string[] argValues)
    {
        List<StatModifierData> modifierList =
            new List<StatModifierData>();

        // 0 : ActionType
        // 1 : Value
        // 2 : Duration
        // 3 : Interval
        // 4부터 StatType / Value 반복

        for (int i = 4; i + 1 < argValues.Length; i += 2)
        {
            StatModifierData modifierData =
                new StatModifierData();

            modifierData.type =
                ParseEnum<StatType>(argValues[i]);

            modifierData.value =
                ParseFloat(argValues[i + 1]);

            modifierList.Add(modifierData);
        }

        return modifierList;
    }

    private T ParseEnum<T>(string argValue)
        where T : struct, Enum
    {
        if (!Enum.TryParse(
            argValue.Trim(),
            true,
            out T result))
        {
            throw new FormatException(
                $"Enum 변환 실패: {argValue}");
        }

        return result;
    }

    private float ParseFloat(string argValue)
    {
        return float.Parse(
            argValue.Trim(),
            CultureInfo.InvariantCulture);
    }

    private string[] SplitCSVLine(string argLine)
    {
        Regex csvRegex = new Regex(
            ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        string[] result = csvRegex.Split(argLine);

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = result[i]
                .Replace("\"\"", "\"")
                .Trim('"');
        }

        return result;
    }
}
#endif