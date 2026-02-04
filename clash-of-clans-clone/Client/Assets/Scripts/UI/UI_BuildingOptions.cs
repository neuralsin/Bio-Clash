namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_BuildingOptions : MonoBehaviour
    {

        [SerializeField] public GameObject _elements = null;

        private static UI_BuildingOptions _instance = null; public static UI_BuildingOptions instance { get { return _instance; } }

        public RectTransform infoPanel = null;
        public RectTransform upgradePanel = null;
        public RectTransform instantPanel = null;
        public RectTransform trainPanel = null;
        public RectTransform clanPanel = null;
        public RectTransform spellPanel = null;
        public RectTransform researchPanel = null;
        public RectTransform removePanel = null;
        public RectTransform boostPanel = null;

        public Button infoButton = null;
        public Button upgradeButton = null;
        public Button instantButton = null;
        public Button trainButton = null;
        public Button clanButton = null;
        public Button spellButton = null;
        public Button researchButton = null;
        public Button removeButton = null;
        public Button boostButton = null;

        public TextMeshProUGUI instantCost = null;
        public TextMeshProUGUI removeCost = null;
        public TextMeshProUGUI boostCost = null;
        public Image removeCostIcon = null;
        [HideInInspector] public bool canDo = false;

        private void Awake()
        {
            _instance = this;
            _elements.SetActive(false);
        }

        public void SetStatus(bool status)
        {
            if(status && Building.selectedinstance != null)
            {
                bool isChainging = Building.selectedinstance.lastChange >= Player.instance.lastUpdateSent;
                int instantGemCost = 0;
                infoPanel.gameObject.SetActive(UI_Main.instance.isActive);
                upgradePanel.gameObject.SetActive(!isChainging && Building.selectedinstance.data.isConstructing == false && UI_Main.instance.isActive && Building.selectedinstance.data.id != Data.BuildingID.buildershut && Building.selectedinstance.data.id != Data.BuildingID.obstacle);
                if (Building.selectedinstance.data.isConstructing)
                {
                    instantGemCost = Data.GetInstantBuildRequiredGems((int)(Building.selectedinstance.data.constructionTime - Player.instance.data.nowTime).TotalSeconds);
                    instantCost.text = instantGemCost.ToString();
                    if(instantGemCost > Player.instance.data.gems)
                    {
                        instantCost.color = Color.red;
                    }
                    else
                    {
                        instantCost.color = Color.white;
                    }
                    instantCost.ForceMeshUpdate(true);
                }
                instantPanel.gameObject.SetActive(!isChainging && Building.selectedinstance.data.isConstructing == true && UI_Main.instance.isActive && instantGemCost > 0);
                if (Building.selectedinstance.data.id == Data.BuildingID.obstacle && UI_Main.instance.isActive && Building.selectedinstance.data.level > 0 && Building.selectedinstance.data.isConstructing == false)
                {
                    canDo = true;
                    int index = -1;
                    for (int i = 0; i < Player.instance.initializationData.serverBuildings.Count; i++)
                    {
                        if(Player.instance.initializationData.serverBuildings[i].id != Data.BuildingID.obstacle.ToString() || Player.instance.initializationData.serverBuildings[i].level != Building.selectedinstance.data.level)
                        {
                            continue;
                        }
                        index = i;
                        break;
                    }
                    if(index >= 0)
                    {
                        if(Player.instance.initializationData.serverBuildings[index].requiredGold > 0)
                        {
                            removeCostIcon.sprite = AssetsBank.instance.goldIcon;
                            removeCost.text = Player.instance.initializationData.serverBuildings[index].requiredGold.ToString();
                            if(Player.instance.gold >= Player.instance.initializationData.serverBuildings[index].requiredGold)
                            {
                                removeCost.color = Color.white;
                            }
                            else
                            {
                                canDo = false;
                                removeCost.color = Color.red;
                            }
                        }
                        else if (Player.instance.initializationData.serverBuildings[index].requiredElixir > 0)
                        {
                            removeCostIcon.sprite = AssetsBank.instance.elixirIcon;
                            removeCost.text = Player.instance.initializationData.serverBuildings[index].requiredElixir.ToString();
                            if (Player.instance.elixir >= Player.instance.initializationData.serverBuildings[index].requiredElixir)
                            {
                                removeCost.color = Color.white;
                            }
                            else
                            {
                                canDo = false;
                                removeCost.color = Color.red;
                            }
                        }
                        else if (Player.instance.initializationData.serverBuildings[index].requiredDarkElixir > 0)
                        {
                            removeCostIcon.sprite = AssetsBank.instance.darkIcon;
                            removeCost.text = Player.instance.initializationData.serverBuildings[index].requiredDarkElixir.ToString();
                            if (Player.instance.darkElixir >= Player.instance.initializationData.serverBuildings[index].requiredDarkElixir)
                            {
                                removeCost.color = Color.white;
                            }
                            else
                            {
                                canDo = false;
                                removeCost.color = Color.red;
                            }
                        }
                        else
                        {
                            removeCostIcon.sprite = AssetsBank.instance.gemsIcon;
                            removeCost.text = Player.instance.initializationData.serverBuildings[index].requiredGems.ToString();
                            if (Player.instance.data.gems >= Player.instance.initializationData.serverBuildings[index].requiredGems)
                            {
                                removeCost.color = Color.white;
                            }
                            else
                            {
                                canDo = false;
                                removeCost.color = Color.red;
                            }
                        }
                    }
                    removePanel.gameObject.SetActive(true);
                    removeCost.ForceMeshUpdate(true);
                }
                else
                {
                    removePanel.gameObject.SetActive(false);
                }
                if ((Building.selectedinstance.data.id == Data.BuildingID.goldmine || Building.selectedinstance.data.id == Data.BuildingID.elixirmine || Building.selectedinstance.data.id == Data.BuildingID.darkelixirmine) && Building.selectedinstance.data.level > 0)
                {
                    canDo = true;
                    int cost = Data.GetBoostResourcesCost(Building.selectedinstance.data.id, Building.selectedinstance.data.level);
                    boostCost.text = cost.ToString();
                    if (Player.instance.data.gems >= cost)
                    {
                        boostCost.color = Color.white;
                    }
                    else
                    {
                        canDo = false;
                        boostCost.color = Color.red;
                    }
                    boostPanel.gameObject.SetActive(Building.selectedinstance.data.boost < Player.instance.data.nowTime);
                    boostCost.ForceMeshUpdate(true);
                }
                else
                {
                    boostPanel.gameObject.SetActive(false);
                }
                trainPanel.gameObject.SetActive(!isChainging && (Building.selectedinstance.data.id == Data.BuildingID.armycamp || Building.selectedinstance.data.id == Data.BuildingID.barracks) && UI_Main.instance.isActive && Building.selectedinstance.data.level > 0);
                clanPanel.gameObject.SetActive(Building.selectedinstance.data.id == Data.BuildingID.clancastle && UI_Main.instance.isActive && Building.selectedinstance.data.level > 0);
                spellPanel.gameObject.SetActive(Building.selectedinstance.data.id == Data.BuildingID.spellfactory && UI_Main.instance.isActive && Building.selectedinstance.data.level > 0);
                researchPanel.gameObject.SetActive(Building.selectedinstance.data.id == Data.BuildingID.laboratory && UI_Main.instance.isActive && Building.selectedinstance.data.level > 0);
            }
            _elements.SetActive(status);
        }

    }
}