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
        var newInfo = Managers.Data.GetHeadQuarterUpgradeInfo(_hqUpgradeInfo.tier + 1);
        if (newInfo == null)
        {
            _hpText.SetText($"{_hqUpgradeInfo.maxHp}");
            _gpsText.SetText($"{_hqUpgradeInfo.goldPerSecond}");
            _tierText.SetText($"{_hqUpgradeInfo.tier}");
            _productionTimeText.SetText($"{_hqUpgradeInfo.productionTimeBonus * HUNDRED_PERCENT}%");
            _costText.SetText(Managers.String.GetString(StringID.MaxLevel));
            _costIcon.SetActive(false);
            _btnUpgrade.interactable = false;
            return;
        }
        
        var colorHex = ColorUtility.ToHtmlStringRGB(_upgradeHighlightColor);
        _hpText.SetText($"{_hqUpgradeInfo.maxHp} -> <color=#{colorHex}>{newInfo.maxHp}</color>");
        _gpsText.SetText($"{_hqUpgradeInfo.goldPerSecond} -> <color=#{colorHex}>{newInfo.goldPerSecond}</color>");
        _tierText.SetText($"{_hqUpgradeInfo.tier} -> <color=#{colorHex}>{newInfo.tier}</color>");
        _productionTimeText.SetText($"{_hqUpgradeInfo.productionTimeBonus*HUNDRED_PERCENT}% -> <color=#{colorHex}>{newInfo.productionTimeBonus*HUNDRED_PERCENT}%</color>");
        
        _costIcon.SetActive(true);
        _costText.SetText($"{newInfo.upgradeCost}");
    }

    void OnBtnUpgrade()
    {
        var sound = Managers.Sound;
        sound.PlaySelectSfx();
        
        var ph = Managers.UI.PopupHandler;
        var popup = ph.OpenPopup<UIConfirm>(PrefabID.UIConfirm);
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
            sound.PlaySelectSfx();
            ph.ClosePopup();
            
            if (isConfirm)
            {
                var success = Managers.Game.GameField.PlayerHq.UpgradeHq();
                if (success)
                {
                    sound.PlayUpgradeSfx(true);
                    _hqUpgradeInfo = Managers.Data.GetHeadQuarterUpgradeInfo(_hqUpgradeInfo.tier + 1);
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
