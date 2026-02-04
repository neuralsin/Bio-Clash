namespace DevelopersHub.ClashOfWhatecer
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections;

    /// <summary>
    /// Auto-setup script for Bio-Clash Fitness system.
    /// This script auto-initializes at runtime - no manual setup required!
    /// </summary>
    public class FitnessSetup : MonoBehaviour
    {
        [Header("Auto-Create These Components")]
        public bool createFitnessManager = true;
        public bool createFitnessUI = true;

        [Header("References (Auto-Found if Empty)")]
        public Canvas mainCanvas;
        public UI_Main uiMain;

        /// <summary>
        /// RUNTIME AUTO-INIT: Automatically creates FitnessSetup on game start.
        /// No need to manually add to scene!
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoInitialize()
        {
            // Only create if not already present
            if (FindObjectOfType<FitnessSetup>() == null && FindObjectOfType<FitnessManager>() == null)
            {
                GameObject setupGO = new GameObject("FitnessSetup_Auto");
                setupGO.AddComponent<FitnessSetup>();
                Debug.Log("[BIO-CLASH] Fitness system auto-initialized!");
            }
        }

        private void Awake()
        {
            // ============================================================
            // AGGRESSIVE CLEANUP: Destroy ALL existing UI_Fitness objects
            // This ensures the old "ugly" UI is removed before we create new one
            // ============================================================
            UI_Fitness[] existingUIs = FindObjectsOfType<UI_Fitness>(true); // include inactive
            foreach (var oldUI in existingUIs)
            {
                Debug.Log($"[BIO-CLASH] Destroying old UI_Fitness: {oldUI.gameObject.name}");
                DestroyImmediate(oldUI.gameObject);
            }
            _cachedFitnessUI = null;
            
            // Create FitnessManager singleton if needed
            if (createFitnessManager && FitnessManager.instance == null)
            {
                GameObject fitnessManagerGO = new GameObject("FitnessManager");
                fitnessManagerGO.AddComponent<FitnessManager>();
                DontDestroyOnLoad(fitnessManagerGO);
                Debug.Log("[BIO-CLASH] FitnessManager created");
            }

            // Start coroutine to wait for UI_Main
            StartCoroutine(InitializeUI());
        }

        private IEnumerator InitializeUI()
        {
            // Wait for UI_Main to be available
            float timeout = 5f;
            while (UI_Main.instance == null && timeout > 0)
            {
                timeout -= 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            if (UI_Main.instance == null)
            {
                Debug.LogWarning("[BIO-CLASH] UI_Main not found after timeout");
                yield break;
            }

            // Cache references
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
            }

            if (uiMain == null)
            {
                uiMain = UI_Main.instance;
            }

            // Create Fitness Button in UI_Main if not assigned
            CreateFitnessButton();

            // Create Fitness UI panel if needed
            if (createFitnessUI && mainCanvas != null)
            {
                CreateFitnessUIPanel();
            }

            Debug.Log("[BIO-CLASH] Fitness UI initialized!");
        }

        /// <summary>
        /// Creates the fitness button in UI_Main's bottom bar area.
        /// </summary>
        private void CreateFitnessButton()
        {
            if (uiMain == null || uiMain._elements == null) return;

            // Check if button already exists via reflection
            var fitnessButtonField = typeof(UI_Main).GetField("_fitnessButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (fitnessButtonField == null)
            {
                Debug.LogWarning("[BIO-CLASH] _fitnessButton field not found in UI_Main");
                return;
            }

            Button existingButton = fitnessButtonField.GetValue(uiMain) as Button;
            if (existingButton != null) 
            {
                Debug.Log("[BIO-CLASH] Fitness button already assigned");
                return;
            }

            // Create button in UI_Main._elements (bottom bar area)
            GameObject btnGO = new GameObject("FitnessButton");
            btnGO.transform.SetParent(uiMain._elements.transform, false);
            
            RectTransform btnRect = btnGO.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.02f, 0.02f);
            btnRect.anchorMax = new Vector2(0.15f, 0.12f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green fitness color

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            
            // Add text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "FITNESS";
            text.fontSize = 18;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            // Wire button to field
            fitnessButtonField.SetValue(uiMain, btn);
            
            // Add click listener
            btn.onClick.AddListener(() => {
                if (UI_Fitness.instance != null)
                {
                    SoundManager.instance?.PlaySound(SoundManager.instance.buttonClickSound);
                    UI_Fitness.instance.Open();
                }
            });

            Debug.Log("[BIO-CLASH] Fitness button created and wired!");
        }

        // Cached reference for UI_Fitness to avoid repeated FindObjectOfType calls
        private static UI_Fitness _cachedFitnessUI = null;

        /// <summary>
        /// Creates the Fitness UI panel programmatically with all components.
        /// </summary>
        private void CreateFitnessUIPanel()
        {
            // Check if UI_Fitness already exists using cached reference
            if (_cachedFitnessUI == null)
            {
                _cachedFitnessUI = FindObjectOfType<UI_Fitness>();
            }
            
            if (_cachedFitnessUI != null)
            {
                Debug.Log("[BIO-CLASH] Deleting old/ugly UI_Fitness to rebuild new one...");
                
                // If the panel is just disabled, we might want to keep it? 
                // No, the user wants a full rebuild because it's "ugly".
                // We must destroy the GAME OBJECT, not just the component.
                if (_cachedFitnessUI.gameObject != null)
                {
                   GameObject.DestroyImmediate(_cachedFitnessUI.gameObject);
                }
                _cachedFitnessUI = null;
            }

            // =================================================================================
            // 1. MAIN PANEL SETUP
            // =================================================================================
            GameObject fitnessPanel = new GameObject("UI_Fitness");
            fitnessPanel.transform.SetParent(mainCanvas.transform, false);
            
            RectTransform panelRect = fitnessPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Add UI_Fitness component
            UI_Fitness fitnessUI = fitnessPanel.AddComponent<UI_Fitness>();
            fitnessUI._panel = fitnessPanel;

            // Blur/Dim Background
            GameObject dimmer = CreatePanel(fitnessPanel.transform, "Dimmer", new Color(0, 0, 0, 0.7f));
            
            // Main Window Background (Clash Style)
            GameObject bgPanel = CreatePanel(fitnessPanel.transform, "Background", new Color(0.13f, 0.16f, 0.22f, 1f));
            RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.1f, 0.1f);
            bgRect.anchorMax = new Vector2(0.9f, 0.9f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // Add Border/Outline to Main Window
            Outline winOutline = bgPanel.AddComponent<Outline>();
            winOutline.effectColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            winOutline.effectDistance = new Vector2(2, -2);

            // =================================================================================
            // 2. HEADER
            // =================================================================================
            GameObject headerObj = CreatePanel(bgPanel.transform, "Header", new Color(0.2f, 0.25f, 0.3f, 1f));
            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 0.9f);
            headerRect.anchorMax = new Vector2(1, 1);
            
            // Title
            GameObject titleGO = CreateText(headerObj.transform, "Title", "FITNESS CENTER", 36, TextAlignmentOptions.Center);
            TextMeshProUGUI titleText = titleGO.GetComponent<TextMeshProUGUI>();
            titleText.color = new Color(1f, 0.95f, 0.6f, 1f); // Gold
            titleText.fontStyle = FontStyles.Bold;

            // Close Button (Top Right)
            GameObject closeBtn = CreateButton(headerObj.transform, "CloseButton", "X", 24, new Vector2(0.94f, 0.1f), new Vector2(0.99f, 0.9f));
            closeBtn.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.2f, 1f); // Red
            fitnessUI._closeButton = closeBtn.GetComponent<Button>();

            // =================================================================================
            // 3. LAYOUT DIVISIONS
            // =================================================================================
            
            // Left Column: Stats (0 - 0.4)
            GameObject statsCol = CreatePanel(bgPanel.transform, "StatsColumn", Color.clear);
            RectTransform statsRect = statsCol.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.02f, 0.02f);
            statsRect.anchorMax = new Vector2(0.40f, 0.88f);

            // Right Column: Controls & Health (0.42 - 0.98)
            GameObject mainCol = CreatePanel(bgPanel.transform, "MainColumn", Color.clear);
            RectTransform mainRect = mainCol.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.42f, 0.02f);
            mainRect.anchorMax = new Vector2(0.98f, 0.88f);

            // =================================================================================
            // 4. STATS SECTION (LEFT COLUMN)
            // =================================================================================
            CreateText(statsCol.transform, "StatsTitle", "MUSCLE LEVELS", 24, new Vector2(0, 0.94f), new Vector2(1, 1f), TextAlignmentOptions.Left)
                .GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 1f);

            string[] muscles = { "Chest", "Back", "Shoulders", "Biceps", "Triceps", "Legs", "Core", "Cardio" };
            float barHeight = 0.08f;
            float gap = 0.03f;
            float startY = 0.88f;

            for (int i = 0; i < muscles.Length; i++)
            {
                float yMax = startY - (i * (barHeight + gap));
                float yMin = yMax - barHeight;
                
                GameObject barObj = CreateMuscleBar(statsCol.transform, muscles[i], yMin, yMax);
                
                // Assign references
                var textComp = barObj.transform.Find(muscles[i] + "Volume").GetComponent<TextMeshProUGUI>();
                var barComp = barObj.transform.Find("BarBG/BarFill").GetComponent<Image>();

                switch(muscles[i]) {
                    case "Chest": fitnessUI._chestVolumeText = textComp; fitnessUI._chestBar = barComp; break;
                    case "Back": fitnessUI._backVolumeText = textComp; fitnessUI._backBar = barComp; break;
                    case "Shoulders": fitnessUI._shouldersVolumeText = textComp; fitnessUI._shouldersBar = barComp; break;
                    case "Legs": fitnessUI._legsVolumeText = textComp; fitnessUI._legsBar = barComp; break;
                    case "Biceps": fitnessUI._bicepsVolumeText = textComp; fitnessUI._bicepsBar = barComp; break;
                    case "Triceps": fitnessUI._tricepsVolumeText = textComp; fitnessUI._tricepsBar = barComp; break;
                    case "Core": fitnessUI._coreVolumeText = textComp; fitnessUI._coreBar = barComp; break;
                    case "Cardio": fitnessUI._cardioVolumeText = textComp; fitnessUI._cardioBar = barComp; break;
                }
            }

            // Streak & Recovery at bottom of stats
            GameObject streakPanel = CreatePanel(statsCol.transform, "StreakInfo", new Color(0,0,0,0.3f));
            RectTransform streakRect = streakPanel.GetComponent<RectTransform>();
            streakRect.anchorMin = new Vector2(0, 0);
            streakRect.anchorMax = new Vector2(1, 0.15f);

            fitnessUI._streakText = CreateText(streakPanel.transform, "Streak", "Streak: 0 Days", 18, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.9f)).GetComponent<TextMeshProUGUI>();
            fitnessUI._recoveryText = CreateText(streakPanel.transform, "Recovery", "Recovery: 100%", 18, new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.45f)).GetComponent<TextMeshProUGUI>();

            // =================================================================================
            // 5. MAIN SECTION (RIGHT COLUMN)
            // =================================================================================
            
            // --- SECTION A: QUICK LOG (TOP) ---
            GameObject quickSection = CreatePanel(mainCol.transform, "QuickLog", new Color(0.18f, 0.22f, 0.28f, 1f));
            RectTransform quickRect = quickSection.GetComponent<RectTransform>();
            quickRect.anchorMin = new Vector2(0, 0.82f);
            quickRect.anchorMax = new Vector2(1, 1f);

            CreateText(quickSection.transform, "QuickTitle", "QUICK LOG", 16, new Vector2(0.02f, 0.7f), new Vector2(0.3f, 0.95f));

            float btnWidth = 0.23f;
            float btnGap = 0.02f;
            float btnYMin = 0.1f;
            float btnYMax = 0.65f;

            fitnessUI._quickBenchButton = CreateButton(quickSection.transform, "BtnBench", "CHEST", 14, 
                new Vector2(btnGap, btnYMin), new Vector2(btnGap + btnWidth, btnYMax)).GetComponent<Button>();
                
            fitnessUI._quickSquatButton = CreateButton(quickSection.transform, "BtnSquat", "LEGS", 14, 
                new Vector2(btnGap*2 + btnWidth, btnYMin), new Vector2(btnGap*2 + btnWidth*2, btnYMax)).GetComponent<Button>();

            fitnessUI._quickDeadliftButton = CreateButton(quickSection.transform, "BtnDead", "BACK", 14, 
                new Vector2(btnGap*3 + btnWidth*2, btnYMin), new Vector2(btnGap*3 + btnWidth*3, btnYMax)).GetComponent<Button>();

            fitnessUI._quickRunButton = CreateButton(quickSection.transform, "BtnRun", "RUN", 14, 
                new Vector2(btnGap*4 + btnWidth*3, btnYMin), new Vector2(btnGap*4 + btnWidth*4, btnYMax)).GetComponent<Button>();


            // --- SECTION B: MANUAL LOG (MIDDLE) ---
            GameObject manualSection = CreatePanel(mainCol.transform, "ManualLog", new Color(0.15f, 0.18f, 0.24f, 1f));
            RectTransform manualRect = manualSection.GetComponent<RectTransform>();
            manualRect.anchorMin = new Vector2(0, 0.55f);
            manualRect.anchorMax = new Vector2(1, 0.80f);

            CreateText(manualSection.transform, "ManTitle", "MANUAL ENTRY", 16, new Vector2(0.02f, 0.8f), new Vector2(1, 0.95f));

            // Row 1: Exercise Name & Muscle
            fitnessUI._exerciseInput = CreateInputField(manualSection.transform, "InpExercise", "Exercise Name...", 
                new Vector2(0.02f, 0.5f), new Vector2(0.6f, 0.75f)).GetComponent<TMP_InputField>();
            
            fitnessUI._muscleInput = CreateInputField(manualSection.transform, "InpMuscle", "Muscle...", 
                new Vector2(0.62f, 0.5f), new Vector2(0.98f, 0.75f)).GetComponent<TMP_InputField>();

            // Row 2: Weight, Reps, Log Button
            fitnessUI._weightInput = CreateInputField(manualSection.transform, "InpWeight", "Kg", 
                new Vector2(0.02f, 0.1f), new Vector2(0.25f, 0.4f)).GetComponent<TMP_InputField>();

            fitnessUI._repsInput = CreateInputField(manualSection.transform, "InpReps", "Reps", 
                new Vector2(0.27f, 0.1f), new Vector2(0.50f, 0.4f)).GetComponent<TMP_InputField>();

            GameObject logBtnObj = CreateButton(manualSection.transform, "BtnLog", "LOG WORKOUT", 18, 
                new Vector2(0.55f, 0.1f), new Vector2(0.98f, 0.4f));
            logBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green
            fitnessUI._logButton = logBtnObj.GetComponent<Button>();


            // --- SECTION C: HEALTH TRACKER (BOTTOM) ---
            GameObject healthSection = CreatePanel(mainCol.transform, "Health", new Color(0.12f, 0.15f, 0.20f, 1f));
            RectTransform healthRect2 = healthSection.GetComponent<RectTransform>();
            healthRect2.anchorMin = new Vector2(0, 0);
            healthRect2.anchorMax = new Vector2(1, 0.53f);

            CreateText(healthSection.transform, "HealthTitle", "DAILY HEALTH", 16, new Vector2(0.02f, 0.9f), new Vector2(1, 0.98f));

            // Grid for Health Inputs
            // Sleep
            CreateText(healthSection.transform, "LblSleep", "Sleep (h)", 12, new Vector2(0.02f, 0.75f), new Vector2(0.3f, 0.85f));
            fitnessUI._sleepHoursInput = CreateInputField(healthSection.transform, "InpSleep", "8", 
                new Vector2(0.02f, 0.6f), new Vector2(0.3f, 0.75f)).GetComponent<TMP_InputField>();

            // Water
            CreateText(healthSection.transform, "LblWater", "Water (L)", 12, new Vector2(0.35f, 0.75f), new Vector2(0.63f, 0.85f));
            fitnessUI._waterLitersInput = CreateInputField(healthSection.transform, "InpWater", "3", 
                new Vector2(0.35f, 0.6f), new Vector2(0.63f, 0.75f)).GetComponent<TMP_InputField>();

            // Steps
            CreateText(healthSection.transform, "LblSteps", "Steps", 12, new Vector2(0.68f, 0.75f), new Vector2(0.98f, 0.85f));
            fitnessUI._stepsInput = CreateInputField(healthSection.transform, "InpSteps", "10000", 
                new Vector2(0.68f, 0.6f), new Vector2(0.98f, 0.75f)).GetComponent<TMP_InputField>();

            // Button
            GameObject healthBtnObj = CreateButton(healthSection.transform, "BtnHealth", "UPDATE HEALTH", 16,
                new Vector2(0.2f, 0.05f), new Vector2(0.8f, 0.25f));
            healthBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.8f, 1f);
            fitnessUI._logHealthButton = healthBtnObj.GetComponent<Button>();
            
            // Health Stats Display (Middle of box)
            GameObject healthStats = CreatePanel(healthSection.transform, "HealthDisplay", Color.clear);
            RectTransform hsRect = healthStats.GetComponent<RectTransform>();
            hsRect.anchorMin = new Vector2(0.05f, 0.3f);
            hsRect.anchorMax = new Vector2(0.95f, 0.55f);
            
            fitnessUI._sleepText = CreateText(healthStats.transform, "TxtSleep", "Sleep: -", 14, new Vector2(0, 0.5f), new Vector2(0.33f, 1f)).GetComponent<TextMeshProUGUI>();
            fitnessUI._waterText = CreateText(healthStats.transform, "TxtWater", "Water: -", 14, new Vector2(0.33f, 0.5f), new Vector2(0.66f, 1f)).GetComponent<TextMeshProUGUI>();
            fitnessUI._stepsText = CreateText(healthStats.transform, "TxtSteps", "Steps: -", 14, new Vector2(0.66f, 0.5f), new Vector2(1f, 1f)).GetComponent<TextMeshProUGUI>();


            // =================================================================================
            // 6. INITIALIZATION 
            // =================================================================================
            
            // Force initialize immediately
            Debug.Log("[BIO-CLASH] UI Construction Complete. Initializing Logic...");
            fitnessUI.Initialize(); // <--- CRITICAL FIX: Wire buttons after creation
            
            fitnessPanel.SetActive(false); // Start hidden
        }

        // ===================================
        // HELPERS
        // ===================================

        private GameObject CreateMuscleBar(Transform parent, string muscleName, float yMin, float yMax)
        {
            GameObject container = new GameObject(muscleName + "_Row");
            container.transform.SetParent(parent, false);
            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, yMin);
            rect.anchorMax = new Vector2(1, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Label (Left)
            CreateText(container.transform, "Label", muscleName, 14, new Vector2(0, 0), new Vector2(0.25f, 1));
            
            // Bar BG (Middle)
            GameObject barBG = CreatePanel(container.transform, "BarBG", new Color(0,0,0,0.5f));
            RectTransform barRect = barBG.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.28f, 0.1f);
            barRect.anchorMax = new Vector2(0.98f, 0.5f);
            
            // Bar Fill - MUST be type=Filled for fillAmount to work!
            GameObject barFill = CreatePanel(barBG.transform, "BarFill", GetMuscleColor(muscleName));
            Image fillImg = barFill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0; // Left
            fillImg.fillAmount = 0.5f; // Default 50%
            
            // Set rect to full size since fillAmount controls the visible portion
            RectTransform fillRect = barFill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one; // Full size now!
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // Value Text (Above Bar or Inside)
            GameObject valText = CreateText(container.transform, muscleName + "Volume", "0 kg", 12, new Vector2(0.28f, 0.55f), new Vector2(0.98f, 1f));
            valText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;

            return container;
        }

        private Color GetMuscleColor(string muscle)
        {
            switch (muscle.ToLower())
            {
                case "chest": return new Color(1f, 0.4f, 0.4f); // Red
                case "back": return new Color(0.4f, 0.6f, 1f); // Blue
                case "shoulders": return new Color(1f, 0.8f, 0.3f); // Orange
                case "legs": return new Color(0.4f, 1f, 0.4f); // Green
                case "biceps": return new Color(1f, 0.5f, 0.8f); // Pink
                case "triceps": return new Color(0.7f, 0.5f, 1f); // Purple
                case "core": return new Color(1f, 1f, 0.5f); // Yellow
                case "cardio": return new Color(0.5f, 1f, 1f); // Cyan
                default: return Color.white;
            }
        }
    }
}
