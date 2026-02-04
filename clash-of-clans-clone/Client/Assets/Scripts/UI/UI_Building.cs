namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System;

    public class UI_Building : MonoBehaviour
    {

        [SerializeField] private Data.BuildingID _id = Data.BuildingID.townhall; public Data.BuildingID id { set { _id = value; } }
        [SerializeField] private Button _button = null;
        [SerializeField] private Button _buttonInfo = null;
        [SerializeField] private Image _icon = null;
        [SerializeField] private Image _resourceIcon = null;
        [SerializeField] public TextMeshProUGUI _titleText = null;
        [SerializeField] public TextMeshProUGUI _resourceText = null;
        [SerializeField] public TextMeshProUGUI _timeText = null;
        [SerializeField] public TextMeshProUGUI _countText = null;

        private void Start()
        {
            _button.onClick.AddListener(Clicked);
            _buttonInfo.onClick.AddListener(Info);
        }

        private void Clicked()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Shop.instance.PlaceBuilding(_id);
        }

        public void Initialize(bool haveWorker)
        {
            Data.ServerBuilding building = Player.instance.GetServerBuilding(_id, 1);
            _titleText.text = Language.instance.GetBuildingName(_id);
            if (Language.instance.IsRTL && _titleText.horizontalAlignment == HorizontalAlignmentOptions.Left)
            {
                _titleText.horizontalAlignment = HorizontalAlignmentOptions.Right;
            }
            _titleText.ForceMeshUpdate(true);
            Sprite icon = AssetsBank.GetBuildingIcon(_id);
            if(icon != null)
            {
                _icon.sprite = icon;
            }
            if(building != null)
            {
                _button.interactable = haveWorker;
                int townHallLevel = 1;
                int count = 0;
                for (int i = 0; i < Player.instance.data.buildings.Count; i++)
                {
                    if (Player.instance.data.buildings[i].id == Data.BuildingID.townhall)
                    {
                        townHallLevel = Player.instance.data.buildings[i].level;
                    }
                    if (Player.instance.data.buildings[i].id == _id)
                    {
                        count++;
                    }
                }

                Data.BuildingCount limits = Data.GetBuildingLimits(townHallLevel, _id.ToString());
                if(limits != null)
                {
                    _countText.text = count.ToString() + "/" + limits.count.ToString();
                    if(count >= limits.count)
                    {
                        _button.interactable = false;
                    }
                }
                else
                {
                    _button.interactable = false;
                    _countText.text = "0/0";
                }

                _timeText.text = Tools.SecondsToTimeFormat(building.buildTime);

                if (building.requiredGold > 0)
                {
                    _resourceText.text = building.requiredGold.ToString();
                    _resourceIcon.sprite = AssetsBank.instance.goldIcon;
                }
                else if (building.requiredElixir > 0)
                {
                    _resourceText.text = building.requiredElixir.ToString();
                    _resourceIcon.sprite = AssetsBank.instance.elixirIcon;
                }
                else if (building.requiredDarkElixir > 0)
                {
                    _resourceText.text = building.requiredDarkElixir.ToString();
                    _resourceIcon.sprite = AssetsBank.instance.darkIcon;
                }
                else
                {
                    if(_id == Data.BuildingID.buildershut)
                    {
                        switch (count)
                        {
                            case 0: building.requiredGems = 0; break;
                            case 1: building.requiredGems = 250; break;
                            case 2: building.requiredGems = 500; break;
                            case 3: building.requiredGems = 1000; break;
                            case 4: building.requiredGems = 2000; break;
                            default: building.requiredGems = 0; break;
                        }
                    }
                    _resourceText.text = count >= 5 ? "none" : building.requiredGems.ToString();
                    _resourceIcon.sprite = AssetsBank.instance.gemsIcon;
                }
                if (building.requiredGold <= Player.instance.gold && building.requiredElixir <= Player.instance.elixir && building.requiredDarkElixir <= Player.instance.darkElixir && building.requiredGems <= Player.instance.data.gems)
                {
                    _resourceText.color = Color.white;
                }
                else
                {
                    _resourceText.color = Color.red;
                    _button.interactable = false;
                }
            }
            else
            {
                _resourceText.color = Color.white;
                _timeText.text = "0";
                _resourceText.text = "0";
                _countText.text = "0/0";
                _resourceIcon.sprite = AssetsBank.instance.gemsIcon;
                _button.interactable = false;
            }
            _resourceText.ForceMeshUpdate(true);
            _timeText.ForceMeshUpdate(true);
            _countText.ForceMeshUpdate(true);
        }

        private void Info()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            UI_Info.instance.OpenBuildingInfo(_id, 1);
        }

    }
}