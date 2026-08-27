using System;
using UnityEngine;
using UnityEngine.UI;

public class UIIdentifierButton : MonoBehaviour
{
    [SerializeField] private string _id;
    [SerializeField] private Button _button;

    private void OnEnable()
    {
        Managers.UI.AddIdentifierButton(_id, _button);
    }

    private void OnDisable()
    {
        Managers.UI.RemoveIdentifierButton(_id, _button);
    }
}
