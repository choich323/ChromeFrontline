using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIOption : APopup
{
    [Header("Sliders")]
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private Slider _sensitivitySlider;
    
    [Header("Selectors")]
    [SerializeField] private UISelector _frameRateSelector;
    [SerializeField] private UISelector _qualitySelector;
    [SerializeField] private UISelector _languageSelector;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _soundText;
    [SerializeField] private TextMeshProUGUI _sensitivityText;
    [SerializeField] private TextMeshProUGUI _frameRateText;
    [SerializeField] private TextMeshProUGUI _qualityText;
    [SerializeField] private TextMeshProUGUI _languageText;
    [SerializeField] private TextMeshProUGUI _soundValueText;
    [SerializeField] private TextMeshProUGUI _sensitivityValueText;
    
    private List<string> _frameList = new List<string>();
    private List<string> _qualityList = new List<string>();
    private List<string> _languageList = new List<string>();
    
    public override void Init()
    {
        RefreshText();
        
        _soundSlider.onValueChanged.AddListener(OnSoundChanged);
        _soundSlider.value = Managers.Sound.MasterVolume;
        
        _sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        var camController = Managers.CamController;
        if (camController != null)
        {
            _sensitivitySlider.value = camController.Sensitivity;
        }

        var pm = Managers.Prefs;
        _frameRateSelector.Init(
            _frameList,
            pm.Frame,
            index =>
            {
                int target = index == 0 ? 30 : (index == 1 ? 60 : -1);
                Application.targetFrameRate = target;
                pm.SetFrame(index);
            }
        );
        
        _qualitySelector.Init(
            _qualityList,
            pm.Quality,
            index =>
            {
                QualitySettings.SetQualityLevel(index);
                pm.SetQuality(index);
            }
        );
        
        _languageSelector.Init(
            _languageList,
            (int)Managers.Language.CurrentLanguage,
            index =>
            {
                Managers.Language.SetLanguage(index);
                RefreshText();
            }
        );
    }

    void OnSoundChanged(float argValue)
    {
        Managers.Sound.SetMasterVolume(argValue);
        _soundValueText.text = $"{argValue * 100:F0}";
    }
    
    void OnSensitivityChanged(float argValue)
    {
        var camController = Managers.CamController;
        if (camController != null)
        {
            camController.SetSensitivity(argValue);
            _sensitivityValueText.text = $"{argValue * 100:F0}";
        }
    }
    
    void RefreshText()
    {
        var sm = Managers.String;
        _title.text = sm.GetString(StringID.OptionTitle);
        _soundText.text = sm.GetString(StringID.Sound);
        _sensitivityText.text = sm.GetString(StringID.Sensitivity);
        _frameRateText.text = sm.GetString(StringID.Frame);
        _qualityText.text = sm.GetString(StringID.Quality);
        _languageText.text = sm.GetString(StringID.Language);
        
        _frameList.Clear();
        _frameList.Add(sm.GetString(StringID.Frame30));
        _frameList.Add(sm.GetString(StringID.Frame60));
        _frameList.Add(sm.GetString(StringID.FrameMax));
        _frameRateSelector.SetOptions(_frameList);
        _frameRateSelector.RefreshText();
        
        _qualityList.Clear();
        _qualityList.Add(sm.GetString(StringID.QualityLow));
        _qualityList.Add(sm.GetString(StringID.QualityMedium));
        _qualityList.Add(sm.GetString(StringID.QualityHigh));
        _qualitySelector.SetOptions(_qualityList);
        _qualitySelector.RefreshText();
        
        _languageList.Clear();
        _languageList.Add(sm.GetString(StringID.LangEnglish));
        _languageList.Add(sm.GetString(StringID.LangKorean));
        _languageSelector.SetOptions(_languageList);
        _languageSelector.RefreshText();
    }
}
