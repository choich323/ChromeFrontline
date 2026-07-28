using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIWorldSelectUnit : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _worldNameText;
    [SerializeField] private Button _btnNode;
    [SerializeField] private Image _bgImage;
    [SerializeField] private Color _selectedColor;

    private string _targetWorldId;
    private System.Action<string> _onClickAction;

    public string WorldId => _targetWorldId;
    
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
        EnableSelectedColor(true);
        _onClickAction?.Invoke(_targetWorldId);
    }

    public void EnableSelectedColor(bool argIsEnable)
    {
        _bgImage.color = argIsEnable ? _selectedColor : Color.white;
    }
}