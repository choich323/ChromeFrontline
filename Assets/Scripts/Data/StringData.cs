using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class LocalizationText
{
    public string en;
    public string kr;
}

[Serializable]
public class StringInfo
{
    public string id;
    public LocalizationText value;
}

[CreateAssetMenu(fileName = "StringData", menuName = "Custom/StringData")]
public class StringData : ScriptableObject
{
    public List<StringInfo> stringInfoList = new List<StringInfo>();

    public IEnumerable<StringInfo> GetInfoList()
    {
        return stringInfoList;
    }
}
