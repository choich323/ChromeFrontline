using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UISelector : MonoBehaviour
{
    private const int DEFAULT_INDEX = 0;
    
    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private Button _leftBtn;
    [SerializeField] private Button _rightBtn;

    private List<string> _options;
    private int _curIndex = DEFAULT_INDEX;
    private Action<int> _onValueChanged;

    public void Init(List<string> argOptions, int argStartIndex, Action<int> argOnValueChanged)
    {
        Clear();
        
        _options = argOptions;
        _curIndex = argStartIndex;
        _onValueChanged = argOnValueChanged;
        
        _leftBtn.onClick.AddListener(OnLeftClick);
        _rightBtn.onClick.AddListener(OnRightClick);
        
        RefreshText();
    }

    public void Clear()
    {
        _options = new List<string>();
        _curIndex = DEFAULT_INDEX;
        _onValueChanged = null;
        
        _leftBtn.onClick.RemoveAllListeners();
        _rightBtn.onClick.RemoveAllListeners();
    }
    
    void OnLeftClick()
    {
        _curIndex--;
        if (_curIndex < 0)
        {
            _curIndex = _options.Count - 1;
        }
        
        _onValueChanged?.Invoke(_curIndex);
        RefreshText();
    }

    void OnRightClick()
    {
        _curIndex++;
        if (_curIndex >= _options.Count)
        {
            _curIndex = 0;
        }
        
        _onValueChanged?.Invoke(_curIndex);
        RefreshText();
    }

    public void RefreshText()
    {
        _valueText.text = _options[_curIndex];
    }

    public void SetOptions(List<string> argOptions)
    {
        _options = argOptions;
    }
}
