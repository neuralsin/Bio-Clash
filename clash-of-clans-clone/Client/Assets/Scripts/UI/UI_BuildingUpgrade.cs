namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DevelopersHub.RealtimeNetworking.Client;
    using System;

    /// <summary>
    /// BIO-CLASH UPGRADE PANEL
    /// Replaces resource-based upgrades with fitness volume requirements.
    /// "Your Body Builds Your Base"
    /// </summary>
    public class UI_BuildingUpgrade : MonoBehaviour
    {

        [SerializeField] public GameObject _elements = null;
        private static UI_BuildingUpgrade _instance = null; public static UI_BuildingUpgrade instance { get { return _instance; } }

        [SerializeField] private Button _closeButton = null;
        [SerializeField] private GameObject _maxLevelPanel = null;
        [SerializeField] private GameObject _detailsPanel = null;
        [SerializeField] private TextMeshProUGUI _titleLevel = null;
        [SerializeField] private TextMeshProUGUI _titleBuilding = null;
        [SerializeField] private Image _icon = null;
        [SerializeField] private Sprite _defaultIcon = null;
        
        // Original resource fields (hidden but kept for compatibility)
        [SerializeField] private TextMeshProUGUI reqGold = null;
        [SerializeField] private TextMeshProUGUI reqElixir = null;
        [SerializeField] private TextMeshProUGUI reqDark = null;
        [SerializeField] private TextMeshProUGUI reqGems = null;
        [SerializeField] private TextMeshProUGUI reqTime = null;
        
        // BIO-CLASH: Fitness requirement fields
        [Header("Bio-Clash Fitness Requirements")]
        [SerializeField] private TextMeshProUGUI reqMuscle = null; // "Chest", "Back", etc
        [SerializeField] private TextMeshProUGUI reqVolume = null; // "500 / 1000 kg"
        [SerializeField] private Image reqVolumeBar = null; // Progress bar
        [SerializeField] private TextMeshProUGUI reqStreak = null; // For Town Hall
        [SerializeField] private GameObject fitnessRequirementsPanel = null;
        
        [SerializeField] private Button _upgradeButton = null;
        [SerializeField] private GameObject _requiredBuildingPanel = null;
        [SerializeField] private GameObject _townHallRequiredPanel = null;
        [SerializeField] private TextMeshProUGUI _townHallRequiredText = null;
        [SerializeField] private UI_RequiredBuilding _requiredBuildingPrefab = null;
        [SerializeField] private RectTransform _requiredBuildingGrid = null;
        [SerializeField] private RectTransform _requiredBuildingRoot = null;

        private List<UI_RequiredBuilding> _buildings = new List<UI_RequiredBuilding>();
        private bool _active = false; public bool isActive { get { return _active; } }
        private Data.ServerBuilding serverBuilding = null;
        private Building selectedinstance = null;

        private void Awake()
        {
            _instance = this;
            _elements.SetActive(false);
        }

        private long id = 0;

        private void Start()
        {
            _closeButton.onClick.AddListener(Close);
            _upgradeButton.onClick.AddListener(Upgrade);
        }

        public void Open()
        {
            serverBuilding = null;
            ClearBuildings();
            _upgradeButton.interactable = true;
            _requiredBuildingPanel.SetActive(false);
            _townHallRequiredPanel.SetActive(false);
            id = Building.selectedinstance.data.databaseID;
            selectedinstance = Building.selectedinstance;
            int x = -1;
            int townHallLevel = 0;
            
            for (int i = 0; i < Player.instance.data.buildings.Count; i++)
            {
                if (Player.instance.data.buildings[i].databaseID == id) { x = i; }
                if (Player.instance.data.buildings[i].id == Data.BuildingID.townhall) { townHallLevel = Player.instance.data.buildings[i].level; }
                if (townHallLevel > 0 && x >= 0) { break; }
            }
            
            if (x >= 0)
            {
                _titleBuilding.text = Language.instance.GetBuildingName(Player.instance.data.buildings[x].id);
                Sprite icon = AssetsBank.GetBuildingIcon(Player.instance.data.buildings[x].id, Player.instance.data.buildings[x].level);
                if (icon != null)
                {
                    _icon.sprite = icon;
                }
                else
                {
                    _icon.sprite = _defaultIcon;
                }
                
                serverBuilding = Player.instance.GetServerBuilding(Player.instance.data.buildings[x].id, Player.instance.data.buildings[x].level + 1);
                
                if (serverBuilding != null && !(Player.instance.data.buildings[x].id == Data.BuildingID.townhall && Player.instance.data.buildings[x].level >= Data.maxTownHallLevel))
                {
                    // =========================================================
                    // BIO-CLASH: FITNESS REQUIREMENT CHECK
                    // =========================================================
                    bool meetsFitnessRequirement = CheckFitnessRequirement(selectedinstance);
                    
                    // Display fitness requirements
                    DisplayFitnessRequirements(selectedinstance);
                    
                    // Check builder availability (still needed)
                    if (UI_Main.instance.haveAvailableBuilder == false)
                    {
                        _upgradeButton.interactable = false;
                        _townHallRequiredPanel.SetActive(true);
                        _townHallRequiredText.text = "You do not have available worker";
                    }
                    else if (!meetsFitnessRequirement)
                    {
                        _upgradeButton.interactable = false;
                        _townHallRequiredPanel.SetActive(true);
                        _townHallRequiredText.text = "?? Workout more to unlock!";
                    }

                    // Hide original resource requirements, show fitness
                    if (reqGold != null) reqGold.transform.parent.gameObject.SetActive(false);
                    if (reqElixir != null) reqElixir.transform.parent.gameObject.SetActive(false);
                    if (reqDark != null) reqDark.transform.parent.gameObject.SetActive(false);
                    if (reqGems != null) reqGems.transform.parent.gameObject.SetActive(false);
                    
                    // Build time based on recovery score (faster if recovered)
                    int baseBuildTime = serverBuilding.buildTime;
                    if (FitnessManager.instance != null)
                    {
                        float recoveryMultiplier = 1f - (FitnessManager.instance.recoveryScore / 200f); // 100% recovery = 50% faster
                        baseBuildTime = Mathf.Max(10, (int)(baseBuildTime * recoveryMultiplier));
                    }
                    if (reqTime != null) reqTime.text = Tools.SecondsToTimeFormat(baseBuildTime);
                    
                    _titleLevel.text = "Upgrade to Level " + (Player.instance.data.buildings[x].level + 1).ToString();
                    _maxLevelPanel.SetActive(false);
                    _detailsPanel.SetActive(true);
                }
                else
                {
                    // Building is at max level
                    _maxLevelPanel.SetActive(true);
                    _detailsPanel.SetActive(false);
                }
                
                _active = true;
                _elements.SetActive(true);
                _titleLevel.ForceMeshUpdate(true);
                _titleBuilding.ForceMeshUpdate(true);
            }
        }

        /// <summary>
        /// Check if player meets fitness requirements for this building upgrade.
        /// </summary>
        private bool CheckFitnessRequirement(Building building)
        {
            if (FitnessManager.instance == null)
            {
                Debug.LogWarning("FitnessManager not found - allowing upgrade for demo");
                return true;
            }
            
            return building.CanUpgradeWithFitness();
        }

        /// <summary>
        /// Display fitness requirements in the upgrade panel.
        /// </summary>
        private void DisplayFitnessRequirements(Building building)
        {
            if (FitnessManager.instance == null) return;
            
            // Auto-create UI if missing (SAFE MODE)
            if (reqMuscle == null || reqVolume == null)
            {
                CreateFitnessRequirementUI();
            }

            // Get muscle group for this building
            string muscleGroup = building.GetMuscleGroupName();
            float requiredVolume = building.GetFitnessRequirement();
            float currentVolume = building.GetCurrentFitnessVolume();
            
            // Handle Town Hall (uses streak)
            if (building.id == Data.BuildingID.townhall)
            {
                int requiredDays = (building.data.level + 1) * 7;
                int currentStreak = FitnessManager.instance.workoutStreak;
                
                if (reqMuscle != null) reqMuscle.text = "?? Consistency";
                if (reqVolume != null) reqVolume.text = $"{currentStreak} / {requiredDays} days";
                if (reqVolumeBar != null) reqVolumeBar.fillAmount = Mathf.Clamp01((float)currentStreak / requiredDays);
                if (reqStreak != null) reqStreak.text = $"Maintain a {requiredDays}-day workout streak!";
            }
            else
            {
                // Regular buildings use muscle volume
                string muscleEmoji = GetMuscleEmoji(muscleGroup);
                
                if (reqMuscle != null) reqMuscle.text = $"{muscleEmoji} {muscleGroup}";
                if (reqVolume != null) reqVolume.text = $"{currentVolume:N0} / {requiredVolume:N0} kg";
                if (reqVolumeBar != null) reqVolumeBar.fillAmount = requiredVolume > 0 ? Mathf.Clamp01(currentVolume / requiredVolume) : 1f;
                if (reqStreak != null) reqStreak.text = $"Lift {muscleGroup.ToLower()} to unlock!";
            }
            
            // Show fitness panel if available
            if (fitnessRequirementsPanel != null)
            {
                fitnessRequirementsPanel.SetActive(true);
            }
        }

        private void CreateFitnessRequirementUI()
        {
            if (_detailsPanel == null) return;

            Debug.Log("??? Auto-creating Fitness Requirement UI elements");

            // Create container
            fitnessRequirementsPanel = new GameObject("FitnessRequirements");
            fitnessRequirementsPanel.transform.SetParent(_detailsPanel.transform, false);
            RectTransform rt = fitnessRequirementsPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0.2f);
            rt.anchorMax = new Vector2(0.95f, 0.45f);
            
            // Background
            Image bg = fitnessRequirementsPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.15f, 0.2f, 0.8f);

            // Muscle Text
            GameObject muscleGO = new GameObject("ReqMuscle");
            muscleGO.transform.SetParent(fitnessRequirementsPanel.transform, false);
            reqMuscle = muscleGO.AddComponent<TextMeshProUGUI>();
            reqMuscle.fontSize = 20;
            reqMuscle.alignment = TextAlignmentOptions.TopLeft;
            RectTransform muscleRect = muscleGO.GetComponent<RectTransform>();
            muscleRect.anchorMin = new Vector2(0.05f, 0.6f);
            muscleRect.anchorMax = new Vector2(0.6f, 0.9f);

            // Volume Text
            GameObject volumeGO = new GameObject("ReqVolume");
            volumeGO.transform.SetParent(fitnessRequirementsPanel.transform, false);
            reqVolume = volumeGO.AddComponent<TextMeshProUGUI>();
            reqVolume.fontSize = 20;
            reqVolume.alignment = TextAlignmentOptions.TopRight;
            RectTransform volumeRect = volumeGO.GetComponent<RectTransform>();
            volumeRect.anchorMin = new Vector2(0.6f, 0.6f);
            volumeRect.anchorMax = new Vector2(0.95f, 0.9f);

            // Bar Background
            GameObject barBG = new GameObject("BarBG");
            barBG.transform.SetParent(fitnessRequirementsPanel.transform, false);
            Image barBGImg = barBG.AddComponent<Image>();
            barBGImg.color = new Color(0, 0, 0, 0.5f);
            RectTransform barBGRect = barBG.GetComponent<RectTransform>();
            barBGRect.anchorMin = new Vector2(0.05f, 0.3f);
            barBGRect.anchorMax = new Vector2(0.95f, 0.5f);

            // Bar Fill
            GameObject barFill = new GameObject("BarFill");
            barFill.transform.SetParent(barBG.transform, false);
            reqVolumeBar = barFill.AddComponent<Image>();
            reqVolumeBar.color = new Color(0.2f, 0.8f, 0.4f, 1f);
            RectTransform barFillRect = barFill.GetComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = Vector2.one;
            barFillRect.offsetMin = Vector2.zero;
            barFillRect.offsetMax = Vector2.zero;

            // Streak/Info Text
            GameObject streakGO = new GameObject("ReqStreak");
            streakGO.transform.SetParent(fitnessRequirementsPanel.transform, false);
            reqStreak = streakGO.AddComponent<TextMeshProUGUI>();
            reqStreak.fontSize = 14;
            reqStreak.alignment = TextAlignmentOptions.Center;
            RectTransform streakRect = streakGO.GetComponent<RectTransform>();
            streakRect.anchorMin = new Vector2(0.05f, 0.05f);
            streakRect.anchorMax = new Vector2(0.95f, 0.25f);
        }

        /// <summary>
        /// Get emoji for muscle group display.
        /// </summary>
        private string GetMuscleEmoji(string muscle)
        {
            switch (muscle.ToLower())
            {
                case "chest": return "??";
                case "back": return "??";
                case "legs": return "??";
                case "shoulders": return "??";
                case "biceps": return "??";
                case "triceps": return "???";
                case "core": return "??";
                case "cardio": return "??";
                default: return "??";
            }
        }

        public void Close()
        {
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            _active = false;
            ClearBuildings();
            _elements.SetActive(false);
        }

        private void Close2()
        {
            _active = false;
            ClearBuildings();
            _elements.SetActive(false);
        }

        private void Upgrade()
        {
            if (serverBuilding != null)
            {
                // BIO-CLASH: Final fitness check before upgrade
                if (!CheckFitnessRequirement(selectedinstance))
                {
                    _townHallRequiredPanel.SetActive(true);
                    _townHallRequiredText.text = "?? Need more workout volume!";
                    return;
                }
                
                if (serverBuilding.buildTime > 0)
                {
                    // Apply recovery bonus to build time
                    int adjustedBuildTime = serverBuilding.buildTime;
                    if (FitnessManager.instance != null)
                    {
                        float recoveryMultiplier = 1f - (FitnessManager.instance.recoveryScore / 200f);
                        adjustedBuildTime = Mathf.Max(10, (int)(serverBuilding.buildTime * recoveryMultiplier));
                    }
                    
                    selectedinstance.isCons = true;
                    selectedinstance.data.constructionTime = Player.instance.data.nowTime.AddSeconds(adjustedBuildTime);
                    selectedinstance.data.buildTime = adjustedBuildTime;
                }
                else
                {
                    selectedinstance.isCons = false;
                    selectedinstance.level = selectedinstance.level + 1;
                    selectedinstance.AdjustUI(true);
                }
                
                selectedinstance.lastChange = DateTime.Now;
                
                Packet packet = new Packet();
                packet.Write((int)Player.RequestsID.UPGRADE);
                packet.Write(id);
                Sender.TCP_Send(packet);
                
                Close2();
                SoundManager.instance.PlaySound(SoundManager.instance.buildStart);
            }
            else
            {
                Close();
            }
        }

        public void ClearBuildings()
        {
            for (int i = 0; i < _buildings.Count; i++)
            {
                if (_buildings[i] != null)
                {
                    Destroy(_buildings[i].gameObject);
                }
            }
            _buildings.Clear();
        }

    }
}