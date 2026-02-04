namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_Shop : MonoBehaviour
    {

        [SerializeField] public GameObject _elements = null;
        [SerializeField] private Button _closeButton = null;
        [SerializeField] public RectTransform _buildingsGrid = null;
        [SerializeField] public TextMeshProUGUI _goldText = null;
        [SerializeField] public TextMeshProUGUI _elixirText = null;
        [SerializeField] public TextMeshProUGUI _darkText = null;
        [SerializeField] public TextMeshProUGUI _gemsText = null;
        [SerializeField] private UI_Building _buildingPrefab = null;
        [SerializeField] private Data.BuildingID[] _buildingsAvailable = null;

        private bool _active = false; public bool isActive { get { return _active; } }
        private static UI_Shop _instance = null; public static UI_Shop instance { get { return _instance; } }
        private List<UI_Building> ui_buildings = new List<UI_Building>();

        private void Awake()
        {
            _instance = this;
            _elements.SetActive(false);
        }

        private void Start()
        {
            if (_buildingsAvailable != null)
            {
                Data.BuildingID[] buildingsAvailable = _buildingsAvailable.Distinct().ToArray();
                for (int i = 0; i < buildingsAvailable.Length; i++)
                {
                    UI_Building building = Instantiate(_buildingPrefab, _buildingsGrid);
                    building.id = buildingsAvailable[i];
                    ui_buildings.Add(building);
                }
            }
            _closeButton.onClick.AddListener(CloseShop);
        }

        public bool IsBuildingInShop(Data.BuildingID id)
        {
            if (_buildingsAvailable != null)
            {
                for (int i = 0; i < _buildingsAvailable.Length; i++)
                {
                    if (_buildingsAvailable[i] == id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetStatus(bool status)
        {
            if (status)
            {
                _goldText.text = Player.instance.gold.ToString();
                _elixirText.text = Player.instance.elixir.ToString();
                _darkText.text = Player.instance.darkElixir.ToString();
                _gemsText.text = Player.instance.data.gems.ToString();
                _buildingsGrid.anchoredPosition = new Vector2(0, _buildingsGrid.anchoredPosition.y);

                int _workers = 0;
                int _busyWorkers = 0;
                if (Player.instance.data.buildings != null && Player.instance.data.buildings.Count > 0)
                {
                    for (int i = 0; i < Player.instance.data.buildings.Count; i++)
                    {
                        if (Player.instance.data.buildings[i].isConstructing)
                        {
                            _busyWorkers += 1;
                        }
                        if(Player.instance.data.buildings[i].id != Data.BuildingID.buildershut)
                        {
                            continue;
                        }
                        _workers += 1;
                    }
                }

                if (ui_buildings != null)
                {
                    for (int i = 0; i < ui_buildings.Count; i++)
                    {
                        ui_buildings[i].Initialize(_workers > _busyWorkers);
                    }
                }
            }
            _active = status;
            _elements.SetActive(status);
        }

        public bool PlaceBuilding(Data.BuildingID id, int x = -1, int y = -1)
        {
            var prefab = UI_Main.instance.GetBuildingPrefab(id);
            if (prefab.Item1 != null)
            {
                if(x < 0 || y < 0)
                {
                    Vector2Int point = UI_Main.instance._grid.GetBestBuildingPlace(prefab.Item2.rows, prefab.Item2.columns);
                    x = point.x;
                    y = point.y;
                }

                Vector3 position = Vector3.zero;

                Data.Building data = new Data.Building();
                data.id = id;
                data.x = x;
                data.y = y;
                data.level = 1;
                data.databaseID = 0;

                bool havrResources = false;

                int sbi = -1;

                for (int i = 0; i < Player.instance.initializationData.serverBuildings.Count; i++)
                {
                    if(Player.instance.initializationData.serverBuildings[i].id != id.ToString() || Player.instance.initializationData.serverBuildings[i].level != 1) { continue; }
                    data.columns = Player.instance.initializationData.serverBuildings[i].columns;
                    data.rows = Player.instance.initializationData.serverBuildings[i].rows;
                    data.buildTime = Player.instance.initializationData.serverBuildings[i].buildTime;
                    sbi = i;
                    // BIO-CLASH: Bypass legacy resource costs for fitness economy
                    // Buildings are free to place - only check gems if required
                    havrResources = Player.instance.data.gems >= Player.instance.initializationData.serverBuildings[i].requiredGems;
                    break;
                }

                if(!havrResources)
                {
                    return false;
                }

                UI_Shop.instance.SetStatus(false);
                UI_Main.instance.SetStatus(true);

                Building building = Instantiate(prefab.Item1, position, Quaternion.identity);
                building.rows = data.rows;
                building.columns = data.columns;

                building.serverIndex = sbi;

                data.radius = 0;
                building.data = data;
                building.databaseID = 0;
                building.PlacedOnGrid(x, y);
                if (building._baseArea)
                {
                    building._baseArea.gameObject.SetActive(true);
                }

                Building.buildinstance = building;
                CameraController.instance.isPlacingBuilding = true;
                UI_Build.instance.SetStatus(true);
                return true;
            }
            return false;
        }

        private void CloseShop()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            SetStatus(false);
            UI_Main.instance.SetStatus(true);
        }

    }
}