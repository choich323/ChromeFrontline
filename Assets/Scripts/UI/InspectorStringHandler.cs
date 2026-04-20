using System;
using TMPro;
using UnityEngine;

public class InspectorStringHandler : MonoBehaviour
{
    [SerializeField] private string _stringId;
    [SerializeField] private TextMeshProUGUI _text;

    private void OnEnable()
    {
        _text.text = Managers.String.GetString(_stringId);
    }
}
