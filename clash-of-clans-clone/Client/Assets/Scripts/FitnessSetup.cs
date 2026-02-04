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
            // ALWAYS run cleanup first - destroy any old/ugly UI_Fitness regardless of other conditions
            UI_Fitness[] existingUIs = FindObjectsByType<UI_Fitness>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var oldUI in existingUIs)
            {
                Debug.Log($"[BIO-CLASH] AutoInit: Destroying old UI_Fitness: {oldUI.gameObject.name}");
                DestroyImmediate(oldUI.gameObject);
            }
            
            // Only create FitnessSetup if not already present
            if (FindFirstObjectByType<FitnessSetup>() == null)
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
            UI_Fitness[] existingUIs = FindObjectsByType<UI_Fitness>(FindObjectsInactive.Include, FindObjectsSortMode.None); // include inactive
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
                mainCanvas = FindFirstObjectByType<Canvas>();
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
                _cachedFitnessUI = FindFirstObjectByType<UI_Fitness>();
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
            GameObject titleGO = CreateText(headerObj.transform, "Title", "[GYM] FITNESS CENTER", 32, TextAlignmentOptions.Center);
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
            
            // Stats title with checkbox styling (matching user's screenshot)
            GameObject statsTitleBg = CreatePanel(statsCol.transform, "StatsTitleBG", new Color(0.1f, 0.12f, 0.16f, 0.9f));
            RectTransform statsTitleRect = statsTitleBg.GetComponent<RectTransform>();
            statsTitleRect.anchorMin = new Vector2(0, 0.92f);
            statsTitleRect.anchorMax = new Vector2(1, 1f);
            
            CreateText(statsTitleBg.transform, "StatsTitle", "□ YOUR STATS", 20, TextAlignmentOptions.Left)
                .GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 1f);

            string[] muscles = { "Chest", "Back", "Shoulders", "Biceps", "Triceps", "Legs", "Core", "Cardio" };
            float barHeight = 0.085f;
            float gap = 0.015f;
            float startY = 0.90f;

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
            quickRect.anchorMin = new Vector2(0, 0.88f);
            quickRect.anchorMax = new Vector2(1, 1f);

            CreateText(quickSection.transform, "QuickTitle", "Quick Log:", 14, new Vector2(0.02f, 0.1f), new Vector2(0.18f, 0.9f));

            float qBtnWidth = 0.19f;
            float qBtnGap = 0.01f;
            float qStartX = 0.20f;

            GameObject benchBtn = CreateButton(quickSection.transform, "BtnBench", "[GYM] Bench", 12, 
                new Vector2(qStartX, 0.15f), new Vector2(qStartX + qBtnWidth, 0.85f));
            benchBtn.GetComponent<Image>().color = new Color(0.9f, 0.5f, 0.2f, 1f); // Orange
            fitnessUI._quickBenchButton = benchBtn.GetComponent<Button>();
                
            GameObject squatBtn = CreateButton(quickSection.transform, "BtnSquat", "□ Squat", 12, 
                new Vector2(qStartX + qBtnWidth + qBtnGap, 0.15f), new Vector2(qStartX + qBtnWidth*2 + qBtnGap, 0.85f));
            squatBtn.GetComponent<Image>().color = new Color(0.4f, 0.6f, 0.8f, 1f); // Blue
            fitnessUI._quickSquatButton = squatBtn.GetComponent<Button>();

            GameObject deadBtn = CreateButton(quickSection.transform, "BtnDead", "□ Deadlift", 12, 
                new Vector2(qStartX + qBtnWidth*2 + qBtnGap*2, 0.15f), new Vector2(qStartX + qBtnWidth*3 + qBtnGap*2, 0.85f));
            deadBtn.GetComponent<Image>().color = new Color(0.4f, 0.6f, 0.8f, 1f); // Blue
            fitnessUI._quickDeadliftButton = deadBtn.GetComponent<Button>();

            GameObject runBtn = CreateButton(quickSection.transform, "BtnRun", "□ Run", 12, 
                new Vector2(qStartX + qBtnWidth*3 + qBtnGap*3, 0.15f), new Vector2(qStartX + qBtnWidth*4 + qBtnGap*3, 0.85f));
            runBtn.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f, 1f); // Red
            fitnessUI._quickRunButton = runBtn.GetComponent<Button>();

            // --- SECTION B: ACTIVE WORKOUT SESSION ---
            GameObject workoutSection = CreatePanel(mainCol.transform, "ActiveWorkout", new Color(0.12f, 0.14f, 0.18f, 1f));
            RectTransform workoutRect = workoutSection.GetComponent<RectTransform>();
            workoutRect.anchorMin = new Vector2(0, 0.62f);
            workoutRect.anchorMax = new Vector2(1, 0.86f);

            // Title row
            CreateText(workoutSection.transform, "WrkTitle", "[GYM] ACTIVE WORKOUT", 16, 
                new Vector2(0.02f, 0.82f), new Vector2(0.4f, 0.98f));
            
            fitnessUI._workoutTimerText = CreateText(workoutSection.transform, "Timer", "[TIME] 0:00", 16, 
                new Vector2(0.4f, 0.82f), new Vector2(0.6f, 0.98f), TextAlignmentOptions.Center).GetComponent<TextMeshProUGUI>();
            
            fitnessUI._workoutStatusText = CreateText(workoutSection.transform, "Status", "[ZZZ] No workout in progress", 14, 
                new Vector2(0.6f, 0.82f), new Vector2(0.98f, 0.98f), TextAlignmentOptions.Right).GetComponent<TextMeshProUGUI>();

            // Stats row
            fitnessUI._totalVolumeText = CreateText(workoutSection.transform, "TotalVol", "[*] 0 kg", 13, 
                new Vector2(0.02f, 0.65f), new Vector2(0.3f, 0.8f)).GetComponent<TextMeshProUGUI>();
            
            fitnessUI._exerciseCountText = CreateText(workoutSection.transform, "ExCount", "[#] 0 exercises", 13, 
                new Vector2(0.3f, 0.65f), new Vector2(0.6f, 0.8f), TextAlignmentOptions.Center).GetComponent<TextMeshProUGUI>();

            // Reset button (to clear old data)
            GameObject resetBtn = CreateButton(workoutSection.transform, "BtnReset", "[X] Reset", 11, 
                new Vector2(0.6f, 0.65f), new Vector2(0.80f, 0.8f));
            resetBtn.GetComponent<Image>().color = new Color(0.6f, 0.3f, 0.3f, 1f); // Red-ish
            Button resetButton = resetBtn.GetComponent<Button>();
            resetButton.onClick.AddListener(() => {
                if (FitnessManager.instance != null) {
                    FitnessManager.instance.ResetAllData();
                    fitnessUI.RefreshStats();
                    Debug.Log("🔄 All fitness data reset!");
                }
            });

            // Control buttons row - START button (visible when no workout active)
            GameObject startBtn = CreateButton(workoutSection.transform, "BtnStart", "▶ START", 14, 
                new Vector2(0.02f, 0.38f), new Vector2(0.32f, 0.62f));
            startBtn.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green
            fitnessUI._startWorkoutButton = startBtn.GetComponent<Button>();

            GameObject pauseBtn = CreateButton(workoutSection.transform, "BtnPause", "[||] Pause", 14, 
                new Vector2(0.34f, 0.38f), new Vector2(0.64f, 0.62f));
            pauseBtn.GetComponent<Image>().color = new Color(0.3f, 0.6f, 0.8f, 1f); // Blue
            fitnessUI._pauseWorkoutButton = pauseBtn.GetComponent<Button>();

            GameObject addSetBtn = CreateButton(workoutSection.transform, "BtnAddSet", "□ ADD SET", 14, 
                new Vector2(0.66f, 0.38f), new Vector2(0.98f, 0.62f));
            addSetBtn.GetComponent<Image>().color = new Color(0.3f, 0.7f, 0.7f, 1f); // Cyan
            fitnessUI._addExerciseButton = addSetBtn.GetComponent<Button>();

            // Stop button (hidden until workout starts)
            GameObject stopBtn = CreateButton(workoutSection.transform, "BtnStop", "■ STOP", 14, 
                new Vector2(0.02f, 0.38f), new Vector2(0.32f, 0.62f));
            stopBtn.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f, 1f); // Red
            fitnessUI._stopWorkoutButton = stopBtn.GetComponent<Button>();
            stopBtn.SetActive(false); // Hidden initially

            // Input row: Exercise, Muscle, Kg, Reps, Sets
            CreateText(workoutSection.transform, "LblExercise", "Exercise:", 11, 
                new Vector2(0.02f, 0.2f), new Vector2(0.12f, 0.35f));
            fitnessUI._exerciseInput = CreateInputField(workoutSection.transform, "InpExercise", "Bench Press", 
                new Vector2(0.12f, 0.2f), new Vector2(0.38f, 0.35f)).GetComponent<TMP_InputField>();

            CreateText(workoutSection.transform, "LblMuscle", "Muscle:", 11, 
                new Vector2(0.40f, 0.2f), new Vector2(0.50f, 0.35f));
            fitnessUI._muscleInput = CreateInputField(workoutSection.transform, "InpMuscle", "Chest", 
                new Vector2(0.50f, 0.2f), new Vector2(0.70f, 0.35f)).GetComponent<TMP_InputField>();

            // Bottom row: Kg, Reps, Sets, LOG Button
            CreateText(workoutSection.transform, "LblKg", "Kg:", 11, 
                new Vector2(0.02f, 0.02f), new Vector2(0.08f, 0.17f));
            fitnessUI._weightInput = CreateInputField(workoutSection.transform, "InpKg", "60", 
                new Vector2(0.08f, 0.02f), new Vector2(0.20f, 0.17f)).GetComponent<TMP_InputField>();

            CreateText(workoutSection.transform, "LblReps", "Reps:", 11, 
                new Vector2(0.22f, 0.02f), new Vector2(0.30f, 0.17f));
            fitnessUI._repsInput = CreateInputField(workoutSection.transform, "InpReps", "10", 
                new Vector2(0.30f, 0.02f), new Vector2(0.42f, 0.17f)).GetComponent<TMP_InputField>();

            CreateText(workoutSection.transform, "LblSets", "Sets:", 11, 
                new Vector2(0.44f, 0.02f), new Vector2(0.52f, 0.17f));
            fitnessUI._setsInput = CreateInputField(workoutSection.transform, "InpSets", "3", 
                new Vector2(0.52f, 0.02f), new Vector2(0.64f, 0.17f)).GetComponent<TMP_InputField>();

            // --- CRITICAL FIX: ENSURE INPUTS ARE ASSIGNED ---
            // Sometimes GetComponent on the result of CreateInputField (which returns GO) might be tricky if not immediate
            // Re-finding them just to be 100% sure they are wired
            if (fitnessUI._exerciseInput == null) fitnessUI._exerciseInput = fitnessUI._panel.transform.Find("MainColumn/ActiveWorkout/InpExercise").GetComponent<TMP_InputField>();
            if (fitnessUI._muscleInput == null) fitnessUI._muscleInput = fitnessUI._panel.transform.Find("MainColumn/ActiveWorkout/InpMuscle").GetComponent<TMP_InputField>();
            if (fitnessUI._weightInput == null) fitnessUI._weightInput = fitnessUI._panel.transform.Find("MainColumn/ActiveWorkout/InpKg").GetComponent<TMP_InputField>();
            if (fitnessUI._repsInput == null) fitnessUI._repsInput = fitnessUI._panel.transform.Find("MainColumn/ActiveWorkout/InpReps").GetComponent<TMP_InputField>();
            if (fitnessUI._setsInput == null) fitnessUI._setsInput = fitnessUI._panel.transform.Find("MainColumn/ActiveWorkout/InpSets").GetComponent<TMP_InputField>();


            GameObject logBtnObj = CreateButton(workoutSection.transform, "BtnLog", "□ LOG", 14, 
                new Vector2(0.68f, 0.02f), new Vector2(0.98f, 0.17f));
            logBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green
            fitnessUI._logButton = logBtnObj.GetComponent<Button>();


            // --- SECTION C: HEALTH TRACKER (BOTTOM) ---
            GameObject healthSection = CreatePanel(mainCol.transform, "Health", new Color(0.12f, 0.15f, 0.20f, 1f));
            RectTransform healthRect2 = healthSection.GetComponent<RectTransform>();
            healthRect2.anchorMin = new Vector2(0, 0);
            healthRect2.anchorMax = new Vector2(1, 0.60f);

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

        // ===================================
        // HELPER METHODS - UI ELEMENT CREATION
        // ===================================

        /// <summary>
        /// Creates a UI panel with RectTransform and Image components.
        /// </summary>
        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image img = panel.AddComponent<Image>();
            img.color = color;
            
            return panel;
        }

        /// <summary>
        /// Creates a TextMeshProUGUI element with full parent anchors.
        /// </summary>
        private GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions align)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            RectTransform rect = textGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            
            return textGO;
        }

        /// <summary>
        /// Creates a TextMeshProUGUI element with specified anchor positions.
        /// </summary>
        private GameObject CreateText(Transform parent, string name, string text, int fontSize, 
            Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions align = TextAlignmentOptions.Left)
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
            tmp.color = Color.white;
            
            return textGO;
        }

        /// <summary>
        /// Creates a Button with Image and TextMeshProUGUI child.
        /// </summary>
        private GameObject CreateButton(Transform parent, string name, string text, int fontSize, 
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            
            RectTransform rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.3f, 0.5f, 0.7f, 1f); // Default blue button color
            
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            
            // Add button hover effect
            ColorBlock colors = btn.colors;
            colors.normalColor = img.color;
            colors.highlightedColor = new Color(img.color.r + 0.1f, img.color.g + 0.1f, img.color.b + 0.1f, 1f);
            colors.pressedColor = new Color(img.color.r - 0.1f, img.color.g - 0.1f, img.color.b - 0.1f, 1f);
            btn.colors = colors;
            
            // Add text child
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(4, 2);
            textRect.offsetMax = new Vector2(-4, -2);
            
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return btnGO;
        }

        /// <summary>
        /// Creates a TMP_InputField with placeholder and input text.
        /// </summary>
        private GameObject CreateInputField(Transform parent, string name, string placeholder, 
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);
            
            RectTransform rect = inputGO.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image img = inputGO.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Dark input background
            
            // Text Area (viewport for input)
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGO.transform, false);
            
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(8, 4);
            textAreaRect.offsetMax = new Vector2(-8, -4);
            
            RectMask2D mask = textArea.AddComponent<RectMask2D>();
            
            // Placeholder text
            GameObject placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(textArea.transform, false);
            
            RectTransform placeholderRect = placeholderGO.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI placeholderTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderTMP.text = placeholder;
            placeholderTMP.fontSize = 14;
            placeholderTMP.fontStyle = FontStyles.Italic;
            placeholderTMP.alignment = TextAlignmentOptions.Left;
            placeholderTMP.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            
            // Actual input text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(textArea.transform, false);
            
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI inputTMP = textGO.AddComponent<TextMeshProUGUI>();
            inputTMP.text = "";
            inputTMP.fontSize = 14;
            inputTMP.alignment = TextAlignmentOptions.Left;
            inputTMP.color = Color.white;
            
            // Add TMP_InputField component
            TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputTMP;
            inputField.placeholder = placeholderTMP;
            inputField.fontAsset = inputTMP.font;
            
            // Style the caret
            inputField.caretColor = Color.white;
            inputField.selectionColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
            
            return inputGO;
        }
    }
}
