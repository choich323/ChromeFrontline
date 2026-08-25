using TMPro;
using UnityEngine;

public class UIDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _talker;
    [SerializeField] private TextMeshProUGUI _text;

    public void SetText(string argTalker, string argText)
    {
        _talker.SetText(argTalker);
        _text.SetText(argText);
    }
}
