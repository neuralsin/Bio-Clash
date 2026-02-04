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
            
            // Bio-Clash: Initialize n8n Flowchart UI
            InitializeN8nFlowchart();
        }

        // ============================================================
        // BIO-CLASH N8N FLOWCHART UI
        // ============================================================
        
        private void InitializeN8nFlowchart()
        {
            // ═══════════════════════════════════════════════════════════════
            // MODERN N8N-STYLE 1:1 MAPPING FLOWCHART
            // Each muscle group maps to exactly one defense building
            // ═══════════════════════════════════════════════════════════════

            // 1:1 Mappings with unique accent colors (HSL-inspired palette)
            var mappings = new List<(string muscle, string building, Color accent)>
            {
                ("CHEST",     "Archer Tower",  new Color(1.0f, 0.42f, 0.42f)),  // Coral Red
                ("BACK",      "Cannon",        new Color(0.4f, 0.6f, 1.0f)),    // Sky Blue
                ("SHOULDERS", "Wizard Tower",  new Color(1.0f, 0.84f, 0.0f)),   // Gold
                ("BICEPS",    "Hidden Tesla",  new Color(0.87f, 0.63f, 0.87f)), // Plum
                ("TRICEPS",   "Mortar",        new Color(0.69f, 0.88f, 0.9f)),  // Powder Blue
                ("LEGS",      "Inferno Tower", new Color(1.0f, 0.55f, 0.0f)),   // Orange
                ("CORE",      "Walls",         new Color(0.6f, 0.98f, 0.6f)),   // Pale Green
                ("CARDIO",    "X-Bow",         new Color(0.0f, 1.0f, 1.0f))     // Cyan
            };

            // Sort by combined text length for clean visual flow
            mappings.Sort((a, b) => (a.muscle.Length + a.building.Length)
                                    .CompareTo(b.muscle.Length + b.building.Length));

            // Container - positioned below currency bar, non-overlapping
            GameObject container = new GameObject("N8nFlowchartContainer");
            container.transform.SetParent(_elements.transform, false);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.02f, 0.76f);
            containerRect.anchorMax = new Vector2(0.98f, 0.86f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Glassmorphism background
            Image containerBg = container.AddComponent<Image>();
            containerBg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f); // Near-black with transparency

            // Subtle border glow
            Outline containerOutline = container.AddComponent<Outline>();
            containerOutline.effectColor = new Color(0.4f, 0.8f, 1.0f, 0.3f); // Soft cyan glow
            containerOutline.effectDistance = new Vector2(1, -1);

            // Horizontal layout with spacing
            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 6, 6);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;

            // Create each node
            foreach (var mapping in mappings)
            {
                CreateFlowNode(container.transform, mapping.muscle, mapping.building, mapping.accent);
            }
        }

        private void CreateFlowNode(Transform parent, string muscle, string building, Color accentColor)
        {
            // Node container
            GameObject nodeObj = new GameObject($"Node_{muscle}");
            nodeObj.transform.SetParent(parent, false);

            // Modern card-style background
            Image nodeImg = nodeObj.AddComponent<Image>();
            nodeImg.color = new Color(0.12f, 0.14f, 0.18f, 0.95f); // Dark charcoal

            // Accent border (unique per muscle)
            Outline accent = nodeObj.AddComponent<Outline>();
            accent.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.7f);
            accent.effectDistance = new Vector2(2, -2);

            // Inner padding
            HorizontalLayoutGroup hlg = nodeObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.spacing = 4;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            // Text with modern styling
            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(nodeObj.transform, false);

            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            
            // Format: MUSCLE → Building (1:1 mapping, clear and readable)
            string hexColor = ColorUtility.ToHtmlStringRGB(accentColor);
            tmp.text = $"<color=#{hexColor}><b>{muscle}</b></color> <color=#888888>→</color> <color=#ffffff>{building}</color>";
            tmp.fontSize = 12;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.richText = true;
            tmp.enableWordWrapping = false;
            tmp.fontStyle = FontStyles.Normal;
            tmp.characterSpacing = 0.5f; // Slightly increased letter spacing for modern feel
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