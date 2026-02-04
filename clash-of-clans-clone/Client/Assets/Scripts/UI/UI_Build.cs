namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DevelopersHub.RealtimeNetworking.Client;
    using System;

    public class UI_Build : MonoBehaviour
    {

        [SerializeField] public GameObject _elements = null;
        public RectTransform buttonConfirm = null;
        public RectTransform buttonCancel = null;
        [HideInInspector] public Button clickConfirmButton = null;
        [SerializeField] private float height = 0.06f;
        private Vector2 size = Vector2.one;

        private static UI_Build _instance = null; public static UI_Build instance { get { return _instance; } }

        private void Awake()
        {
            _instance = this;
            _elements.SetActive(false);
            clickConfirmButton = buttonConfirm.gameObject.GetComponent<Button>();
        }

        private void Start()
        {
            buttonConfirm.gameObject.GetComponent<Button>().onClick.AddListener(Confirm);
            buttonCancel.gameObject.GetComponent<Button>().onClick.AddListener(Cancel);
            buttonConfirm.anchorMin = Vector3.zero;
            buttonConfirm.anchorMax = Vector3.zero;
            buttonCancel.anchorMin = Vector3.zero;
            buttonCancel.anchorMax = Vector3.zero;
            size = new Vector2(Screen.height * height, Screen.height * height);
            buttonConfirm.sizeDelta = size * CameraController.instance.zoomScale;
            buttonCancel.sizeDelta = size * CameraController.instance.zoomScale;
        }

        private void Update()
        {
            if(Building.buildinstance != null && CameraController.instance.isPlacingBuilding)
            {
                buttonConfirm.sizeDelta = size / CameraController.instance.zoomScale;
                buttonCancel.sizeDelta = size / CameraController.instance.zoomScale;

                Vector3 end = UI_Main.instance._grid.GetEndPosition(Building.buildinstance);

                Vector3 planDownLeft = CameraController.instance.planDownLeft;
                Vector3 planTopRight = CameraController.instance.planTopRight;

                float w = planTopRight.x - planDownLeft.x;
                float h = planTopRight.y - planDownLeft.y;

                float endW = end.x - planDownLeft.x;
                float endH = end.y - planDownLeft.y;

                Vector2 screenPoint = new Vector2(endW / w * Screen.width, endH / h * Screen.height);

                Vector2 confirmPoint = screenPoint;
                confirmPoint.x += (buttonConfirm.rect.width + 10f);
                buttonConfirm.anchoredPosition = confirmPoint;

                Vector2 cancelPoint = screenPoint;
                cancelPoint.x -= (buttonCancel.rect.width + 10f);
                buttonCancel.anchoredPosition = cancelPoint;
            }
        }

        public void SetStatus(bool status)
        {
            _elements.SetActive(status);
        }

        [HideInInspector] public bool isBuildingWall = false;
        [HideInInspector] public int wallX = 0;
        [HideInInspector] public int wallY = 0;
        [HideInInspector] public List<Vector2Int> wallsBuilt = new List<Vector2Int>();

        private void Confirm()
        {
            //SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            if (Building.buildinstance != null/* && UI_Main.instance._grid.CanPlaceBuilding(Building.instance, Building.instance.currentX, Building.instance.currentY)*/)
            {
                if (!UI_WarLayout.instance.isActive)
                {
                    if (!CheckLimit())
                    {
                        Cancel();
                        return;
                    }

                    if (Building.buildinstance.serverIndex >= 0)
                    {
                        Player.instance.data.gems -= Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredGems;
                        Player.instance.elixir -= Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredElixir;
                        Player.instance.gold -= Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredGold;
                        Player.instance.darkElixir -= Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredDarkElixir;

                        Building.buildinstance.placeGemCost = Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredGems;
                        Building.buildinstance.placeElixirCost = Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredElixir;
                        Building.buildinstance.placeGoldCost = Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredGold;
                        Building.buildinstance.placeDarkElixirCost = Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredDarkElixir;
                    }
                }
                isBuildingWall = (Building.buildinstance.id == Data.BuildingID.wall);
                if (!isBuildingWall)
                {
                    wallsBuilt.Clear();
                }
                wallX = Building.buildinstance.currentX;
                wallY = Building.buildinstance.currentY;
                Packet packet = new Packet();
                packet.Write((int)Player.RequestsID.BUILD);
                packet.Write(SystemInfo.deviceUniqueIdentifier);
                packet.Write(Building.buildinstance.id.ToString());
                packet.Write(Building.buildinstance.currentX);
                packet.Write(Building.buildinstance.currentY);
                packet.Write(UI_WarLayout.instance.isActive ? 2 : 1);
                packet.Write(UI_WarLayout.instance.placingID);
                Building.buildinstance.lastChange = DateTime.Now;
                Sender.TCP_Send(packet);
                if (UI_WarLayout.instance.isActive && UI_WarLayout.instance.placingItem != null)
                {
                    Destroy(UI_WarLayout.instance.placingItem);
                    UI_WarLayout.instance.placingItem = null;
                }
                BuildConf();
                if (isBuildingWall)
                {
                    CheckeWall();
                }
                else
                {
                    SoundManager.instance.PlaySound(SoundManager.instance.buildStart);
                }
            }
        }

        private bool CheckLimit()
        {
            if (Building.buildinstance.serverIndex >= 0)
            {
                if(Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredGems > Player.instance.data.gems || Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredElixir > Player.instance.elixir || Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredDarkElixir > Player.instance.darkElixir || Player.instance.initializationData.serverBuildings[Building.buildinstance.serverIndex].requiredGold > Player.instance.gold)
                {
                    return false;
                }
            }
            int townHallLevel = 1;
            int count = 0;
            for (int i = 0; i < Player.instance.data.buildings.Count; i++)
            {
                if (Player.instance.data.buildings[i].id == Data.BuildingID.townhall)
                {
                    townHallLevel = Player.instance.data.buildings[i].level;
                }
                if (Player.instance.data.buildings[i].id == Building.buildinstance.id)
                {
                    count++;
                }
            }

            Data.BuildingCount limits = Data.GetBuildingLimits(townHallLevel, Building.buildinstance.id.ToString());
            if (limits != null)
            {
                if (count >= limits.count)
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckeWall()
        {
            int warLayoutIndex = -1;
                    bool haveMoreWall = false;
                    if (UI_WarLayout.instance.isActive)
                    {
                        if (UI_WarLayout.instance.buildingItems != null)
                        {
                            for (int i = 0; i < UI_WarLayout.instance.buildingItems.Count; i++)
                            {
                                if (UI_WarLayout.instance.buildingItems[i] != null && UI_WarLayout.instance.buildingItems[i].globalID == Data.BuildingID.wall)
                                {
                                    haveMoreWall = true;
                                    warLayoutIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        int townhallLevel = 1;
                        int haveCount = 0;
                        for (int i = 0; i < Player.instance.data.buildings.Count; i++)
                        {
                            if (Player.instance.data.buildings[i].id == Data.BuildingID.townhall) { townhallLevel = Player.instance.data.buildings[i].level; }
                            if (Player.instance.data.buildings[i].id != Data.BuildingID.wall) { continue; }
                            haveCount++;
                        }
                        Data.BuildingCount limit = Data.GetBuildingLimits(townhallLevel, Data.BuildingID.wall.ToString());
                        if (limit != null && haveCount < limit.count)
                        {
                            haveMoreWall = true;
                        }
                    }
                    if (haveMoreWall)
                    {
                        UI_Build.instance.wallsBuilt.Add(new Vector2Int(UI_Build.instance.wallX, UI_Build.instance.wallY));
                        bool handled = false;
                        if (UI_Build.instance.wallsBuilt.Count > 1)
                        {
                            int deltaX = UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x - UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 2].x;
                            int deltaY = UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y - UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 2].y;
                            if (Mathf.Abs(deltaX) == 1 && deltaY == 0)
                            {
                                if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x + deltaX, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y, 1, 1))
                                {
                                    handled = true;
                                    PlaceWall(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x + deltaX, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y, warLayoutIndex);
                                }
                                else
                                {
                                    if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y + 1, 1, 1))
                                    {
                                        handled = true;
                                        PlaceWall(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y + 1, warLayoutIndex);
                                    }
                                    else if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y - 1, 1, 1))
                                    {
                                        handled = true;
                                        PlaceWall(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y - 1, warLayoutIndex);
                                    }
                                }
                            }
                            else if (Mathf.Abs(deltaY) == 1 && deltaX == 0)
                            {
                                if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y + deltaY, 1, 1))
                                {
                                    handled = true;
                                    PlaceWall(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y + deltaY, warLayoutIndex);
                                }
                                else
                                {
                                    if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x + 1, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y, 1, 1))
                                    {
                                        handled = true;
                                        PlaceWall(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x + 1, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y, warLayoutIndex);
                                    }
                                    else if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x - 1, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y, 1, 1))
                                    {
                                        handled = true;
                                        PlaceWall(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x - 1, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y, warLayoutIndex);
                                    }
                                }
                            }
                        }
                        if (handled == false)
                        {
                            for (int i = 0; i < Player.instance.data.buildings.Count; i++)
                            {
                                if (Player.instance.data.buildings[i].id != Data.BuildingID.wall) { continue; }
                                Vector2Int pos = Building.GetBuildingPosition(Player.instance.data.buildings[i]);
                                int deltaX = pos.x - UI_Build.instance.wallX;
                                int deltaY = pos.y - UI_Build.instance.wallY;
                                if (Mathf.Abs(deltaX) == 1 && deltaY == 0)
                                {
                                    if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallX + deltaX, UI_Build.instance.wallY, Player.instance.data.buildings[i].rows, Player.instance.data.buildings[i].columns))
                                    {
                                        handled = true;
                                        PlaceWall(UI_Build.instance.wallX + deltaX, UI_Build.instance.wallY, warLayoutIndex);
                                        break;
                                    }
                                    else
                                    {
                                        if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallX, UI_Build.instance.wallY + 1, Player.instance.data.buildings[i].rows, Player.instance.data.buildings[i].columns))
                                        {
                                            handled = true;
                                            PlaceWall(UI_Build.instance.wallX, UI_Build.instance.wallY + 1, warLayoutIndex);
                                            break;
                                        }
                                        else if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallX, UI_Build.instance.wallY - 1, Player.instance.data.buildings[i].rows, Player.instance.data.buildings[i].columns))
                                        {
                                            handled = true;
                                            PlaceWall(UI_Build.instance.wallX, UI_Build.instance.wallY - 1, warLayoutIndex);
                                            break;
                                        }
                                    }
                                }
                                else if (Mathf.Abs(deltaY) == 1 && deltaX == 0)
                                {
                                    if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallX, UI_Build.instance.wallY + deltaY, Player.instance.data.buildings[i].rows, Player.instance.data.buildings[i].columns))
                                    {
                                        handled = true;
                                        PlaceWall(UI_Build.instance.wallX, UI_Build.instance.wallY + deltaY, warLayoutIndex);
                                        break;
                                    }
                                    else
                                    {
                                        if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallX + 1, UI_Build.instance.wallY, Player.instance.data.buildings[i].rows, Player.instance.data.buildings[i].columns))
                                        {
                                            handled = true;
                                            PlaceWall(UI_Build.instance.wallX + 1, UI_Build.instance.wallY, warLayoutIndex);
                                            break;
                                        }
                                        else if (UI_Main.instance._grid.CanPlaceBuilding(UI_Build.instance.wallX - 1, UI_Build.instance.wallY, Player.instance.data.buildings[i].rows, Player.instance.data.buildings[i].columns))
                                        {
                                            handled = true;
                                            PlaceWall(UI_Build.instance.wallX - 1, UI_Build.instance.wallY, warLayoutIndex);
                                            break;
                                        }
                                    }
                                }
                            }
                            if (handled == false)
                            {
                                PlaceWall(UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].x + 1, UI_Build.instance.wallsBuilt[UI_Build.instance.wallsBuilt.Count - 1].y, warLayoutIndex);
                            }
                        }
                    }
        }

        private void PlaceWall(int x, int y, int warIndex)
        {
            if (warIndex >= 0)
            {
                UI_WarLayout.instance.buildingItems[warIndex].PlaceWall(x, y);
            }
            else
            {
                UI_Shop.instance.PlaceBuilding(Data.BuildingID.wall, x, y);
            }
        }

        public void BuildConf()
        {
            if (Building.buildinstance != null)
            {
                CameraController.instance.isPlacingBuilding = false;
                Building.buildinstance.BuildForFirstTimeStarted();
                if (UI_WarLayout.instance.isActive && UI_WarLayout.instance.placingItem != null)
                {
                    UI_WarLayout.instance.placingItem.SetActive(true);
                    UI_WarLayout.instance.placingItem = null;
                }
            }
        }

        public void Cancel()
        {
            if (Building.buildinstance != null)
            {
                CameraController.instance.isPlacingBuilding = false;
                Building.buildinstance.RemovedFromGrid();
                if (UI_WarLayout.instance.isActive && UI_WarLayout.instance.placingItem != null)
                {
                    UI_WarLayout.instance.placingItem.SetActive(true);
                    UI_WarLayout.instance.placingItem = null;
                }
            }
        }

    }
}