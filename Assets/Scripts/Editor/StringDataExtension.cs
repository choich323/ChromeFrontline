using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(StringData))]
public class StringDataExtension : Editor
{
    private string _searchString = "";

    public override void OnInspectorGUI()
    {
        StringData data = (StringData)target;
        serializedObject.Update();

        SerializedProperty listProp = serializedObject.FindProperty("stringInfoList");

        // --- 상단 컨트롤 바 ---
        EditorGUILayout.BeginVertical("helpbox");
        {
            EditorGUILayout.LabelField("🔤 String Data Manager", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("ID 정렬 (A-Z)")) SortByID(data);
            
            // [추가 버튼] 리스트 맨 뒤에 새 항목 추가
            GUI.color = Color.green; // 버튼 색상 강조
            if (GUILayout.Button("새 항목 추가 (+)"))
            {
                Undo.RecordObject(data, "Add New String Info");
                data.stringInfoList.Add(new StringInfo());
                EditorUtility.SetDirty(data);
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            // [검색 필드]
            EditorGUILayout.BeginHorizontal();
            _searchString = EditorGUILayout.TextField("검색 필터", _searchString, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) _searchString = "";
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // --- 리스트 표시 영역 ---
        if (listProp == null || listProp.arraySize == 0)
        {
            EditorGUILayout.HelpBox("리스트가 비어 있습니다.", MessageType.Info);
        }
        else
        {
            DrawFilteredList(data, listProp);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFilteredList(StringData data, SerializedProperty listProp)
    {
        bool isFiltering = !string.IsNullOrEmpty(_searchString);
        int indexToRemove = -1; // 삭제할 인덱스 추적

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            SerializedProperty idProp = element.FindPropertyRelative("id");

            if (isFiltering && !idProp.stringValue.ToLower().Contains(_searchString.ToLower()))
                continue;

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(element, true);
                
                GUI.color = new Color(1f, 0.6f, 0.6f);
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    indexToRemove = i;
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        if (indexToRemove != -1)
        {
            Undo.RecordObject(data, "Remove String Info");
            data.stringInfoList.RemoveAt(indexToRemove);
            EditorUtility.SetDirty(data);
        }
    }

    private void SortByID(StringData data)
    {
        Undo.RecordObject(data, "Sort String Data By ID");
        data.stringInfoList = data.stringInfoList
            .OrderBy(info => info?.id ?? string.Empty)
            .ToList();
        EditorUtility.SetDirty(data);
        serializedObject.Update();
    }
}
