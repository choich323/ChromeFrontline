using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIWorldSelectUnit : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _worldNameText;
    [SerializeField] private Button _btnNode;

    private string _targetWorldId;
    private System.Action<string> _onClickAction;

    public void Init(string argWorldId, int argWorldNumber, string argWorldName, System.Action<string> argOnClickAction)
    {
        _targetWorldId = argWorldId;
        _onClickAction = argOnClickAction;

        string world = Managers.String.GetString(StringID.World);
        _worldNameText.text = $"{world} {argWorldNumber} : {argWorldName}";
        
        _btnNode.onClick.RemoveAllListeners();
        _btnNode.onClick.AddListener(OnClick);
    }
    
    void OnClick()
    {
        Managers.Sound.PlaySelectSfx();
        _onClickAction?.Invoke(_targetWorldId);
    }
}