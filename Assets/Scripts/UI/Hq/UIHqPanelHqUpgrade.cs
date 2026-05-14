using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHqPanelHqUpgrade : AUIHqRightPanelSelect
{
    private const float HUNDRED_PERCENT = 100f;
    
    [SerializeField] private Button _btnUpgrade;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _gpsText;
    [SerializeField] private TextMeshProUGUI _tierText;
    [SerializeField] private TextMeshProUGUI _productionTimeText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private GameObject _costIcon;
    [SerializeField] private Color _upgradeHighlightColor = Color.green;

    private HeadQuarterUpgradeInfo _hqUpgradeInfo;
    
    protected override void OnInit()
    {
        var tier = Managers.Game.GameField.PlayerHq.Tier;
        _hqUpgradeInfo = Managers.Data.GetHeadQuarterUpgradeInfo(tier);
        _btnUpgrade.interactable = true;
        _btnUpgrade.onClick.AddListener(OnBtnUpgrade);
        _costIcon.SetActive(true);
    }
    
    public override void SetType()
    {
        _panelType = HqRightPanelType.HqUpgrade;
    }

    public override void SetPanel()
    {
        SetText();
    }

    void SetText()
    {
        var newInfo = Managers.Data.GetHeadQuarterUpgradeInfo(_hqUpgradeInfo.level + 1);
        if (newInfo == null)
        {
            _hpText.text = $"{_hqUpgradeInfo.maxHp}";
            _gpsText.text = $"{_hqUpgradeInfo.goldPerSecond}";
            _tierText.text = $"{_hqUpgradeInfo.level}";
            _productionTimeText.text = $"{_hqUpgradeInfo.productionTimeBonus * HUNDRED_PERCENT}%";
            _costText.text = Managers.String.GetString(StringID.MaxLevel);
            _costIcon.SetActive(false);
            _btnUpgrade.interactable = false;
            return;
        }
        
        var colorHex = ColorUtility.ToHtmlStringRGB(_upgradeHighlightColor);
        _hpText.text = $"{_hqUpgradeInfo.maxHp} -> <color=#{colorHex}>{newInfo.maxHp}</color>";
        _gpsText.text = $"{_hqUpgradeInfo.goldPerSecond} -> <color=#{colorHex}>{newInfo.goldPerSecond}</color>";
        _tierText.text = $"{_hqUpgradeInfo.level} -> <color=#{colorHex}>{newInfo.level}</color>";
        _productionTimeText.text = $"{_hqUpgradeInfo.productionTimeBonus*HUNDRED_PERCENT}% -> <color=#{colorHex}>{newInfo.productionTimeBonus*HUNDRED_PERCENT}%</color>";
        
        _costIcon.SetActive(true);
        _costText.text = $"{newInfo.upgradeCost}";
    }

    void OnBtnUpgrade()
    {
        var ph = Managers.UI.PopupHandler;
        var popup = ph.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
        popup.Init();
        var sm = Managers.String;
        string msg = sm.GetString(StringID.ConfirmHqUpgrade);
        string confirm = sm.GetString(StringID.Yes);
        string cancel = sm.GetString(StringID.No);
        bool isConfirm = false;
        popup.SetData(msg, OnConfirm, OnClose, confirm, cancel);

        void OnConfirm()
        {
            isConfirm = true;
        }

        void OnClose()
        {
            ph.ClosePopup();
            
            if (isConfirm)
            {
                var success = Managers.Game.GameField.PlayerHq.UpgradeHq();
                if (success)
                {
                    _hqUpgradeInfo = Managers.Data.GetHeadQuarterUpgradeInfo(_hqUpgradeInfo.level + 1);
                    SetPanel();
                }
            }
        }
    }

    public override void Clear()
    {
        
    }

    public override void Destroy()
    {
        _btnUpgrade.onClick.RemoveAllListeners();
    }
}
