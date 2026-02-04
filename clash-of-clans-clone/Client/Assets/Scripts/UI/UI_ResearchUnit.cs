namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DevelopersHub.RealtimeNetworking.Client;
    using TMPro;
    using System;

    public class UI_ResearchUnit : MonoBehaviour
    {

        [SerializeField] private Data.UnitID _id = Data.UnitID.barbarian; public Data.UnitID id { get { return _id; } set { _id = value; } }
        [SerializeField] private Button _button = null;
        [SerializeField] private Button _buttonInfo = null;
        [SerializeField] private Image _icon = null;
        [SerializeField] private Image _resourceIcon = null;
        [SerializeField] public TextMeshProUGUI _titleText = null;
        [SerializeField] public TextMeshProUGUI _resourceText = null;
        [SerializeField] public TextMeshProUGUI _levelText = null;
        [SerializeField] public TextMeshProUGUI _maxLevelText = null;
        [SerializeField] public TextMeshProUGUI _reqTimeText = null;
        [SerializeField] public TextMeshProUGUI _timeText = null;
        [SerializeField] public GameObject _normalPanel = null;
        [SerializeField] public GameObject _maxPanel = null;
        [SerializeField] public GameObject _researchingPanel = null;

        private bool researching = false;
        private DateTime _endResearch;

        private void Start()
        {
            _button.onClick.AddListener(Clicked);
            _buttonInfo.onClick.AddListener(Info);
        }

        public void Initialize()
        {
            _button.interactable = true;
            researching = false;
            _normalPanel.SetActive(false);
            _maxPanel.SetActive(false);
            _researchingPanel.SetActive(false);
            _titleText.text = Language.instance.GetUnitName(_id);
            if (Language.instance.IsRTL && _titleText.horizontalAlignment == HorizontalAlignmentOptions.Left)
            {
                _titleText.horizontalAlignment = HorizontalAlignmentOptions.Right;
            }
            int dataIndex = -1;
            int researchIndex = -1;
            int level = 1;
            for (int i = 0; i < Player.instance.initializationData.research.Count; i++)
            {
                if (Player.instance.initializationData.research[i].type == Data.ResearchType.unit && Player.instance.initializationData.research[i].globalID == _id.ToString())
                {
                    researchIndex = i; level = Player.instance.initializationData.research[i].level; break;
                }
            }
            Sprite icon = AssetsBank.GetUnitIcon(_id);
            if (icon != null)
            {
                _icon.sprite = icon;
            }
            for (int i = 0; i < Player.instance.initializationData.serverUnits.Count; i++)
            {
                if (Player.instance.initializationData.serverUnits[i].id == _id && Player.instance.initializationData.serverUnits[i].level == level + 1)
                {
                    dataIndex = i; break;
                }
            }
            if (dataIndex >= 0)
            {
                if (researchIndex >= 0)
                {
                    if (Player.instance.initializationData.research[researchIndex].end <= Player.instance.data.nowTime)
                    {
                        SetupItem(level, dataIndex);
                    }
                    else
                    {
                        _timeText.text = "";
                        _button.interactable = false;
                        _endResearch = Player.instance.initializationData.research[researchIndex].end;
                        _researchingPanel.SetActive(true);
                        researching = true;
                    }
                }
                else
                {
                    SetupItem(level, dataIndex);
                }
            }
            else
            {
                _maxLevelText.text = "+" + level.ToString();
                _button.interactable = false;
                _maxPanel.SetActive(true);
            }
        }

        private void SetupItem(int level, int dataIndex)
        {
            _reqTimeText.text = Tools.SecondsToTimeFormat(Player.instance.initializationData.serverUnits[dataIndex].researchTime);
            _levelText.text = "+" + level.ToString();
            // BIO-CLASH: Bypass legacy resource costs for fitness economy
            // Research is now free - only check gems if required
            if (Player.instance.initializationData.serverUnits[dataIndex].researchGems > 0)
            {
                _resourceIcon.sprite = AssetsBank.instance.gemsIcon;
                _resourceText.text = Player.instance.initializationData.serverUnits[dataIndex].researchGems.ToString();
                if (_button.interactable && Player.instance.initializationData.serverUnits[dataIndex].researchGems > Player.instance.data.gems)
                {
                    _button.interactable = false;
                }
            }
            else
            {
                _resourceText.text = "FREE";
                _resourceIcon.gameObject.SetActive(false);
            }
            _normalPanel.SetActive(true);
        }

        private void Clicked()
        {
            _button.interactable = false;
            Packet paket = new Packet();
            paket.Write((int)Player.RequestsID.RESEARCH);
            paket.Write((int)Data.ResearchType.unit);
            paket.Write(_id.ToString());
            Sender.TCP_Send(paket);
        }

        private void Update()
        {
            if (researching)
            {
                if(_endResearch > Player.instance.data.nowTime)
                {
                    _timeText.text = Tools.SecondsToTimeFormat(_endResearch - Player.instance.data.nowTime);
                }
                else
                {
                    researching = false;
                    Initialize();
                }
            }
        }

        private void Info()
        {
            UI_Info.instance.OpenUnitInfo(_id);
        }

    }
}