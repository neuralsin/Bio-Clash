namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_WarLayoutBuilding : MonoBehaviour
    {

        [SerializeField] private Button _button = null;
        [SerializeField] private Image _icon = null;
        [SerializeField] private TextMeshProUGUI _name = null;
        private long _id = 0;
        private Data.BuildingID _globalID = Data.BuildingID.townhall; public Data.BuildingID globalID { get { return _globalID; } }

        private void Start()
        {
            _button.onClick.AddListener(Clicked);
        }
        
        public void Initialized(Data.Building building)
        {
            _globalID = building.id;
            _id = building.databaseID;
            _name.text = Language.instance.GetBuildingName(building.id);
            Sprite icon = AssetsBank.GetBuildingIcon(building.id);
            if (icon != null)
            {
                _icon.sprite = icon;
            }
        }

        private int preferedX = -1;
        private int preferedY = -1;

        public void PlaceWall(int x, int y)
        {
            preferedX = x;
            preferedY = y;
            Clicked();
        }

        private void Clicked()
        {
            if (UI_WarLayout.instance.placingItem != null)
            {
                UI_WarLayout.instance.placingItem.SetActive(true);
                UI_WarLayout.instance.placingItem = null;
                UI_Build.instance.Cancel();
            }
            int n = -1;
            for (int i = 0; i < Player.instance.data.buildings.Count; i++)
            {
                if(Player.instance.data.buildings[i].databaseID == _id)
                {
                    n = i;
                    break;
                }
            }
            if(n >= 0)
            {
                var prefab = UI_Main.instance.GetBuildingPrefab(Player.instance.data.buildings[n].id);
                if (prefab.Item1 != null)
                {
                    UI_WarLayout.instance.placingID = Player.instance.data.buildings[n].databaseID;
                    Vector3 position = Vector3.zero;
                    Building building = Instantiate(prefab.Item1, position, Quaternion.identity);
                    building.data = Player.instance.data.buildings[n];
                    building.rows = prefab.Item2.rows;
                    building.columns = prefab.Item2.columns;
                    building.databaseID = 0;
                    if (preferedX < 0 || preferedY < 0)
                    {
                        Vector2Int point = UI_Main.instance._grid.GetBestBuildingPlace(prefab.Item2.rows, prefab.Item2.columns);
                        preferedX = point.x;
                        preferedY = point.y;
                    }
                    building.PlacedOnGrid(preferedX, preferedY);
                    if (building._baseArea)
                    {
                        building._baseArea.gameObject.SetActive(true);
                    }
                    Building.buildinstance = building;
                    CameraController.instance.isPlacingBuilding = true;
                    UI_WarLayout.instance.placingItem = gameObject;
                    UI_WarLayout.instance.placingItem.SetActive(false);
                    UI_Build.instance.SetStatus(true);
                }
            }
        }

    }
}