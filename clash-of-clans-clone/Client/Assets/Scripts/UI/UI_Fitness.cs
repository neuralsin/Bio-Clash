namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Fitness Center UI Panel
    /// Matches the game's visual style for workout logging and stats display.
    /// </summary>
    public class UI_Fitness : MonoBehaviour
    {
        private static UI_Fitness _instance;
        public static UI_Fitness instanse { get { return _instance; } }

        [Header("Main Panel")]
        [SerializeField] private GameObject _panel = null;
        [SerializeField] private Button _closeButton = null;

        [Header("Workout Input")]
        [SerializeField] private TMP_Dropdown _exerciseDropdown = null;
        [SerializeField] private TMP_Dropdown _muscleDropdown = null;
        [SerializeField] private TMP_InputField _weightInput = null;
        [SerializeField] private TMP_InputField _repsInput = null;
        [SerializeField] private Button _logButton = null;

        [Header("Quick Log Buttons")]
        [SerializeField] private Button _quickBenchButton = null;
        [SerializeField] private Button _quickSquatButton = null;
        [SerializeField] private Button _quickDeadliftButton = null;
        [SerializeField] private Button _quickRunButton = null;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI _chestVolumeText = null;
        [SerializeField] private TextMeshProUGUI _backVolumeText = null;
        [SerializeField] private TextMeshProUGUI _legsVolumeText = null;
        [SerializeField] private TextMeshProUGUI _shouldersVolumeText = null;
        [SerializeField] private TextMeshProUGUI _bicepsVolumeText = null;
        [SerializeField] private TextMeshProUGUI _tricepsVolumeText = null;
        [SerializeField] private TextMeshProUGUI _coreVolumeText = null;
        [SerializeField] private TextMeshProUGUI _cardioVolumeText = null;

        [Header("Progress Bars")]
        [SerializeField] private Image _chestBar = null;
        [SerializeField] private Image _backBar = null;
        [SerializeField] private Image _legsBar = null;
        [SerializeField] private Image _shouldersBar = null;
        [SerializeField] private Image _bicepsBar = null;
        [SerializeField] private Image _tricepsBar = null;
        [SerializeField] private Image _coreBar = null;
        [SerializeField] private Image _cardioBar = null;

        [Header("Recovery & Streak")]
        [SerializeField] private TextMeshProUGUI _recoveryText = null;
        [SerializeField] private Image _recoveryBar = null;
        [SerializeField] private TextMeshProUGUI _streakText = null;
        [SerializeField] private TextMeshProUGUI _attackPowerText = null;
        [SerializeField] private TextMeshProUGUI _defensePowerText = null;

        [Header("Animation")]
        [SerializeField] private Animator _panelAnimator = null;

        private bool _isActive = false;
        public bool isActive { get { return _isActive; } }

        // Exercise presets for quick log
        private readonly Dictionary<string, (FitnessManager.MuscleGroup muscle, float weight, int reps)> quickExercises = 
            new Dictionary<string, (FitnessManager.MuscleGroup, float, int)>
        {
            { "Bench Press", (FitnessManager.MuscleGroup.Chest, 60, 10) },
            { "Squat", (FitnessManager.MuscleGroup.Legs, 80, 10) },
            { "Deadlift", (FitnessManager.MuscleGroup.Back, 100, 8) },
            { "Run 30min", (FitnessManager.MuscleGroup.Cardio, 30, 1) }
        };

        private void Awake()
        {
            _instance = this;
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void Start()
        {
            // Close button
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            // Log workout button
            if (_logButton != null)
                _logButton.onClick.AddListener(LogWorkout);

            // Quick log buttons
            if (_quickBenchButton != null)
                _quickBenchButton.onClick.AddListener(() => QuickLog("Bench Press"));
            if (_quickSquatButton != null)
                _quickSquatButton.onClick.AddListener(() => QuickLog("Squat"));
            if (_quickDeadliftButton != null)
                _quickDeadliftButton.onClick.AddListener(() => QuickLog("Deadlift"));
            if (_quickRunButton != null)
                _quickRunButton.onClick.AddListener(() => QuickLog("Run 30min"));

            // Populate dropdowns
            PopulateDropdowns();
        }

        private void PopulateDropdowns()
        {
            if (_exerciseDropdown != null)
            {
                _exerciseDropdown.ClearOptions();
                _exerciseDropdown.AddOptions(new List<string>
                {
                    "Bench Press", "Incline Press", "Dumbbell Fly",
                    "Deadlift", "Barbell Row", "Lat Pulldown", "Pull-ups",
                    "Squat", "Leg Press", "Lunges", "Leg Curl",
                    "Overhead Press", "Lateral Raise", "Face Pull",
                    "Bicep Curl", "Hammer Curl", "Preacher Curl",
                    "Tricep Pushdown", "Skull Crusher", "Dips",
                    "Plank", "Crunches", "Leg Raise",
                    "Running", "Cycling", "Rowing"
                });
            }

            if (_muscleDropdown != null)
            {
                _muscleDropdown.ClearOptions();
                _muscleDropdown.AddOptions(new List<string>
                {
                    "Chest", "Back", "Shoulders", "Biceps", "Triceps", "Legs", "Core", "Cardio"
                });
            }
        }

        public void Open()
        {
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
            _isActive = true;
            _panel.SetActive(true);
            
            if (_panelAnimator != null)
                _panelAnimator.SetTrigger("Open");

            RefreshStats();
            UI_Main.instanse.SetStatus(false);
        }

        public void Close()
        {
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
            _isActive = false;
            
            if (_panelAnimator != null)
                _panelAnimator.SetTrigger("Close");
            
            StartCoroutine(CloseAfterAnimation());
        }

        private IEnumerator CloseAfterAnimation()
        {
            yield return new WaitForSeconds(0.3f);
            _panel.SetActive(false);
            UI_Main.instanse.SetStatus(true);
        }

        private void LogWorkout()
        {
            if (FitnessManager.instance == null)
            {
                Debug.LogError("FitnessManager not initialized!");
                return;
            }

            // Get selected muscle
            int muscleIndex = _muscleDropdown != null ? _muscleDropdown.value : 0;
            FitnessManager.MuscleGroup muscle = (FitnessManager.MuscleGroup)muscleIndex;

            // Get weight and reps
            float weight = 0;
            int reps = 0;
            
            if (_weightInput != null)
                float.TryParse(_weightInput.text, out weight);
            if (_repsInput != null)
                int.TryParse(_repsInput.text, out reps);

            if (weight <= 0 || reps <= 0)
            {
                Debug.Log("Invalid weight or reps");
                return;
            }

            // Log the workout
            if (muscle == FitnessManager.MuscleGroup.Cardio)
            {
                FitnessManager.instance.LogCardio((int)weight); // Weight field used as minutes for cardio
            }
            else
            {
                FitnessManager.instance.LogWorkout(muscle, weight, reps);
            }

            // Clear inputs
            if (_weightInput != null) _weightInput.text = "";
            if (_repsInput != null) _repsInput.text = "";

            // Refresh stats display
            RefreshStats();

            // Play success sound
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);

            // Show success animation (could add particle effect here)
            Debug.Log($"✅ Logged: {weight}kg x {reps} reps ({muscle})");
        }

        private void QuickLog(string exercise)
        {
            if (!quickExercises.ContainsKey(exercise)) return;

            var preset = quickExercises[exercise];
            
            if (preset.muscle == FitnessManager.MuscleGroup.Cardio)
            {
                FitnessManager.instance.LogCardio((int)preset.weight);
            }
            else
            {
                FitnessManager.instance.LogWorkout(preset.muscle, preset.weight, preset.reps);
            }

            RefreshStats();
            SoundManager.instanse.PlaySound(SoundManager.instanse.buttonClickSound);
        }

        public void RefreshStats()
        {
            if (FitnessManager.instance == null) return;

            float maxVolume = 10000f; // For progress bar scaling

            // Update volume texts and bars for each muscle
            UpdateMuscleDisplay(_chestVolumeText, _chestBar, FitnessManager.MuscleGroup.Chest, maxVolume);
            UpdateMuscleDisplay(_backVolumeText, _backBar, FitnessManager.MuscleGroup.Back, maxVolume);
            UpdateMuscleDisplay(_legsVolumeText, _legsBar, FitnessManager.MuscleGroup.Legs, maxVolume);
            UpdateMuscleDisplay(_shouldersVolumeText, _shouldersBar, FitnessManager.MuscleGroup.Shoulders, maxVolume);
            UpdateMuscleDisplay(_bicepsVolumeText, _bicepsBar, FitnessManager.MuscleGroup.Biceps, maxVolume);
            UpdateMuscleDisplay(_tricepsVolumeText, _tricepsBar, FitnessManager.MuscleGroup.Triceps, maxVolume);
            UpdateMuscleDisplay(_coreVolumeText, _coreBar, FitnessManager.MuscleGroup.Core, maxVolume);
            
            // Cardio uses minutes
            if (_cardioVolumeText != null)
                _cardioVolumeText.text = $"{FitnessManager.instance.GetMuscleVolume(FitnessManager.MuscleGroup.Cardio):N0} min";
            if (_cardioBar != null)
                _cardioBar.fillAmount = FitnessManager.instance.GetMuscleVolume(FitnessManager.MuscleGroup.Cardio) / 500f;

            // Recovery
            if (_recoveryText != null)
                _recoveryText.text = $"{FitnessManager.instance.recoveryScore}%";
            if (_recoveryBar != null)
                _recoveryBar.fillAmount = FitnessManager.instance.recoveryScore / 100f;

            // Streak
            if (_streakText != null)
                _streakText.text = $"🔥 {FitnessManager.instance.workoutStreak} days";

            // Attack/Defense Power
            if (_attackPowerText != null)
                _attackPowerText.text = $"⚔️ {FitnessManager.instance.GetAttackPower():N0}";
            if (_defensePowerText != null)
                _defensePowerText.text = $"🛡️ {FitnessManager.instance.GetDefensePower():N0}";
        }

        private void UpdateMuscleDisplay(TextMeshProUGUI text, Image bar, FitnessManager.MuscleGroup muscle, float maxVolume)
        {
            float volume = FitnessManager.instance.GetMuscleVolume(muscle);
            
            if (text != null)
                text.text = $"{volume:N0} kg";
            if (bar != null)
                bar.fillAmount = Mathf.Clamp01(volume / maxVolume);
        }

        // Update is called once per frame (for animations if needed)
        private void Update()
        {
            if (_isActive)
            {
                // Could add real-time updates here
            }
        }
    }
}
