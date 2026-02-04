namespace DevelopersHub.ClashOfWhatecer
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Auto-setup script for Bio-Clash Fitness system.
    /// Attach this to an empty GameObject in the scene.
    /// It will automatically create the FitnessManager and configure the UI.
    /// </summary>
    public class FitnessSetup : MonoBehaviour
    {
        [Header("Auto-Create These Components")]
        public bool createFitnessManager = true;
        public bool createFitnessUI = true;

        [Header("References (Auto-Found if Empty)")]
        public Canvas mainCanvas;
        public UI_Main uiMain;

        private void Awake()
        {
            // Create FitnessManager singleton if needed
            if (createFitnessManager && FitnessManager.instance == null)
            {
                GameObject fitnessManagerGO = new GameObject("FitnessManager");
                fitnessManagerGO.AddComponent<FitnessManager>();
                DontDestroyOnLoad(fitnessManagerGO);
                Debug.Log("✅ FitnessManager created automatically");
            }

            // Find main canvas if not assigned
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
            }

            // Find UI_Main if not assigned
            if (uiMain == null)
            {
                uiMain = FindObjectOfType<UI_Main>();
            }

            // Create Fitness UI panel if needed
            if (createFitnessUI && mainCanvas != null)
            {
                CreateFitnessUIPanel();
            }
        }

        /// <summary>
        /// Creates the Fitness UI panel programmatically with all components.
        /// </summary>
        private void CreateFitnessUIPanel()
        {
            // Check if UI_Fitness already exists
            if (FindObjectOfType<UI_Fitness>() != null)
            {
                Debug.Log("UI_Fitness already exists in scene");
                return;
            }

            // Create main panel
            GameObject fitnessPanel = new GameObject("UI_Fitness");
            fitnessPanel.transform.SetParent(mainCanvas.transform, false);
            
            // Add UI_Fitness component
            UI_Fitness fitnessUI = fitnessPanel.AddComponent<UI_Fitness>();
            
            // Create RectTransform
            RectTransform panelRect = fitnessPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Assign main panel
            fitnessUI._panel = fitnessPanel;

            // Create background panel
            GameObject bgPanel = CreatePanel(fitnessPanel.transform, "Background", new Color(0.13f, 0.16f, 0.22f, 0.98f));
            RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.15f, 0.1f);
            bgRect.anchorMax = new Vector2(0.85f, 0.9f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Create title
            GameObject titleGO = CreateText(bgPanel.transform, "Title", "💪 FITNESS CENTER", 42, TextAlignmentOptions.Center);
            TextMeshProUGUI titleText = titleGO.GetComponent<TextMeshProUGUI>();
            titleText.color = new Color(1f, 0.92f, 0.5f, 1f); // CoC Gold
            
            // Add outline for impact
            Outline outline = titleGO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);

            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1);

            // Create close button
            GameObject closeBtn = CreateButton(bgPanel.transform, "CloseButton", "✕", 32);
            closeBtn.GetComponent<Image>().color = new Color(0.85f, 0.1f, 0.1f, 1f); // CoC Red
            
             // Add outline to button frame
            Outline btnOutline = closeBtn.AddComponent<Outline>();
            btnOutline.effectColor = Color.black;
            btnOutline.effectDistance = new Vector2(1, -1);
            
            RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.92f, 0.92f);
            closeBtnRect.anchorMax = new Vector2(0.98f, 0.98f);
            fitnessUI._closeButton = closeBtn.GetComponent<Button>();

            // Create Quick Log section (TOP: 0.88 - 0.96)
            GameObject quickLogSection = CreatePanel(bgPanel.transform, "QuickLogSection", new Color(0.2f, 0.25f, 0.3f, 1f));
            RectTransform quickRect = quickLogSection.GetComponent<RectTransform>();
            quickRect.anchorMin = new Vector2(0.05f, 0.88f);
            quickRect.anchorMax = new Vector2(0.95f, 0.96f);

            CreateText(quickLogSection.transform, "QuickLabel", "Quick Log:", 18, TextAlignmentOptions.Left);

            // Quick log buttons row
            float buttonWidth = 0.23f;
            fitnessUI._quickBenchButton = CreateButton(quickLogSection.transform, "BenchButton", "🏋️ Bench", 14, new Vector2(0.15f, 0.1f), new Vector2(0.15f + buttonWidth, 0.9f)).GetComponent<Button>();
            fitnessUI._quickSquatButton = CreateButton(quickLogSection.transform, "SquatButton", "🦵 Squat", 14, new Vector2(0.40f, 0.1f), new Vector2(0.40f + buttonWidth, 0.9f)).GetComponent<Button>();
            fitnessUI._quickDeadliftButton = CreateButton(quickLogSection.transform, "DeadliftButton", "🦴 Deadlift", 14, new Vector2(0.65f, 0.1f), new Vector2(0.65f + buttonWidth, 0.9f)).GetComponent<Button>();

            // Create Workout Session section (0.72 - 0.86)
            GameObject workoutSection = CreatePanel(bgPanel.transform, "WorkoutSessionSection", new Color(0.12f, 0.1f, 0.18f, 1f));
            RectTransform workoutRect = workoutSection.GetComponent<RectTransform>();
            workoutRect.anchorMin = new Vector2(0.05f, 0.72f);
            workoutRect.anchorMax = new Vector2(0.95f, 0.86f);
            fitnessUI._workoutSessionPanel = workoutSection;

            CreateText(workoutSection.transform, "WorkoutTitle", "🏋️ ACTIVE WORKOUT", 20, new Vector2(0, 0.65f), new Vector2(0.35f, 1f));
            fitnessUI._workoutTimerText = CreateText(workoutSection.transform, "WorkoutTimer", "⏱️ 0:00", 24, new Vector2(0.35f, 0.65f), new Vector2(0.65f, 1f)).GetComponent<TextMeshProUGUI>();
            fitnessUI._workoutStatusText = CreateText(workoutSection.transform, "WorkoutStatus", "💤 Tap START", 14, new Vector2(0.65f, 0.65f), new Vector2(1f, 1f)).GetComponent<TextMeshProUGUI>();

            // Session stats row
            fitnessUI._totalVolumeText = CreateText(workoutSection.transform, "TotalVolume", "💪 0 kg", 14, new Vector2(0.02f, 0.35f), new Vector2(0.35f, 0.6f)).GetComponent<TextMeshProUGUI>();
            fitnessUI._exerciseCountText = CreateText(workoutSection.transform, "ExerciseCount", "📋 0 sets", 14, new Vector2(0.35f, 0.35f), new Vector2(0.65f, 0.6f)).GetComponent<TextMeshProUGUI>();

            // Control buttons row
            fitnessUI._startWorkoutButton = CreateButton(workoutSection.transform, "StartWorkoutBtn", "▶️ START", 14, new Vector2(0.02f, 0.02f), new Vector2(0.24f, 0.32f)).GetComponent<Button>();
            fitnessUI._startWorkoutButton.image.color = new Color(0.4f, 0.8f, 0.2f, 1f); // Emerald Green

            fitnessUI._stopWorkoutButton = CreateButton(workoutSection.transform, "StopWorkoutBtn", "⏹️ FINISH", 14, new Vector2(0.02f, 0.02f), new Vector2(0.24f, 0.32f)).GetComponent<Button>();
            fitnessUI._stopWorkoutButton.image.color = new Color(1f, 0.6f, 0.2f, 1f); // Orange
            fitnessUI._pauseWorkoutButton = CreateButton(workoutSection.transform, "PauseWorkoutBtn", "⏸️ PAUSE", 14, new Vector2(0.26f, 0.02f), new Vector2(0.48f, 0.32f)).GetComponent<Button>();
            fitnessUI._addExerciseButton = CreateButton(workoutSection.transform, "AddExerciseBtn", "➕ ADD SET", 14, new Vector2(0.50f, 0.02f), new Vector2(0.72f, 0.32f)).GetComponent<Button>();
            fitnessUI._quickRunButton = CreateButton(quickLogSection.transform, "RunButton", "🏃 Run", 14, new Vector2(0.74f, 0.02f), new Vector2(0.98f, 0.32f)).GetComponent<Button>();

            // Create Workout Input section (0.54 - 0.70)
            GameObject inputSection = CreatePanel(bgPanel.transform, "InputSection", new Color(0.15f, 0.2f, 0.25f, 1f));
            RectTransform inputRect = inputSection.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.05f, 0.54f);
            inputRect.anchorMax = new Vector2(0.95f, 0.70f);

            // Input row 1: Exercise + Muscle
            CreateText(inputSection.transform, "ExerciseLabel", "Exercise:", 14, new Vector2(0.02f, 0.55f), new Vector2(0.15f, 0.95f));
            fitnessUI._exerciseInput = CreateInputField(inputSection.transform, "ExerciseInput", "Bench Press", new Vector2(0.16f, 0.55f), new Vector2(0.48f, 0.95f)).GetComponent<TMP_InputField>();
            
            CreateText(inputSection.transform, "MuscleLabel", "Muscle:", 14, new Vector2(0.50f, 0.55f), new Vector2(0.62f, 0.95f));
            fitnessUI._muscleInput = CreateInputField(inputSection.transform, "MuscleInput", "Chest", new Vector2(0.63f, 0.55f), new Vector2(0.98f, 0.95f)).GetComponent<TMP_InputField>();
            
            fitnessUI._weightInput = CreateInputField(inputSection.transform, "WeightInput", "60", new Vector2(0.11f, 0.05f), new Vector2(0.25f, 0.50f)).GetComponent<TMP_InputField>();
            
            CreateText(inputSection.transform, "WeightLabel", "Kg:", 14, new Vector2(0.02f, 0.05f), new Vector2(0.10f, 0.50f));
            CreateText(inputSection.transform, "RepsLabel", "Reps:", 14, new Vector2(0.27f, 0.05f), new Vector2(0.38f, 0.50f));
            fitnessUI._repsInput = CreateInputField(inputSection.transform, "RepsInput", "10", new Vector2(0.39f, 0.05f), new Vector2(0.52f, 0.50f)).GetComponent<TMP_InputField>();
            
            CreateText(inputSection.transform, "SetsLabel", "Sets:", 14, new Vector2(0.54f, 0.05f), new Vector2(0.65f, 0.50f));
            fitnessUI._setsInput = CreateInputField(inputSection.transform, "SetsInput", "3", new Vector2(0.66f, 0.05f), new Vector2(0.78f, 0.50f)).GetComponent<TMP_InputField>();
            
            fitnessUI._logButton = CreateButton(inputSection.transform, "LogButton", "📝 LOG", 16, new Vector2(0.80f, 0.05f), new Vector2(0.98f, 0.50f)).GetComponent<Button>();
            fitnessUI._logButton.image.color = new Color(0.4f, 0.8f, 0.2f, 1f); // Emerald Green

            // Create Stats section (LEFT: 0.05-0.48, 0.08-0.52)
            GameObject statsSection = CreatePanel(bgPanel.transform, "StatsSection", new Color(0.12f, 0.15f, 0.2f, 1f));
            RectTransform statsRect = statsSection.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.05f, 0.08f);
            statsRect.anchorMax = new Vector2(0.48f, 0.52f);

            CreateText(statsSection.transform, "StatsTitle", "📊 YOUR STATS", 24, new Vector2(0, 0.88f), new Vector2(1, 1));

            // Muscle volume bars - We need to assign these to specific fields
            // Dictionary to map names to fields would be ideal, but for now manual assignment:
            string[] muscles = { "Chest", "Back", "Shoulders", "Legs", "Biceps", "Triceps", "Core", "Cardio" };
            float yStart = 0.78f;
            float yStep = 0.1f;
            
            for (int i = 0; i < muscles.Length; i++)
            {
                float y = yStart - (i * yStep);
                var barObj = CreateMuscleBar(statsSection.transform, muscles[i], y);
                // Assign to fitnessUI based on name
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

            // Recovery and Streak
            fitnessUI._recoveryText = CreateText(statsSection.transform, "RecoveryLabel", "❤️ Recovery: 100%", 20, new Vector2(0.02f, 0.02f), new Vector2(0.35f, 0.12f)).GetComponent<TextMeshProUGUI>();
            fitnessUI._streakText = CreateText(statsSection.transform, "StreakLabel", "🔥 Streak: 0 days", 20, new Vector2(0.4f, 0.02f), new Vector2(0.7f, 0.12f)).GetComponent<TextMeshProUGUI>();
            fitnessUI._attackPowerText = CreateText(statsSection.transform, "AttackLabel", "⚔️ Power: 0", 20, new Vector2(0.72f, 0.02f), new Vector2(0.98f, 0.12f)).GetComponent<TextMeshProUGUI>();

            // ============================================================
            // HEALTH TRACKING SECTION (RIGHT: 0.52-0.95, 0.08-0.52)
            // ============================================================
            GameObject healthSection = CreatePanel(bgPanel.transform, "HealthSection", new Color(0.1f, 0.18f, 0.15f, 1f));
            RectTransform healthRect = healthSection.GetComponent<RectTransform>();
            healthRect.anchorMin = new Vector2(0.52f, 0.08f);
            healthRect.anchorMax = new Vector2(0.95f, 0.52f);

            CreateText(healthSection.transform, "HealthTitle", "💚 HEALTH", 18, new Vector2(0, 0.88f), new Vector2(1, 1));

            // Health input fields (compact layout)
            CreateText(healthSection.transform, "SleepLabel", "😴 Sleep:", 12, new Vector2(0.02f, 0.72f), new Vector2(0.35f, 0.85f));
            fitnessUI._sleepHoursInput = CreateInputField(healthSection.transform, "SleepInput", "8", new Vector2(0.36f, 0.72f), new Vector2(0.98f, 0.85f)).GetComponent<TMP_InputField>();

            CreateText(healthSection.transform, "WaterLabel", "💧 Water:", 12, new Vector2(0.02f, 0.55f), new Vector2(0.35f, 0.68f));
            fitnessUI._waterLitersInput = CreateInputField(healthSection.transform, "WaterInput", "2.5", new Vector2(0.36f, 0.55f), new Vector2(0.98f, 0.68f)).GetComponent<TMP_InputField>();

            CreateText(healthSection.transform, "StepsLabel", "👟 Steps:", 12, new Vector2(0.02f, 0.38f), new Vector2(0.35f, 0.51f));
            fitnessUI._stepsInput = CreateInputField(healthSection.transform, "StepsInput", "5000", new Vector2(0.36f, 0.38f), new Vector2(0.98f, 0.51f)).GetComponent<TMP_InputField>();

            CreateText(healthSection.transform, "HeartLabel", "❤️ BPM:", 12, new Vector2(0.02f, 0.21f), new Vector2(0.35f, 0.34f));
            fitnessUI._heartRateInput = CreateInputField(healthSection.transform, "HeartInput", "72", new Vector2(0.36f, 0.21f), new Vector2(0.98f, 0.34f)).GetComponent<TMP_InputField>();

            // Log Health button
            fitnessUI._logHealthButton = CreateButton(healthSection.transform, "LogHealthButton", "📝 LOG", 14, new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.18f)).GetComponent<Button>();
            
            // Exercise List Container
            // We need a scroll view for the exercise list in the workout section
            GameObject scrollObj = CreatePanel(workoutSection.transform, "ExerciseListScroll", new Color(0,0,0,0.3f));
            RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.02f, 0.35f); // Overwrite volume/count text area? No, the list should check where it goes. 
            // In layout I see Volume/Count at 0.35-0.6. The list can go below or above?
            // Actually I want the list to be creating items. 
            // Let's create a container for it.
            fitnessUI._exerciseListContainer = scrollObj.transform;
            
            // Exercise Item Prefab - Create a dummy one to clone
            fitnessUI._exerciseItemPrefab = new GameObject("ExerciseItemPrefab");
            fitnessUI._exerciseItemPrefab.AddComponent<RectTransform>();
            fitnessUI._exerciseItemPrefab.AddComponent<TextMeshProUGUI>();
            fitnessUI._exerciseItemPrefab.transform.SetParent(fitnessPanel.transform);
            fitnessUI._exerciseItemPrefab.SetActive(false);

            // Initially hide the panel
            fitnessPanel.SetActive(false);

            Debug.Log("✅ Fitness UI with Workout Session created automatically");
        }

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

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            RectTransform rect = textGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10, 0);
            rect.offsetMax = new Vector2(-10, 0);
            
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            
            return textGO;
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject textGO = CreateText(parent, name, text, fontSize);
            RectTransform rect = textGO.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return textGO;
        }

        private GameObject CreateButton(Transform parent, string name, string text, int fontSize)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            
            RectTransform rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            
            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            
            // Add hover color
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.7f, 1f, 1f);
            colors.pressedColor = new Color(0.2f, 0.5f, 0.8f, 1f);
            btn.colors = colors;
            
            // Add text child
            GameObject textGO = CreateText(btnGO.transform, "Text", text, fontSize, TextAlignmentOptions.Center);
            
            return btnGO;
        }

        private GameObject CreateButton(Transform parent, string name, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject btn = CreateButton(parent, name, text, fontSize);
            RectTransform rect = btn.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return btn;
        }

        private GameObject CreateInputField(Transform parent, string name, string placeholder, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);
            
            RectTransform rect = inputGO.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image img = inputGO.AddComponent<Image>();
            img.color = new Color(0.1f, 0.12f, 0.15f, 1f); // Deep Dark Blue
            
            // Add subtle outline
            Outline outline = inputGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.35f, 0.45f, 1f);
            outline.effectDistance = new Vector2(1, -1);
            
            // Text area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGO.transform, false);
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(5, 2);
            textAreaRect.offsetMax = new Vector2(-5, -2);
            
            // Placeholder
            GameObject placeholderGO = CreateText(textArea.transform, "Placeholder", placeholder, 16);
            TextMeshProUGUI placeholderTMP = placeholderGO.GetComponent<TextMeshProUGUI>();
            placeholderTMP.color = new Color(0.5f, 0.5f, 0.6f, 1f);
            
            // Input text
            GameObject textGO = CreateText(textArea.transform, "Text", "", 16);
            
            TMP_InputField input = inputGO.AddComponent<TMP_InputField>();
            input.textComponent = textGO.GetComponent<TextMeshProUGUI>();
            input.placeholder = placeholderTMP;
            input.textViewport = textAreaRect;
            
            return inputGO;
        }

        private GameObject CreateMuscleBar(Transform parent, string muscleName, float y)
        {
            // Label
            CreateText(parent, muscleName + "Label", muscleName + ":", 16, new Vector2(0.02f, y - 0.08f), new Vector2(0.18f, y));
            
            // Background bar
            GameObject bgBar = new GameObject(muscleName + "BarBG");
            bgBar.transform.SetParent(parent, false);
            RectTransform bgRect = bgBar.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.2f, y - 0.06f);
            bgRect.anchorMax = new Vector2(0.75f, y - 0.02f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bgBar.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.5f); // Translucent Black
            
            // Add outline to bar
            Outline barOutline = bgBar.AddComponent<Outline>();
            barOutline.effectColor = new Color(0.4f, 0.4f, 0.45f, 0.8f);
            barOutline.effectDistance = new Vector2(1, -1);
            
            // Fill bar
            GameObject fillBar = new GameObject(muscleName + "BarFill");
            fillBar.transform.SetParent(bgBar.transform, false);
            RectTransform fillRect = fillBar.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1); // 50% fill as example
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImg = fillBar.AddComponent<Image>();
            fillImg.color = GetMuscleColor(muscleName);
            
            // Volume text
            CreateText(parent, muscleName + "Volume", "500/1000 kg", 14, new Vector2(0.77f, y - 0.08f), new Vector2(0.98f, y));
            
            return bgBar;
        }

        private Color GetMuscleColor(string muscle)
        {
            switch (muscle.ToLower())
            {
                case "chest": return new Color(0.9f, 0.3f, 0.3f, 1f);
                case "back": return new Color(0.3f, 0.7f, 0.9f, 1f);
                case "shoulders": return new Color(0.9f, 0.7f, 0.3f, 1f);
                case "legs": return new Color(0.5f, 0.9f, 0.3f, 1f);
                case "biceps": return new Color(0.9f, 0.5f, 0.3f, 1f);
                case "triceps": return new Color(0.7f, 0.3f, 0.9f, 1f);
                case "core": return new Color(0.9f, 0.9f, 0.3f, 1f);
                case "cardio": return new Color(0.3f, 0.9f, 0.7f, 1f);
                default: return Color.white;
            }
        }
    }
}
