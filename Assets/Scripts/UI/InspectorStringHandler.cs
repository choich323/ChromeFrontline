using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class InspectorStringGroup
{
    public string stringId;
    public TextMeshProUGUI text;
}

public class InspectorStringHandler : MonoBehaviour
{
    [SerializeField] private List<InspectorStringGroup> _stringGroupList;

    private void OnEnable()
    {
        foreach (var group in _stringGroupList)
        {
            group.text.SetText(Managers.String.GetString(group.stringId));
        }
    }
}
