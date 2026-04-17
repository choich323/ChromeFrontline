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
        // 1. 데이터 동기화 시작
        StringData data = (StringData)target;
        serializedObject.Update();

        SerializedProperty listProp = serializedObject.FindProperty("stringInfoList");

        // --- 상단 컨트롤 바 (정렬 및 검색) ---
        EditorGUILayout.BeginVertical("helpbox");
        {
            EditorGUILayout.LabelField("🔤 String Data Manager", EditorStyles.boldLabel);
            
            // [정렬 버튼] 클릭 시 Undo 기록 후 데이터 재배치
            if (GUILayout.Button("ID 기준 알파벳 정렬 (A-Z)", GUILayout.Height(25)))
            {
                SortByID(data);
            }

            EditorGUILayout.Space(2);

            // [검색 필드]
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("검색", GUILayout.Width(40));
                _searchString = EditorGUILayout.TextField(_searchString, EditorStyles.toolbarSearchField);
                if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(45)))
                {
                    _searchString = "";
                }
            }
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
            DrawFilteredList(listProp);
        }

        // 2. 인스펙터 수정사항 적용 및 저장
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFilteredList(SerializedProperty listProp)
    {
        bool isFiltering = !string.IsNullOrEmpty(_searchString);
        
        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);
            SerializedProperty idProp = element.FindPropertyRelative("id");

            // 검색어가 포함되지 않은 항목은 그리지 않고 스킵
            if (isFiltering && !idProp.stringValue.ToLower().Contains(_searchString.ToLower()))
                continue;

            EditorGUILayout.BeginVertical("box");
            {
                // StringInfo 내부의 모든 필드를 자동으로 그려줌
                EditorGUILayout.PropertyField(element, true);
            }
            EditorGUILayout.EndVertical();
        }
    }

    private void SortByID(StringData data)
    {
        // 핵심: 수정 직전에 Undo 시스템에 현재 상태를 저장 (Ctrl+Z 가능하게 함)
        Undo.RecordObject(data, "Sort String Data By ID");
        
        data.stringInfoList = data.stringInfoList
            .OrderBy(info => info?.id ?? string.Empty)
            .ToList();

        EditorUtility.SetDirty(data);
        
        serializedObject.Update(); 
        
        Debug.Log($"<color=cyan>[StringData]</color> '{data.name}' 정렬 완료 및 기록됨.");
    }
}
