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
            
            // Bio-Clash: Initialize Codex UI
            InitializeCodex();
        }

        // ============================================================
        // BIO-CLASH CODEX UI
        // ============================================================
        
        private GameObject _codexPanel;
        private bool _isCodexOpen = false;

        private void InitializeCodex()
        {
            // Create CODEX button
            GameObject codexBtnObj = new GameObject("BtnCodex");
            codexBtnObj.transform.SetParent(_elements.transform, false);
            
            RectTransform btnRect = codexBtnObj.AddComponent<RectTransform>();
            // Position top-left (near currency) or top-right? Let's put it top-center-ish
            btnRect.anchorMin = new Vector2(0.8f, 0.9f);
            btnRect.anchorMax = new Vector2(0.95f, 0.98f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            
            Image btnImg = codexBtnObj.AddComponent<Image>();
            btnImg.color = new Color(0.4f, 0.2f, 0.6f, 1f); // Purple
            
            Button btn = codexBtnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(ToggleCodex);
            
            // Button Text
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(codexBtnObj.transform, false);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "📖 CODEX";
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold; // Make sure FontStyles is available or use numeric

            // Create Codex Panel (Hidden by default)
            _codexPanel = new GameObject("CodexPanel");
            _codexPanel.transform.SetParent(_elements.transform, false);
            _codexPanel.SetActive(false);
            
            RectTransform panelRect = _codexPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImg = _codexPanel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f); // Dark background
            
            // Add Border
            Outline outline = _codexPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.4f, 0.8f, 1f, 0.5f);
            outline.effectDistance = new Vector2(2, 2);

            // Title
            CreateText(_codexPanel.transform, "Title", "🧬 BIO-CODEX: MUSCLE MAPPING", 28, 
                new Vector2(0, 0.9f), new Vector2(1, 1), TextAlignmentOptions.Center, Color.cyan);

            // Close Codex Button
            GameObject closeBtn = new GameObject("CloseCodex");
            closeBtn.transform.SetParent(_codexPanel.transform, false);
            RectTransform closeRect = closeBtn.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.92f, 0.92f);
            closeRect.anchorMax = new Vector2(0.98f, 0.98f);
            
            Image closeImg = closeBtn.AddComponent<Image>();
            closeImg.color = Color.red;
            
            Button cBtn = closeBtn.AddComponent<Button>();
            cBtn.onClick.AddListener(ToggleCodex);
            
            GameObject closeTxt = new GameObject("Text");
            closeTxt.transform.SetParent(closeBtn.transform, false);
            RectTransform ctRect = closeTxt.AddComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            
            TextMeshProUGUI cTmp = closeTxt.AddComponent<TextMeshProUGUI>();
            cTmp.text = "X";
            cTmp.alignment = TextAlignmentOptions.Center;
            cTmp.fontSize = 20;

            // Content
            string content = 
                "<color=#ff9999><b>CHEST</b></color>  ➔  <color=#ffffff>Archer Tower</color>\n" +
                "<color=#9999ff><b>BACK</b></color>   ➔  <color=#ffffff>Cannon</color>\n" +
                "<color=#ffff99><b>SHOULDERS</b></color> ➔ <color=#ffffff>Wizard Tower / Air Def</color>\n" +
                "<color=#ff99ff><b>BICEPS</b></color> ➔  <color=#ffffff>Hidden Tesla / Bomb Tower</color>\n" +
                "<color=#cc99ff><b>TRICEPS</b></color> ➔ <color=#ffffff>Mortar</color>\n" +
                "<color=#99ff99><b>LEGS</b></color>   ➔  <color=#ffffff>Inferno Tower</color>\n" +
                "<color=#ffffcc><b>CORE</b></color>   ➔  <color=#ffffff>Walls</color>\n" +
                "<color=#99ffff><b>CARDIO</b></color> ➔  <color=#ffffff>X-Bow</color>\n\n" +
                "<i>Train specific muscles to unlock upgrades for these buildings!</i>";

            CreateText(_codexPanel.transform, "Content", content, 24,
                new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.85f), TextAlignmentOptions.TopLeft, Color.white);
        }

        private void ToggleCodex()
        {
            _isCodexOpen = !_isCodexOpen;
            if (_codexPanel != null)
                _codexPanel.SetActive(_isCodexOpen);
            
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, 
            Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions align, Color color)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            RectTransform rect = textGO.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.richText = true; // Enable rich text for colors
            
            return textGO;
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