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

            // Create background panel
            GameObject bgPanel = CreatePanel(fitnessPanel.transform, "Background", new Color(0.1f, 0.1f, 0.15f, 0.95f));
            RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.15f, 0.1f);
            bgRect.anchorMax = new Vector2(0.85f, 0.9f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Create title
            GameObject titleGO = CreateText(bgPanel.transform, "Title", "💪 FITNESS CENTER", 36, TextAlignmentOptions.Center);
            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1);

            // Create close button
            GameObject closeBtn = CreateButton(bgPanel.transform, "CloseButton", "✕", 32);
            RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.92f, 0.92f);
            closeBtnRect.anchorMax = new Vector2(0.98f, 0.98f);

            // Create Quick Log section
            GameObject quickLogSection = CreatePanel(bgPanel.transform, "QuickLogSection", new Color(0.2f, 0.25f, 0.3f, 1f));
            RectTransform quickRect = quickLogSection.GetComponent<RectTransform>();
            quickRect.anchorMin = new Vector2(0.05f, 0.75f);
            quickRect.anchorMax = new Vector2(0.95f, 0.88f);

            CreateText(quickLogSection.transform, "QuickLabel", "Quick Log:", 20, TextAlignmentOptions.Left);

            // Quick log buttons row
            float buttonWidth = 0.23f;
            CreateButton(quickLogSection.transform, "BenchButton", "🏋️ Bench", 16, new Vector2(0.02f, 0.1f), new Vector2(buttonWidth, 0.9f));
            CreateButton(quickLogSection.transform, "SquatButton", "🦵 Squat", 16, new Vector2(0.27f, 0.1f), new Vector2(0.27f + buttonWidth, 0.9f));
            CreateButton(quickLogSection.transform, "DeadliftButton", "🦴 Deadlift", 16, new Vector2(0.52f, 0.1f), new Vector2(0.52f + buttonWidth, 0.9f));
            CreateButton(quickLogSection.transform, "RunButton", "🏃 Run 30m", 16, new Vector2(0.77f, 0.1f), new Vector2(0.98f, 0.9f));

            // Create Workout Input section
            GameObject inputSection = CreatePanel(bgPanel.transform, "InputSection", new Color(0.15f, 0.2f, 0.25f, 1f));
            RectTransform inputRect = inputSection.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.05f, 0.55f);
            inputRect.anchorMax = new Vector2(0.95f, 0.73f);

            CreateText(inputSection.transform, "MuscleLabel", "Muscle:", 18, new Vector2(0.02f, 0.5f), new Vector2(0.18f, 0.9f));
            CreateInputField(inputSection.transform, "MuscleDropdown", "Chest ▼", new Vector2(0.2f, 0.5f), new Vector2(0.48f, 0.9f));
            
            CreateText(inputSection.transform, "WeightLabel", "Weight (kg):", 18, new Vector2(0.5f, 0.5f), new Vector2(0.68f, 0.9f));
            CreateInputField(inputSection.transform, "WeightInput", "60", new Vector2(0.7f, 0.5f), new Vector2(0.85f, 0.9f));
            
            CreateText(inputSection.transform, "RepsLabel", "Reps:", 18, new Vector2(0.02f, 0.1f), new Vector2(0.15f, 0.45f));
            CreateInputField(inputSection.transform, "RepsInput", "10", new Vector2(0.17f, 0.1f), new Vector2(0.32f, 0.45f));
            
            CreateButton(inputSection.transform, "LogButton", "📝 LOG WORKOUT", 20, new Vector2(0.5f, 0.1f), new Vector2(0.98f, 0.45f));

            // Create Stats section
            GameObject statsSection = CreatePanel(bgPanel.transform, "StatsSection", new Color(0.12f, 0.15f, 0.2f, 1f));
            RectTransform statsRect = statsSection.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.05f, 0.08f);
            statsRect.anchorMax = new Vector2(0.95f, 0.53f);

            CreateText(statsSection.transform, "StatsTitle", "📊 YOUR STATS", 24, new Vector2(0, 0.88f), new Vector2(1, 1));

            // Muscle volume bars
            string[] muscles = { "Chest", "Back", "Shoulders", "Legs", "Biceps", "Triceps", "Core", "Cardio" };
            float yStart = 0.78f;
            float yStep = 0.1f;
            
            for (int i = 0; i < muscles.Length; i++)
            {
                float y = yStart - (i * yStep);
                CreateMuscleBar(statsSection.transform, muscles[i], y);
            }

            // Recovery and Streak
            CreateText(statsSection.transform, "RecoveryLabel", "❤️ Recovery: 100%", 20, new Vector2(0.02f, 0.02f), new Vector2(0.35f, 0.12f));
            CreateText(statsSection.transform, "StreakLabel", "🔥 Streak: 0 days", 20, new Vector2(0.4f, 0.02f), new Vector2(0.7f, 0.12f));
            CreateText(statsSection.transform, "AttackLabel", "⚔️ Power: 0", 20, new Vector2(0.72f, 0.02f), new Vector2(0.98f, 0.12f));

            // ============================================================
            // HEALTH TRACKING SECTION
            // ============================================================
            GameObject healthSection = CreatePanel(bgPanel.transform, "HealthSection", new Color(0.1f, 0.18f, 0.15f, 1f));
            RectTransform healthRect = healthSection.GetComponent<RectTransform>();
            healthRect.anchorMin = new Vector2(0.52f, 0.08f);
            healthRect.anchorMax = new Vector2(0.95f, 0.53f);
            
            // Adjust stats section to make room for health
            statsRect.anchorMax = new Vector2(0.48f, 0.53f);

            CreateText(healthSection.transform, "HealthTitle", "💚 HEALTH TRACKING", 22, new Vector2(0, 0.88f), new Vector2(1, 1));

            // Health input fields
            CreateText(healthSection.transform, "SleepLabel", "😴 Sleep (hrs):", 16, new Vector2(0.02f, 0.72f), new Vector2(0.45f, 0.82f));
            CreateInputField(healthSection.transform, "SleepInput", "8", new Vector2(0.5f, 0.72f), new Vector2(0.95f, 0.82f));

            CreateText(healthSection.transform, "WaterLabel", "💧 Water (L):", 16, new Vector2(0.02f, 0.58f), new Vector2(0.45f, 0.68f));
            CreateInputField(healthSection.transform, "WaterInput", "2.5", new Vector2(0.5f, 0.58f), new Vector2(0.95f, 0.68f));

            CreateText(healthSection.transform, "StepsLabel", "👟 Steps:", 16, new Vector2(0.02f, 0.44f), new Vector2(0.45f, 0.54f));
            CreateInputField(healthSection.transform, "StepsInput", "5000", new Vector2(0.5f, 0.44f), new Vector2(0.95f, 0.54f));

            CreateText(healthSection.transform, "HeartLabel", "❤️ Heart Rate:", 16, new Vector2(0.02f, 0.30f), new Vector2(0.45f, 0.40f));
            CreateInputField(healthSection.transform, "HeartInput", "72", new Vector2(0.5f, 0.30f), new Vector2(0.95f, 0.40f));

            CreateText(healthSection.transform, "WeightLabel", "⚖️ Weight (kg):", 16, new Vector2(0.02f, 0.16f), new Vector2(0.45f, 0.26f));
            CreateInputField(healthSection.transform, "WeightInput", "70", new Vector2(0.5f, 0.16f), new Vector2(0.95f, 0.26f));

            // Log Health button
            CreateButton(healthSection.transform, "LogHealthButton", "📝 LOG HEALTH", 18, new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.12f));

            // ============================================================
            // WORKOUT SESSION SECTION
            // ============================================================
            GameObject workoutSection = CreatePanel(bgPanel.transform, "WorkoutSessionSection", new Color(0.12f, 0.1f, 0.18f, 1f));
            RectTransform workoutRect = workoutSection.GetComponent<RectTransform>();
            workoutRect.anchorMin = new Vector2(0.05f, 0.55f);
            workoutRect.anchorMax = new Vector2(0.95f, 0.73f);

            // Move input section higher
            inputRect.anchorMin = new Vector2(0.05f, 0.37f);
            inputRect.anchorMax = new Vector2(0.95f, 0.53f);

            CreateText(workoutSection.transform, "WorkoutTitle", "🏋️ ACTIVE WORKOUT", 22, new Vector2(0, 0.75f), new Vector2(0.5f, 1f));

            // Timer display
            CreateText(workoutSection.transform, "WorkoutTimer", "⏱️ 0:00", 28, new Vector2(0.5f, 0.75f), new Vector2(1f, 1f));

            // Status text
            CreateText(workoutSection.transform, "WorkoutStatus", "💤 No workout in progress", 16, new Vector2(0.02f, 0.45f), new Vector2(0.6f, 0.7f));

            // Session stats
            CreateText(workoutSection.transform, "TotalVolume", "💪 0 kg", 16, new Vector2(0.6f, 0.55f), new Vector2(1f, 0.7f));
            CreateText(workoutSection.transform, "ExerciseCount", "📋 0 exercises", 16, new Vector2(0.6f, 0.4f), new Vector2(1f, 0.55f));

            // Control buttons row
            CreateButton(workoutSection.transform, "StartWorkoutBtn", "▶️ START", 16, new Vector2(0.02f, 0.05f), new Vector2(0.32f, 0.35f));
            CreateButton(workoutSection.transform, "StopWorkoutBtn", "⏹️ FINISH", 16, new Vector2(0.02f, 0.05f), new Vector2(0.32f, 0.35f));
            CreateButton(workoutSection.transform, "PauseWorkoutBtn", "⏸️ PAUSE", 16, new Vector2(0.34f, 0.05f), new Vector2(0.64f, 0.35f));
            CreateButton(workoutSection.transform, "AddExerciseBtn", "➕ ADD SET", 16, new Vector2(0.66f, 0.05f), new Vector2(0.98f, 0.35f));

            // Sets input (add to existing input section)
            CreateText(inputSection.transform, "SetsLabel", "Sets:", 18, new Vector2(0.35f, 0.1f), new Vector2(0.48f, 0.45f));
            CreateInputField(inputSection.transform, "SetsInput", "3", new Vector2(0.5f, 0.1f), new Vector2(0.65f, 0.45f));

            // Update log button text
            var logBtnTransform = inputSection.transform.Find("LogButton");
            if (logBtnTransform != null)
            {
                var btnText = logBtnTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = "📝 QUICK LOG";
            }

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
            img.color = new Color(0.2f, 0.25f, 0.3f, 1f);
            
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
            placeholderTMP.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            
            // Input text
            GameObject textGO = CreateText(textArea.transform, "Text", "", 16);
            
            TMP_InputField input = inputGO.AddComponent<TMP_InputField>();
            input.textComponent = textGO.GetComponent<TextMeshProUGUI>();
            input.placeholder = placeholderTMP;
            input.textViewport = textAreaRect;
            
            return inputGO;
        }

        private void CreateMuscleBar(Transform parent, string muscleName, float y)
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
            bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            
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
