#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace DevelopersHub.ClashOfWhatecer
{
    /// <summary>
    /// Editor menu items for Bio-Clash Fitness setup.
    /// Provides one-click setup for hackathon demo.
    /// </summary>
    public class FitnessEditorSetup
    {
        [MenuItem("Bio-Clash/Setup Fitness System")]
        public static void SetupFitnessSystem()
        {
            // 1. Create FitnessManager
            if (Object.FindObjectOfType<FitnessManager>() == null)
            {
                GameObject fitnessManager = new GameObject("FitnessManager");
                fitnessManager.AddComponent<FitnessManager>();
                Undo.RegisterCreatedObjectUndo(fitnessManager, "Create FitnessManager");
                Debug.Log("✅ Created FitnessManager");
            }
            else
            {
                Debug.Log("FitnessManager already exists");
            }

            // 2. Create FitnessSetup (creates UI at runtime)
            if (Object.FindObjectOfType<FitnessSetup>() == null)
            {
                GameObject fitnessSetup = new GameObject("FitnessSetup");
                FitnessSetup setup = fitnessSetup.AddComponent<FitnessSetup>();
                setup.createFitnessManager = true;
                setup.createFitnessUI = true;
                Undo.RegisterCreatedObjectUndo(fitnessSetup, "Create FitnessSetup");
                Debug.Log("✅ Created FitnessSetup");
            }
            else
            {
                Debug.Log("FitnessSetup already exists");
            }

            // 3. Find UI_Main and ensure fitness button
            UI_Main uiMain = Object.FindObjectOfType<UI_Main>();
            if (uiMain != null)
            {
                Debug.Log("✅ UI_Main found - assign _fitnessButton in Inspector");
            }
            else
            {
                Debug.LogWarning("UI_Main not found in scene");
            }

            EditorUtility.DisplayDialog("Bio-Clash Setup", 
                "Fitness system setup complete!\n\n" +
                "Next steps:\n" +
                "1. Assign _fitnessButton in UI_Main Inspector\n" +
                "2. Run the game to see Fitness UI\n" +
                "3. Run fitness_tables.sql on your MySQL database", 
                "OK");
        }

        [MenuItem("Bio-Clash/Create Fitness Button")]
        public static void CreateFitnessButton()
        {
            UI_Main uiMain = Object.FindObjectOfType<UI_Main>();
            if (uiMain == null)
            {
                EditorUtility.DisplayDialog("Error", "UI_Main not found in scene", "OK");
                return;
            }

            // Find an existing button to duplicate style
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "Canvas not found in scene", "OK");
                return;
            }

            // Create fitness button
            GameObject btnGO = new GameObject("FitnessButton");
            btnGO.transform.SetParent(canvas.transform, false);

            RectTransform rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(100, 0);
            rect.sizeDelta = new Vector2(150, 60);

            UnityEngine.UI.Image img = btnGO.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.2f, 0.8f, 0.4f, 1f);

            UnityEngine.UI.Button btn = btnGO.AddComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = img;

            // Add text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TMPro.TextMeshProUGUI tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = "💪 FITNESS";
            tmp.fontSize = 20;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;

            Undo.RegisterCreatedObjectUndo(btnGO, "Create Fitness Button");

            // Select the button
            Selection.activeGameObject = btnGO;

            EditorUtility.DisplayDialog("Created Fitness Button", 
                "Fitness button created!\n\n" +
                "Now drag this button to the _fitnessButton field in UI_Main", 
                "OK");
        }

        [MenuItem("Bio-Clash/Add Demo Workout Data")]
        public static void AddDemoWorkoutData()
        {
            FitnessManager fm = Object.FindObjectOfType<FitnessManager>();
            if (fm == null)
            {
                // Create if not exists
                GameObject go = new GameObject("FitnessManager");
                fm = go.AddComponent<FitnessManager>();
            }

            // Add demo workout data
            fm.muscleVolumes[0] = 1200; // Chest
            fm.muscleVolumes[1] = 800;  // Back
            fm.muscleVolumes[2] = 600;  // Shoulders
            fm.muscleVolumes[3] = 400;  // Biceps
            fm.muscleVolumes[4] = 350;  // Triceps
            fm.muscleVolumes[5] = 2500; // Legs
            fm.muscleVolumes[6] = 500;  // Core
            fm.muscleVolumes[7] = 150;  // Cardio minutes
            fm.workoutStreak = 12;
            fm.recoveryScore = 85;

            Debug.Log("✅ Added demo workout data for presentation");
            EditorUtility.DisplayDialog("Demo Data Added", 
                "Added realistic workout data:\n\n" +
                "• Chest: 1200 kg\n" +
                "• Back: 800 kg\n" +
                "• Legs: 2500 kg\n" +
                "• Streak: 12 days\n" +
                "• Recovery: 85%", 
                "OK");
        }
    }
}
#endif
