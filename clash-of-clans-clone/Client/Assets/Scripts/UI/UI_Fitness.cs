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
        public static UI_Fitness instance { get { return _instance; } }

        [Header("Main Panel")]
        [SerializeField] public GameObject _panel = null;
        [SerializeField] public Button _closeButton = null;

        [Header("Workout Input")]
        [SerializeField] public TMP_InputField _exerciseInput = null; // Changed from Dropdown
        [SerializeField] public TMP_InputField _muscleInput = null;   // Changed from Dropdown
        [SerializeField] public TMP_InputField _weightInput = null;
        [SerializeField] public TMP_InputField _repsInput = null;
        [SerializeField] public Button _logButton = null;

        [Header("Quick Log Buttons")]
        [SerializeField] public Button _quickBenchButton = null;
        [SerializeField] public Button _quickSquatButton = null;
        [SerializeField] public Button _quickDeadliftButton = null;
        [SerializeField] public Button _quickRunButton = null;

        [Header("Stats Display")]
        [SerializeField] public TextMeshProUGUI _chestVolumeText = null;
        [SerializeField] public TextMeshProUGUI _backVolumeText = null;
        [SerializeField] public TextMeshProUGUI _legsVolumeText = null;
        [SerializeField] public TextMeshProUGUI _shouldersVolumeText = null;
        [SerializeField] public TextMeshProUGUI _bicepsVolumeText = null;
        [SerializeField] public TextMeshProUGUI _tricepsVolumeText = null;
        [SerializeField] public TextMeshProUGUI _coreVolumeText = null;
        [SerializeField] public TextMeshProUGUI _cardioVolumeText = null;

        [Header("Progress Bars")]
        [SerializeField] public Image _chestBar = null;
        [SerializeField] public Image _backBar = null;
        [SerializeField] public Image _legsBar = null;
        [SerializeField] public Image _shouldersBar = null;
        [SerializeField] public Image _bicepsBar = null;
        [SerializeField] public Image _tricepsBar = null;
        [SerializeField] public Image _coreBar = null;
        [SerializeField] public Image _cardioBar = null;

        [Header("Recovery & Streak")]
        [SerializeField] public TextMeshProUGUI _recoveryText = null;
        [SerializeField] public Image _recoveryBar = null;
        [SerializeField] public TextMeshProUGUI _streakText = null;
        [SerializeField] public TextMeshProUGUI _attackPowerText = null;
        [SerializeField] public TextMeshProUGUI _defensePowerText = null;

        [Header("Health Tracking")]
        [SerializeField] public TMP_InputField _sleepHoursInput = null;
        [SerializeField] public TMP_InputField _waterLitersInput = null;
        [SerializeField] public TMP_InputField _stepsInput = null;
        [SerializeField] public TMP_InputField _heartRateInput = null;
        [SerializeField] public TMP_InputField _bodyWeightInput = null;
        [SerializeField] public Button _logHealthButton = null;

        [Header("Health Display")]
        [SerializeField] public TextMeshProUGUI _sleepText = null;
        [SerializeField] public Image _sleepBar = null;
        [SerializeField] public TextMeshProUGUI _waterText = null;
        [SerializeField] public Image _waterBar = null;
        [SerializeField] public TextMeshProUGUI _stepsText = null;
        [SerializeField] public Image _stepsBar = null;
        [SerializeField] public TextMeshProUGUI _heartRateText = null;
        [SerializeField] public TextMeshProUGUI _bodyWeightText = null;
        [SerializeField] public TextMeshProUGUI _healthScoreText = null;

        [Header("Workout Session")]
        [SerializeField] public GameObject _workoutSessionPanel = null;
        [SerializeField] public Button _startWorkoutButton = null;
        [SerializeField] public Button _stopWorkoutButton = null;
        [SerializeField] public Button _pauseWorkoutButton = null;
        [SerializeField] public TextMeshProUGUI _workoutTimerText = null;
        [SerializeField] public TextMeshProUGUI _workoutStatusText = null;
        [SerializeField] public Transform _exerciseListContainer = null;
        [SerializeField] public GameObject _exerciseItemPrefab = null;
        [SerializeField] public Button _addExerciseButton = null;
        [SerializeField] public TMP_InputField _setsInput = null;
        [SerializeField] public TextMeshProUGUI _totalVolumeText = null;
        [SerializeField] public TextMeshProUGUI _exerciseCountText = null;

        [Header("Animation")]
        [SerializeField] private Animator _panelAnimator = null;

        private bool _isActive = false;
        public bool isActive { get { return _isActive; } }

        // Health tracking data
        private float _todaySleepHours = 0f;
        private float _todayWaterLiters = 0f;
        private int _todaySteps = 0;
        private int _currentHeartRate = 0;
        private float _bodyWeight = 70f;

        // ============================================================
        // WORKOUT SESSION TRACKING
        // ============================================================
        private bool _isWorkoutActive = false;
        private bool _isWorkoutPaused = false;
        private float _workoutStartTime = 0f;
        private float _workoutDuration = 0f;
        private float _pausedDuration = 0f;
        private float _pauseStartTime = 0f;
        
        // Exercise tracking within session
        [System.Serializable]
        public class WorkoutExercise
        {
            public string exerciseName;
            public FitnessManager.MuscleGroup muscleGroup;
            public float weight;
            public int reps;
            public int sets;
            public float volume; // weight * reps * sets
            public System.DateTime timestamp;
        }
        
        private List<WorkoutExercise> _currentWorkoutExercises = new List<WorkoutExercise>();
        private float _sessionTotalVolume = 0f;

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

        private void OnEnable()
        {
            if (_panel != null)
            {
                StartCoroutine(AnimatePopIn());
            }
        }

        private IEnumerator AnimatePopIn()
        {
            _panel.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            
            // Phase 1: 0.7 -> 1.1 (Overshoot)
            float timer = 0f;
            float duration = 0.2f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                float scale = Mathf.Lerp(0.7f, 1.1f, t);
                _panel.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            // Phase 2: 1.1 -> 1.0 (Settle)
            timer = 0f;
            duration = 0.1f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                float scale = Mathf.Lerp(1.1f, 1.0f, t);
                _panel.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            _panel.transform.localScale = Vector3.one;
        }

        private IEnumerator PunchScale(Transform target)
        {
            Vector3 original = Vector3.one;
            float duration = 0.15f;
            float time = 0;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float scale = 1.0f + Mathf.Sin(t * Mathf.PI) * 0.05f; // 5% punch
                target.localScale = original * scale;
                yield return null;
            }
            target.localScale = original;
        }

        private void Start()
        {
            RefreshStats();
            
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
            else
                Debug.LogWarning("UI_Fitness: Close Button not assigned!");

            if (_logButton != null)
                _logButton.onClick.AddListener(AddExerciseToSession);

            if (_quickBenchButton != null) _quickBenchButton.onClick.AddListener(() => QuickLog("Bench Press"));
            if (_quickSquatButton != null) _quickSquatButton.onClick.AddListener(() => QuickLog("Squat"));
            if (_quickDeadliftButton != null) _quickDeadliftButton.onClick.AddListener(() => QuickLog("Deadlift"));
            if (_quickRunButton != null) _quickRunButton.onClick.AddListener(() => QuickLog("Run"));
            
            // Workout Session Listeners
            if (_startWorkoutButton != null) _startWorkoutButton.onClick.AddListener(StartWorkout);
            if (_stopWorkoutButton != null) _stopWorkoutButton.onClick.AddListener(StopWorkout);
            if (_pauseWorkoutButton != null) _pauseWorkoutButton.onClick.AddListener(TogglePauseWorkout);
            if (_addExerciseButton != null) _addExerciseButton.onClick.AddListener(AddExerciseToSession);
            
            LoadHealthData();

            // Initialize workout session UI
            UpdateWorkoutSessionUI();
        }

        private void PopulateDropdowns()
        {
            if (_exerciseInput != null)
            {
                _exerciseInput.text = ""; // Clear input field
            }

            if (_muscleInput != null)
            {
                _muscleInput.text = ""; // Clear input field
            }
        }

        public void Open()
        {
            _panel.SetActive(true);
            _isActive = true;
            RefreshStats();
        }

        public void Close()
        {
            _panel.SetActive(false);
            _isActive = false;
        }

        private IEnumerator CloseAfterAnimation()
        {
            yield return new WaitForSeconds(0.3f);
            _panel.SetActive(false);
            UI_Main.instance.SetStatus(true);
        }

        private void LogWorkout()
        {
            if (FitnessManager.instance == null)
            {
                Debug.LogError("FitnessManager not initialized!");
                return;
            }

            float weight = 0;
            if (_weightInput != null && !string.IsNullOrEmpty(_weightInput.text))
            {
                float.TryParse(_weightInput.text, out weight);
            }

            int reps = 0;
            if (_repsInput != null && !string.IsNullOrEmpty(_repsInput.text))
            {
                int.TryParse(_repsInput.text, out reps);
            }

            // Get exercise name from input
            string exerciseName = _exerciseInput != null ? _exerciseInput.text : "";
            
            // Try auto-detection first
            FitnessManager.MuscleGroup muscle = FitnessManager.MuscleGroup.Chest;
            bool autoDetected = false;

            if (!string.IsNullOrEmpty(exerciseName))
            {
                var detected = FitnessManager.instance.LogExercise(exerciseName, weight, reps);
                if (detected.HasValue)
                {
                    muscle = detected.Value;
                    autoDetected = true;
                }
            }

            if (!autoDetected)
            {
                // Fallback to manual muscle input
                string muscleName = _muscleInput != null ? _muscleInput.text : "Chest";
                // Simple parsing or default
                try {
                    muscle = (FitnessManager.MuscleGroup)System.Enum.Parse(typeof(FitnessManager.MuscleGroup), muscleName, true);
                    FitnessManager.instance.LogWorkout(muscle, weight, reps);
                } catch {
                    Debug.Log($"⚠️ Could not parse muscle '{muscleName}', defaulting to Chest");
                    FitnessManager.instance.LogWorkout(FitnessManager.MuscleGroup.Chest, weight, reps);
                }
            }

            // Clear inputs
            if (_weightInput != null) _weightInput.text = "";
            if (_repsInput != null) _repsInput.text = "";
            if (_exerciseInput != null) _exerciseInput.text = "";

            // Refresh stats display
            RefreshStats();

            // Play success sound
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            
            StartCoroutine(PunchScale(_panel.transform)); // Punch whole panel
            
            Debug.Log($"✅ Logged: {weight}kg x {reps} reps");
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
            
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
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
                _streakText.text = $"[FIRE] {FitnessManager.instance.workoutStreak} days";

            // Attack/Defense Power
            if (_attackPowerText != null)
                _attackPowerText.text = $"[ATK] {FitnessManager.instance.GetAttackPower():N0}";
            if (_defensePowerText != null)
                _defensePowerText.text = $"[DEF] {FitnessManager.instance.GetDefensePower():N0}";

            // Health metrics
            RefreshHealthDisplay();
        }

        private void UpdateMuscleDisplay(TextMeshProUGUI text, Image bar, FitnessManager.MuscleGroup muscle, float maxVolume)
        {
            float volume = FitnessManager.instance.GetMuscleVolume(muscle);
            
            if (text != null)
                text.text = $"{volume:N0} kg";
            if (bar != null)
                bar.fillAmount = Mathf.Clamp01(volume / maxVolume);
        }

        // ============================================================
        // HEALTH TRACKING
        // ============================================================

        /// <summary>
        /// Log health metrics (sleep, water, steps, heart rate, weight).
        /// </summary>
        private void LogHealth()
        {
            // Parse input values
            if (_sleepHoursInput != null && !string.IsNullOrEmpty(_sleepHoursInput.text))
            {
                float.TryParse(_sleepHoursInput.text, out _todaySleepHours);
                _sleepHoursInput.text = "";
            }

            if (_waterLitersInput != null && !string.IsNullOrEmpty(_waterLitersInput.text))
            {
                float.TryParse(_waterLitersInput.text, out _todayWaterLiters);
                _waterLitersInput.text = "";
            }

            if (_stepsInput != null && !string.IsNullOrEmpty(_stepsInput.text))
            {
                int.TryParse(_stepsInput.text, out _todaySteps);
                _stepsInput.text = "";
            }

            if (_heartRateInput != null && !string.IsNullOrEmpty(_heartRateInput.text))
            {
                int.TryParse(_heartRateInput.text, out _currentHeartRate);
                _heartRateInput.text = "";
            }

            if (_bodyWeightInput != null && !string.IsNullOrEmpty(_bodyWeightInput.text))
            {
                float.TryParse(_bodyWeightInput.text, out _bodyWeight);
                _bodyWeightInput.text = "";
            }

            // Save and refresh
            SaveHealthData();
            RefreshHealthDisplay();

            // Update recovery based on health
            UpdateRecoveryFromHealth();

            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            Debug.Log($"💚 Health logged: Sleep {_todaySleepHours}h, Water {_todayWaterLiters}L, Steps {_todaySteps}");
        }

        /// <summary>
        /// Refresh health metrics display.
        /// </summary>
        private void RefreshHealthDisplay()
        {
            // Sleep (target: 8 hours)
            if (_sleepText != null)
                _sleepText.text = $"😴 {_todaySleepHours:F1}h / 8h";
            if (_sleepBar != null)
                _sleepBar.fillAmount = Mathf.Clamp01(_todaySleepHours / 8f);

            // Water (target: 3 liters)
            if (_waterText != null)
                _waterText.text = $"💧 {_todayWaterLiters:F1}L / 3L";
            if (_waterBar != null)
                _waterBar.fillAmount = Mathf.Clamp01(_todayWaterLiters / 3f);

            // Steps (target: 10,000)
            if (_stepsText != null)
                _stepsText.text = $"👟 {_todaySteps:N0} / 10,000";
            if (_stepsBar != null)
                _stepsBar.fillAmount = Mathf.Clamp01(_todaySteps / 10000f);

            // Heart rate
            if (_heartRateText != null)
            {
                string hrEmoji = _currentHeartRate < 60 ? "[LOW]" : (_currentHeartRate > 100 ? "[HIGH]" : "[OK]");
                _heartRateText.text = $"{hrEmoji} {_currentHeartRate} BPM";
            }

            // Body weight
            if (_bodyWeightText != null)
                _bodyWeightText.text = $"⚖️ {_bodyWeight:F1} kg";

            // Health score
            if (_healthScoreText != null)
            {
                int score = CalculateHealthScore();
                string scoreEmoji = score >= 80 ? "🌟" : (score >= 50 ? "👍" : "⚠️");
                _healthScoreText.text = $"{scoreEmoji} Health: {score}/100";
            }
        }

        /// <summary>
        /// Calculate overall health score from metrics.
        /// </summary>
        private int CalculateHealthScore()
        {
            int score = 0;

            // Sleep score (0-25 points)
            score += Mathf.RoundToInt(Mathf.Clamp01(_todaySleepHours / 8f) * 25);

            // Water score (0-25 points)
            score += Mathf.RoundToInt(Mathf.Clamp01(_todayWaterLiters / 3f) * 25);

            // Steps score (0-25 points)
            score += Mathf.RoundToInt(Mathf.Clamp01(_todaySteps / 10000f) * 25);

            // Heart rate score (0-25 points) - optimal is 60-80 BPM resting
            if (_currentHeartRate >= 50 && _currentHeartRate <= 90)
                score += 25;
            else if (_currentHeartRate > 0)
                score += 10;

            return score;
        }

        /// <summary>
        /// Update recovery score based on health metrics.
        /// Good sleep and hydration improve recovery.
        /// </summary>
        private void UpdateRecoveryFromHealth()
        {
            if (FitnessManager.instance == null) return;

            int healthScore = CalculateHealthScore();

            // Health score above 70 improves recovery
            if (healthScore >= 70)
            {
                FitnessManager.instance.recoveryScore = Mathf.Min(100, 
                    FitnessManager.instance.recoveryScore + (healthScore - 50) / 5);
            }
        }

        /// <summary>
        /// Save health data to PlayerPrefs.
        /// </summary>
        private void SaveHealthData()
        {
            PlayerPrefs.SetFloat("health_sleep", _todaySleepHours);
            PlayerPrefs.SetFloat("health_water", _todayWaterLiters);
            PlayerPrefs.SetInt("health_steps", _todaySteps);
            PlayerPrefs.SetInt("health_heartrate", _currentHeartRate);
            PlayerPrefs.SetFloat("health_weight", _bodyWeight);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load health data from PlayerPrefs.
        /// </summary>
        private void LoadHealthData()
        {
            _todaySleepHours = PlayerPrefs.GetFloat("health_sleep", 0);
            _todayWaterLiters = PlayerPrefs.GetFloat("health_water", 0);
            _todaySteps = PlayerPrefs.GetInt("health_steps", 0);
            _currentHeartRate = PlayerPrefs.GetInt("health_heartrate", 0);
            _bodyWeight = PlayerPrefs.GetFloat("health_weight", 70f);
        }

        // Update is called once per frame (for animations if needed)
        private void Update()
        {
            if (_isActive)
            {
                // Update workout timer if active
                if (_isWorkoutActive && !_isWorkoutPaused)
                {
                    _workoutDuration = Time.time - _workoutStartTime - _pausedDuration;
                    UpdateWorkoutTimerDisplay();
                }
            }
        }

        // ============================================================
        // WORKOUT SESSION CONTROL
        // ============================================================

        /// <summary>
        /// Start a new workout session.
        /// </summary>
        private void StartWorkout()
        {
            if (_isWorkoutActive)
            {
                Debug.Log("⚠️ Workout already in progress!");
                return;
            }

            _isWorkoutActive = true;
            _isWorkoutPaused = false;
            _workoutStartTime = Time.time;
            _workoutDuration = 0f;
            _pausedDuration = 0f;
            _currentWorkoutExercises.Clear();
            _sessionTotalVolume = 0f;

            UpdateWorkoutSessionUI();
            
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            Debug.Log("[GYM] Workout started!");
        }

        /// <summary>
        /// Stop and finish the current workout session.
        /// </summary>
        private void StopWorkout()
        {
            if (!_isWorkoutActive)
            {
                Debug.Log("⚠️ No workout in progress!");
                return;
            }

            _isWorkoutActive = false;
            _isWorkoutPaused = false;

            // Log all exercises from session
            foreach (var exercise in _currentWorkoutExercises)
            {
                // Already logged individually, just finalize
                Debug.Log($"📝 Logged: {exercise.exerciseName} - {exercise.sets}x{exercise.reps} @ {exercise.weight}kg");
            }

            // Show workout summary
            int minutes = Mathf.FloorToInt(_workoutDuration / 60);
            int seconds = Mathf.FloorToInt(_workoutDuration % 60);
            
            if (_workoutStatusText != null)
                _workoutStatusText.text = $"✅ Workout Complete!\n{minutes}:{seconds:D2} | {_currentWorkoutExercises.Count} exercises | {_sessionTotalVolume:N0}kg";

            UpdateWorkoutSessionUI();
            
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            Debug.Log($"✅ Workout finished! Duration: {minutes}:{seconds:D2}, Volume: {_sessionTotalVolume}kg");
        }

        /// <summary>
        /// Toggle pause/resume for current workout.
        /// </summary>
        private void TogglePauseWorkout()
        {
            if (!_isWorkoutActive) return;

            if (_isWorkoutPaused)
            {
                // Resume
                _pausedDuration += Time.time - _pauseStartTime;
                _isWorkoutPaused = false;
                if (_workoutStatusText != null)
                    _workoutStatusText.text = "[GYM] Workout Active";
                Debug.Log("▶️ Workout resumed");
            }
            else
            {
                // Pause
                _pauseStartTime = Time.time;
                _isWorkoutPaused = true;
                if (_workoutStatusText != null)
                    _workoutStatusText.text = "[PAUSE] Paused";
                Debug.Log("[PAUSE] Workout paused");
            }

            UpdateWorkoutSessionUI();
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
        }

        /// <summary>
        /// Add an exercise to the current workout session.
        /// Uses the exercise dropdown, weight, reps, and sets inputs.
        /// </summary>
        private void AddExerciseToSession()
        {
            if (!_isWorkoutActive)
            {
                Debug.Log("⚠️ Start a workout first!");
                return;
            }

            // Get exercise name from input field
            string exerciseName = "Custom Exercise";
            if (_exerciseInput != null && !string.IsNullOrEmpty(_exerciseInput.text))
                exerciseName = _exerciseInput.text;

            // Get weight
            float weight = 0;
            if (_weightInput != null && !string.IsNullOrEmpty(_weightInput.text))
                float.TryParse(_weightInput.text, out weight);

            // Get reps
            int reps = 0;
            if (_repsInput != null && !string.IsNullOrEmpty(_repsInput.text))
                int.TryParse(_repsInput.text, out reps);

            // Get sets
            int sets = 1;
            if (_setsInput != null && !string.IsNullOrEmpty(_setsInput.text))
                int.TryParse(_setsInput.text, out sets);

            if (weight <= 0 || reps <= 0)
            {
                Debug.Log("⚠️ Please enter weight and reps!");
                return;
            }

            // Auto-detect muscle group from exercise name
            FitnessManager.MuscleGroup muscleGroup = FitnessManager.MuscleGroup.Chest;
            if (FitnessManager.ExerciseToMuscle.TryGetValue(exerciseName, out var detected))
                muscleGroup = detected;
            else if (_muscleInput != null && !string.IsNullOrEmpty(_muscleInput.text))
            {
                // Try parsing muscle name from input
                try
                {
                    muscleGroup = (FitnessManager.MuscleGroup)System.Enum.Parse(typeof(FitnessManager.MuscleGroup), _muscleInput.text, true);
                }
                catch { /* Keep default */ }
            }

            // Calculate volume
            float volume = weight * reps * sets;

            // Create exercise entry
            var exercise = new WorkoutExercise
            {
                exerciseName = exerciseName,
                muscleGroup = muscleGroup,
                weight = weight,
                reps = reps,
                sets = sets,
                volume = volume,
                timestamp = System.DateTime.Now
            };

            _currentWorkoutExercises.Add(exercise);
            _sessionTotalVolume += volume;

            // Log to FitnessManager (each set)
            for (int i = 0; i < sets; i++)
            {
                FitnessManager.instance.LogWorkout(muscleGroup, weight, reps);
            }

            // Clear inputs
            if (_weightInput != null) _weightInput.text = "";
            if (_repsInput != null) _repsInput.text = "";
            if (_setsInput != null) _setsInput.text = "";

            // Update UI
            RefreshStats();
            UpdateExerciseList();
            UpdateWorkoutSessionUI();

            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
                
            StartCoroutine(PunchScale(_logButton.transform)); // Punch log button
            
            Debug.Log($"➕ Added: {exerciseName} - {sets}x{reps} @ {weight}kg ({muscleGroup})");
        }

        /// <summary>
        /// Update the workout timer display.
        /// </summary>
        private void UpdateWorkoutTimerDisplay()
        {
            if (_workoutTimerText == null) return;

            int hours = Mathf.FloorToInt(_workoutDuration / 3600);
            int minutes = Mathf.FloorToInt((_workoutDuration % 3600) / 60);
            int seconds = Mathf.FloorToInt(_workoutDuration % 60);

            if (hours > 0)
                _workoutTimerText.text = $"[TIME] {hours}:{minutes:D2}:{seconds:D2}";
            else
                _workoutTimerText.text = $"[TIME] {minutes}:{seconds:D2}";
        }

        /// <summary>
        /// Update workout session UI elements.
        /// </summary>
        private void UpdateWorkoutSessionUI()
        {
            // Show/hide buttons based on state
            if (_startWorkoutButton != null)
                _startWorkoutButton.gameObject.SetActive(!_isWorkoutActive);
            if (_stopWorkoutButton != null)
                _stopWorkoutButton.gameObject.SetActive(_isWorkoutActive);
            if (_pauseWorkoutButton != null)
            {
                _pauseWorkoutButton.gameObject.SetActive(_isWorkoutActive);
                var btnText = _pauseWorkoutButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = _isWorkoutPaused ? "[>] Resume" : "[||] Pause";
            }
            if (_addExerciseButton != null)
                _addExerciseButton.gameObject.SetActive(_isWorkoutActive);

            // Update status
            if (_workoutStatusText != null)
            {
                if (!_isWorkoutActive)
                    _workoutStatusText.text = "[ZZZ] No workout in progress";
                else if (_isWorkoutPaused)
                    _workoutStatusText.text = "[||] Paused";
                else
                    _workoutStatusText.text = "[GYM] Workout Active";
            }

            // Update totals
            if (_totalVolumeText != null)
                _totalVolumeText.text = $"[*] {_sessionTotalVolume:N0} kg";
            if (_exerciseCountText != null)
                _exerciseCountText.text = $"[#] {_currentWorkoutExercises.Count} exercises";
        }

        /// <summary>
        /// Update the exercise list display during workout.
        /// </summary>
        private void UpdateExerciseList()
        {
            if (_exerciseListContainer == null) return;

            // Clear existing items (except prefab)
            foreach (Transform child in _exerciseListContainer)
            {
                if (child.gameObject != _exerciseItemPrefab)
                    Destroy(child.gameObject);
            }

            // Create items for each exercise (show last 5)
            int startIndex = Mathf.Max(0, _currentWorkoutExercises.Count - 5);
            for (int i = startIndex; i < _currentWorkoutExercises.Count; i++)
            {
                var ex = _currentWorkoutExercises[i];
                CreateExerciseListItem(ex, i + 1);
            }
        }

        /// <summary>
        /// Create a visual item for an exercise in the list.
        /// </summary>
        private void CreateExerciseListItem(WorkoutExercise exercise, int index)
        {
            if (_exerciseListContainer == null) return;

            // Create text item
            GameObject item = new GameObject($"Exercise_{index}");
            item.transform.SetParent(_exerciseListContainer, false);

            var rect = item.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 30);
            rect.anchoredPosition = new Vector2(0, -(index - 1) * 35);

            var text = item.AddComponent<TextMeshProUGUI>();
            text.text = $"{index}. {exercise.exerciseName} - {exercise.sets}x{exercise.reps} @ {exercise.weight}kg ({exercise.muscleGroup})";
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
        }

        /// <summary>
        /// Get workout session data for external use.
        /// </summary>
        public List<WorkoutExercise> GetCurrentWorkoutExercises()
        {
            return _currentWorkoutExercises;
        }

        public bool IsWorkoutActive()
        {
            return _isWorkoutActive;
        }

        public float GetWorkoutDuration()
        {
            return _workoutDuration;
        }
    }
}
