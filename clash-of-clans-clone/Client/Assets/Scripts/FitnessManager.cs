namespace DevelopersHub.ClashOfWhatecer
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using DevelopersHub.RealtimeNetworking.Client;

    /// <summary>
    /// Manages all fitness-related data and gameplay mechanics.
    /// "Your Body Builds Your Base" - Volume lifted determines upgrade eligibility.
    /// </summary>
    public class FitnessManager : MonoBehaviour
    {
        private static FitnessManager _instance;
        public static FitnessManager instance { get { return _instance; } }

        // ============================================================
        // MUSCLE GROUPS & VOLUME TRACKING
        // ============================================================
        public enum MuscleGroup
        {
            Chest = 0,
            Back = 1,
            Shoulders = 2,
            Biceps = 3,
            Triceps = 4,
            Legs = 5,
            Core = 6,
            Cardio = 7 // Measured in minutes, not kg
        }

        // Total volume lifted per muscle group (in kg)
        public float[] muscleVolumes = new float[8];
        
        // Today's workout volume
        public float[] todayVolumes = new float[8];
        
        // Recovery score (0-100) - affects builder speed
        public int recoveryScore = 100;
        
        // Workout streak (consecutive days)
        public int workoutStreak = 0;
        
        // Last workout date
        public DateTime lastWorkoutDate = DateTime.MinValue;

        // ============================================================
        // THE CODEX: One Body Part → One Building Type (1:1 Mapping)
        // ============================================================
        // 
        // CHEST     → Archer Tower   (upper push power)
        // BACK      → Cannon         (foundation strength)
        // SHOULDERS → Wizard Tower   (overhead stability)
        // BICEPS    → Hidden Tesla   (quick precision)
        // TRICEPS   → Mortar         (pushing force)
        // LEGS      → Inferno Tower  (power base)
        // CORE      → Walls          (stability foundation)
        // CARDIO    → X-Bow          (endurance targeting)
        // 
        // Additional mappings:
        // Air Defense → Shoulders (sharing)
        // Bomb Tower → Biceps
        // Air Sweeper → Back
        //
        public static readonly Dictionary<Data.BuildingID, MuscleGroup> BuildingToMuscle = new Dictionary<Data.BuildingID, MuscleGroup>
        {
            // PRIMARY 1:1 MAPPINGS (The Core CODEX)
            { Data.BuildingID.archertower, MuscleGroup.Chest },      // Chest → Archer Tower
            { Data.BuildingID.cannon, MuscleGroup.Back },            // Back → Cannon  
            { Data.BuildingID.wizardtower, MuscleGroup.Shoulders },  // Shoulders → Wizard Tower
            { Data.BuildingID.hiddentesla, MuscleGroup.Biceps },     // Biceps → Hidden Tesla
            { Data.BuildingID.mortor, MuscleGroup.Triceps },         // Triceps → Mortar
            { Data.BuildingID.infernotower, MuscleGroup.Legs },      // Legs → Inferno Tower
            { Data.BuildingID.wall, MuscleGroup.Core },              // Core → Walls
            { Data.BuildingID.xbow, MuscleGroup.Cardio },            // Cardio → X-Bow
            
            // SECONDARY MAPPINGS (share muscle groups with similar buildings)
            { Data.BuildingID.airdefense, MuscleGroup.Shoulders },   // Also Shoulders
            { Data.BuildingID.bombtower, MuscleGroup.Biceps },       // Also Biceps
            { Data.BuildingID.airsweeper, MuscleGroup.Back }         // Also Back
        };

        // Volume required per building level (cumulative)
        public static readonly Dictionary<int, float> LevelRequirements = new Dictionary<int, float>
        {
            { 1, 0 },      // Level 1: Free
            { 2, 500 },    // Level 2: 500 kg
            { 3, 1500 },   // Level 3: 1500 kg
            { 4, 3500 },   // Level 4: 3500 kg
            { 5, 7000 },   // Level 5: 7000 kg
            { 6, 12000 },  // Level 6: 12000 kg
            { 7, 20000 },  // Level 7: 20000 kg
            { 8, 35000 },  // Level 8: 35000 kg
            { 9, 55000 },  // Level 9: 55000 kg
            { 10, 80000 }  // Level 10: 80000 kg
        };

        // ============================================================
        // EXERCISE AUTO-DETECTION: Exercise → Muscle Group Mapping
        // ============================================================
        public static readonly Dictionary<string, MuscleGroup> ExerciseToMuscle = new Dictionary<string, MuscleGroup>(StringComparer.OrdinalIgnoreCase)
        {
            // CHEST EXERCISES
            { "Bench Press", MuscleGroup.Chest },
            { "Incline Press", MuscleGroup.Chest },
            { "Decline Press", MuscleGroup.Chest },
            { "Dumbbell Fly", MuscleGroup.Chest },
            { "Chest Fly", MuscleGroup.Chest },
            { "Push-ups", MuscleGroup.Chest },
            { "Pushups", MuscleGroup.Chest },
            { "Cable Crossover", MuscleGroup.Chest },
            { "Pec Deck", MuscleGroup.Chest },
            { "Dips", MuscleGroup.Chest }, // Also triceps but primary chest

            // BACK EXERCISES
            { "Deadlift", MuscleGroup.Back },
            { "Barbell Row", MuscleGroup.Back },
            { "Dumbbell Row", MuscleGroup.Back },
            { "Bent Over Row", MuscleGroup.Back },
            { "Lat Pulldown", MuscleGroup.Back },
            { "Pull-ups", MuscleGroup.Back },
            { "Pullups", MuscleGroup.Back },
            { "Chin-ups", MuscleGroup.Back },
            { "Chinups", MuscleGroup.Back },
            { "Seated Row", MuscleGroup.Back },
            { "Cable Row", MuscleGroup.Back },
            { "T-Bar Row", MuscleGroup.Back },
            { "Face Pull", MuscleGroup.Back },

            // SHOULDER EXERCISES
            { "Overhead Press", MuscleGroup.Shoulders },
            { "Military Press", MuscleGroup.Shoulders },
            { "Shoulder Press", MuscleGroup.Shoulders },
            { "Lateral Raise", MuscleGroup.Shoulders },
            { "Side Raise", MuscleGroup.Shoulders },
            { "Front Raise", MuscleGroup.Shoulders },
            { "Rear Delt Fly", MuscleGroup.Shoulders },
            { "Arnold Press", MuscleGroup.Shoulders },
            { "Upright Row", MuscleGroup.Shoulders },
            { "Shrugs", MuscleGroup.Shoulders },

            // BICEPS EXERCISES
            { "Bicep Curl", MuscleGroup.Biceps },
            { "Barbell Curl", MuscleGroup.Biceps },
            { "Dumbbell Curl", MuscleGroup.Biceps },
            { "Hammer Curl", MuscleGroup.Biceps },
            { "Preacher Curl", MuscleGroup.Biceps },
            { "Concentration Curl", MuscleGroup.Biceps },
            { "Cable Curl", MuscleGroup.Biceps },
            { "EZ Bar Curl", MuscleGroup.Biceps },
            { "Spider Curl", MuscleGroup.Biceps },

            // TRICEPS EXERCISES
            { "Tricep Pushdown", MuscleGroup.Triceps },
            { "Tricep Extension", MuscleGroup.Triceps },
            { "Skull Crusher", MuscleGroup.Triceps },
            { "Overhead Extension", MuscleGroup.Triceps },
            { "Close Grip Bench", MuscleGroup.Triceps },
            { "Tricep Dips", MuscleGroup.Triceps },
            { "Kickbacks", MuscleGroup.Triceps },
            { "Diamond Push-ups", MuscleGroup.Triceps },

            // LEG EXERCISES
            { "Squat", MuscleGroup.Legs },
            { "Back Squat", MuscleGroup.Legs },
            { "Front Squat", MuscleGroup.Legs },
            { "Leg Press", MuscleGroup.Legs },
            { "Lunges", MuscleGroup.Legs },
            { "Leg Extension", MuscleGroup.Legs },
            { "Leg Curl", MuscleGroup.Legs },
            { "Calf Raise", MuscleGroup.Legs },
            { "Romanian Deadlift", MuscleGroup.Legs },
            { "Hip Thrust", MuscleGroup.Legs },
            { "Bulgarian Split Squat", MuscleGroup.Legs },
            { "Hack Squat", MuscleGroup.Legs },
            { "Step-ups", MuscleGroup.Legs },

            // CORE EXERCISES
            { "Plank", MuscleGroup.Core },
            { "Crunches", MuscleGroup.Core },
            { "Sit-ups", MuscleGroup.Core },
            { "Situps", MuscleGroup.Core },
            { "Leg Raise", MuscleGroup.Core },
            { "Russian Twist", MuscleGroup.Core },
            { "Ab Wheel", MuscleGroup.Core },
            { "Cable Crunch", MuscleGroup.Core },
            { "Hanging Leg Raise", MuscleGroup.Core },
            { "Mountain Climbers", MuscleGroup.Core },
            { "Dead Bug", MuscleGroup.Core },
            { "Bicycle Crunch", MuscleGroup.Core },

            // CARDIO EXERCISES
            { "Running", MuscleGroup.Cardio },
            { "Run", MuscleGroup.Cardio },
            { "Jogging", MuscleGroup.Cardio },
            { "Cycling", MuscleGroup.Cardio },
            { "Bike", MuscleGroup.Cardio },
            { "Rowing", MuscleGroup.Cardio },
            { "Swimming", MuscleGroup.Cardio },
            { "Jump Rope", MuscleGroup.Cardio },
            { "Burpees", MuscleGroup.Cardio },
            { "HIIT", MuscleGroup.Cardio },
            { "Elliptical", MuscleGroup.Cardio },
            { "Stair Climber", MuscleGroup.Cardio },
            { "Walking", MuscleGroup.Cardio },
            { "Treadmill", MuscleGroup.Cardio }
        };

        // ============================================================
        // MUSCLE CURRENCY: Each muscle has its own currency (volume-based)
        // Used ONLY for upgrading buildings mapped to that muscle
        // ============================================================
        [System.Serializable]
        public class MuscleCurrency
        {
            public string muscleName;
            public float volume;         // Total kg lifted
            public float todayVolume;    // Today's volume
            public int workoutCount;     // Total sets logged
            public string mappedBuilding; // The building this unlocks
        }

        public MuscleCurrency[] muscleCurrencies = new MuscleCurrency[8];

        // ============================================================
        // REAL FITNESS SCIENCE ALGORITHMS
        // ============================================================
        
        /// <summary>
        /// Workout history entry for trend analysis
        /// </summary>
        [System.Serializable]
        public class WorkoutEntry
        {
            public DateTime date;
            public MuscleGroup muscle;
            public string exercise;
            public float weight;
            public int reps;
            public int sets;
            public int rpe; // Rate of Perceived Exertion (1-10)
            public float volume => weight * reps * sets;
            public float estimated1RM => Calculate1RM(weight, reps);
        }
        
        // Workout history (last 30 days)
        public List<WorkoutEntry> workoutHistory = new List<WorkoutEntry>();
        
        // Personal records (1RM per muscle group)
        public float[] personalRecords = new float[8];
        
        // Weekly volume targets (recommended volume per muscle group)
        public static readonly float[] WeeklyVolumeTargets = {
            10000f,  // Chest: 10-20 sets/week @ ~500kg/set avg
            12000f,  // Back: Higher volume typical
            6000f,   // Shoulders: Lower volume needs
            4000f,   // Biceps: Small muscle
            4000f,   // Triceps: Small muscle
            15000f,  // Legs: High volume capacity
            5000f,   // Core: Moderate volume
            150f     // Cardio: 150 min/week recommended
        };
        
        /// <summary>
        /// Calculate estimated 1-Rep Max using Brzycki formula
        /// 1RM = weight × (36 / (37 - reps))
        /// Valid for reps 1-10
        /// </summary>
        public static float Calculate1RM(float weight, int reps)
        {
            if (reps <= 0) return 0;
            if (reps == 1) return weight;
            if (reps > 10) reps = 10; // Formula accuracy drops after 10 reps
            
            return weight * (36f / (37f - reps));
        }
        
        /// <summary>
        /// Calculate estimated reps at a given percentage of 1RM
        /// Inverse of Brzycki formula
        /// </summary>
        public static int CalculateRepsAt(float oneRM, float weight)
        {
            if (weight >= oneRM) return 1;
            if (weight <= 0) return 0;
            
            float percentage = weight / oneRM;
            // Reps ≈ 37 - (36 / percentage)
            int reps = Mathf.RoundToInt(37f - (36f / percentage));
            return Mathf.Clamp(reps, 1, 30);
        }
        
        /// <summary>
        /// Estimate calories burned from a workout set
        /// Based on: MET value × weight (person) × duration
        /// Simplified: ~0.05 cal per kg lifted for strength, ~8 cal/min for cardio
        /// </summary>
        public float EstimateCaloriesBurned(float volume, bool isCardio = false)
        {
            if (isCardio)
            {
                // Cardio: ~8 calories per minute (moderate intensity)
                return volume * 8f;
            }
            else
            {
                // Strength: ~0.05 calories per kg of volume
                return volume * 0.05f;
            }
        }
        
        /// <summary>
        /// Get total calories burned today across all workouts
        /// </summary>
        public float GetTodayCaloriesBurned()
        {
            float total = 0;
            for (int i = 0; i < 7; i++) // Strength muscles
            {
                total += EstimateCaloriesBurned(todayVolumes[i], false);
            }
            total += EstimateCaloriesBurned(todayVolumes[7], true); // Cardio
            return total;
        }
        
        /// <summary>
        /// Get weekly volume for a muscle group (last 7 days)
        /// </summary>
        public float GetWeeklyVolume(MuscleGroup muscle)
        {
            float total = 0;
            DateTime weekAgo = DateTime.Today.AddDays(-7);
            
            foreach (var entry in workoutHistory)
            {
                if (entry.muscle == muscle && entry.date >= weekAgo)
                {
                    total += entry.volume;
                }
            }
            return total;
        }
        
        /// <summary>
        /// Get progress percentage toward weekly volume target
        /// </summary>
        public float GetWeeklyProgress(MuscleGroup muscle)
        {
            float current = GetWeeklyVolume(muscle);
            float target = WeeklyVolumeTargets[(int)muscle];
            return Mathf.Clamp01(current / target) * 100f;
        }
        
        /// <summary>
        /// Check if progressive overload achieved this week
        /// (Volume or intensity increased vs last week)
        /// </summary>
        public bool IsProgressiveOverload(MuscleGroup muscle)
        {
            DateTime weekAgo = DateTime.Today.AddDays(-7);
            DateTime twoWeeksAgo = DateTime.Today.AddDays(-14);
            
            float thisWeek = 0, lastWeek = 0;
            
            foreach (var entry in workoutHistory)
            {
                if (entry.muscle == muscle)
                {
                    if (entry.date >= weekAgo)
                        thisWeek += entry.volume;
                    else if (entry.date >= twoWeeksAgo)
                        lastWeek += entry.volume;
                }
            }
            
            // Progressive overload = at least 2.5% more volume
            return thisWeek > lastWeek * 1.025f;
        }
        
        /// <summary>
        /// Get personal record for a muscle group (highest estimated 1RM)
        /// </summary>
        public float GetPersonalRecord(MuscleGroup muscle)
        {
            return personalRecords[(int)muscle];
        }
        
        /// <summary>
        /// Update personal record if new lift is higher
        /// </summary>
        private void UpdatePersonalRecord(MuscleGroup muscle, float weight, int reps)
        {
            float estimated1RM = Calculate1RM(weight, reps);
            if (estimated1RM > personalRecords[(int)muscle])
            {
                personalRecords[(int)muscle] = estimated1RM;
                Debug.Log($"[NEW PR!] {muscle}: {estimated1RM:F1}kg estimated 1RM!");
            }
        }
        
        /// <summary>
        /// Calculate optimal rest time based on intensity and goals
        /// Hypertrophy: 60-90s, Strength: 2-5 min
        /// </summary>
        public int GetRecommendedRestTime(float percentOf1RM)
        {
            if (percentOf1RM >= 0.9f) return 300; // 5 min for near-max
            if (percentOf1RM >= 0.8f) return 180; // 3 min for strength
            if (percentOf1RM >= 0.7f) return 120; // 2 min for power
            if (percentOf1RM >= 0.6f) return 90;  // 90s for hypertrophy
            return 60; // 60s for endurance/pump
        }
        
        /// <summary>
        /// Get muscle group recovery status based on last workout
        /// Returns hours since last trained
        /// </summary>
        public int GetMuscleRecoveryHours(MuscleGroup muscle)
        {
            DateTime lastTrained = DateTime.MinValue;
            
            foreach (var entry in workoutHistory)
            {
                if (entry.muscle == muscle && entry.date > lastTrained)
                {
                    lastTrained = entry.date;
                }
            }
            
            if (lastTrained == DateTime.MinValue) return 168; // Never trained = 7 days
            return (int)(DateTime.Now - lastTrained).TotalHours;
        }
        
        /// <summary>
        /// Check if muscle is recovered enough to train again
        /// Small muscles: 48h, Large muscles: 72h
        /// </summary>
        public bool IsMuscleRecovered(MuscleGroup muscle)
        {
            int hoursNeeded = muscle switch
            {
                MuscleGroup.Legs => 72,
                MuscleGroup.Back => 72,
                MuscleGroup.Chest => 72,
                MuscleGroup.Cardio => 24, // Can do daily
                _ => 48 // Small muscles
            };
            
            return GetMuscleRecoveryHours(muscle) >= hoursNeeded;
        }

        // ============================================================
        // UNITY LIFECYCLE
        // ============================================================
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Initialize muscle currencies
            InitializeMuscleCurrencies();
            
            // Load saved fitness data
            LoadFitnessData();
            
            // Sync with server to get latest valid stats
            SyncFitnessFromServer();
        }

        /// <summary>
        /// Initialize the muscle currency tracking system.
        /// Each muscle is its own currency for building upgrades.
        /// </summary>
        private void InitializeMuscleCurrencies()
        {
            string[] muscleNames = { "Chest", "Back", "Shoulders", "Biceps", "Triceps", "Legs", "Core", "Cardio" };
            string[] buildingNames = { "Archer Tower", "Cannon", "Wizard Tower", "Hidden Tesla", "Mortar", "Inferno Tower", "Walls", "X-Bow" };

            for (int i = 0; i < 8; i++)
            {
                muscleCurrencies[i] = new MuscleCurrency
                {
                    muscleName = muscleNames[i],
                    volume = muscleVolumes[i],
                    todayVolume = todayVolumes[i],
                    workoutCount = 0,
                    mappedBuilding = buildingNames[i]
                };
            }
        }

        // ============================================================
        // WORKOUT LOGGING
        // ============================================================
        
        /// <summary>
        /// Log a workout set. Volume = weight * reps
        /// Now also tracks workout history, PRs, and calories
        /// </summary>
        public void LogWorkout(MuscleGroup muscle, float weight, int reps, int sets = 1, string exerciseName = "")
        {
            float volume = weight * reps * sets;
            muscleVolumes[(int)muscle] += volume;
            todayVolumes[(int)muscle] += volume;
            
            // Update muscle currency
            if (muscleCurrencies[(int)muscle] != null)
            {
                muscleCurrencies[(int)muscle].volume = muscleVolumes[(int)muscle];
                muscleCurrencies[(int)muscle].todayVolume = todayVolumes[(int)muscle];
                muscleCurrencies[(int)muscle].workoutCount++;
            }
            
            // Add to workout history
            workoutHistory.Add(new WorkoutEntry
            {
                date = DateTime.Now,
                muscle = muscle,
                exercise = exerciseName,
                weight = weight,
                reps = reps,
                sets = sets,
                rpe = 7 // Default RPE
            });
            
            // Trim history to last 30 days
            DateTime cutoff = DateTime.Today.AddDays(-30);
            workoutHistory.RemoveAll(e => e.date < cutoff);
            
            // Update personal record
            UpdatePersonalRecord(muscle, weight, reps);
            
            // Update streak
            if (lastWorkoutDate.Date != DateTime.Today)
            {
                if (lastWorkoutDate.Date == DateTime.Today.AddDays(-1))
                {
                    workoutStreak++;
                }
                else
                {
                    workoutStreak = 1;
                }
                lastWorkoutDate = DateTime.Today;
            }

            // Calculate recovery impact (intense workouts reduce recovery)
            float intensity = GetPersonalRecord(muscle) > 0 ? weight / GetPersonalRecord(muscle) : 0.5f;
            if (intensity > 0.8f) // Heavy set
            {
                recoveryScore = Mathf.Max(0, recoveryScore - 15);
            }
            else if (volume > 500)
            {
                recoveryScore = Mathf.Max(0, recoveryScore - 10);
            }

            // Save to local storage
            SaveFitnessData();

            // Sync with server
            SendWorkoutToServer(muscle, volume, reps);

            // Check for level up
            CheckAndShowLevelUp(muscle);

            // Log with calorie info
            float calories = EstimateCaloriesBurned(volume, muscle == MuscleGroup.Cardio);
            Debug.Log($"[LOGGED] {volume:F0}kg {muscle} | 1RM: {Calculate1RM(weight, reps):F1}kg | ~{calories:F0} cal");
        }

        /// <summary>
        /// Log cardio workout (in minutes)
        /// </summary>
        public void LogCardio(int minutes)
        {
            muscleVolumes[(int)MuscleGroup.Cardio] += minutes;
            todayVolumes[(int)MuscleGroup.Cardio] += minutes;
            
            // Update currency
            if (muscleCurrencies[(int)MuscleGroup.Cardio] != null)
            {
                muscleCurrencies[(int)MuscleGroup.Cardio].volume = muscleVolumes[(int)MuscleGroup.Cardio];
                muscleCurrencies[(int)MuscleGroup.Cardio].workoutCount++;
            }
            
            // Cardio improves recovery
            recoveryScore = Mathf.Min(100, recoveryScore + (minutes / 5));
            
            SaveFitnessData();
            SendWorkoutToServer(MuscleGroup.Cardio, minutes, 1);
            
            Debug.Log($"🏃 Logged: {minutes} min cardio. Recovery: {recoveryScore}%");
        }

        /// <summary>
        /// LOG EXERCISE WITH AUTO-DETECTION
        /// Automatically identifies which muscle group the exercise targets
        /// and adds volume to the correct muscle currency.
        /// </summary>
        /// <param name="exerciseName">Name of the exercise (e.g., "Bench Press", "Squat")</param>
        /// <param name="weight">Weight used in kg</param>
        /// <param name="reps">Number of reps</param>
        /// <returns>The detected muscle group, or null if not found</returns>
        public MuscleGroup? LogExercise(string exerciseName, float weight, int reps)
        {
            // Try exact match first
            if (ExerciseToMuscle.TryGetValue(exerciseName, out MuscleGroup muscle))
            {
                LogWorkout(muscle, weight, reps);
                Debug.Log($"🎯 Auto-detected: '{exerciseName}' → {muscle} ({weight}kg x {reps})");
                return muscle;
            }

            // Try partial match (contains)
            foreach (var kvp in ExerciseToMuscle)
            {
                if (exerciseName.ToLower().Contains(kvp.Key.ToLower()) || 
                    kvp.Key.ToLower().Contains(exerciseName.ToLower()))
                {
                    LogWorkout(kvp.Value, weight, reps);
                    Debug.Log($"🎯 Fuzzy match: '{exerciseName}' → {kvp.Value} (matched '{kvp.Key}')");
                    return kvp.Value;
                }
            }

            Debug.LogWarning($"⚠️ Unknown exercise: '{exerciseName}'. Please select muscle group manually.");
            return null;
        }

        /// <summary>
        /// Get the muscle currency for a specific muscle group.
        /// This is used to check if player can afford building upgrades.
        /// </summary>
        public MuscleCurrency GetMuscleCurrency(MuscleGroup muscle)
        {
            if (muscleCurrencies[(int)muscle] != null)
                return muscleCurrencies[(int)muscle];
            return null;
        }

        /// <summary>
        /// Get all muscle currencies for display.
        /// </summary>
        public MuscleCurrency[] GetAllMuscleCurrencies()
        {
            return muscleCurrencies;
        }

        /// <summary>
        /// Check if player can afford an upgrade using the specific muscle currency.
        /// </summary>
        public bool CanAffordUpgrade(MuscleGroup muscle, float requiredVolume)
        {
            if (muscleCurrencies[(int)muscle] != null)
                return muscleCurrencies[(int)muscle].volume >= requiredVolume;
            return false;
        }

        // ============================================================
        // BUILDING UPGRADE CHECKS (THE CODEX)
        // ============================================================
        
        /// <summary>
        /// Check if player has enough volume to upgrade a building
        /// </summary>
        public bool CanUpgradeBuilding(Data.BuildingID buildingId, int currentLevel)
        {
            // Town Hall uses consistency (streak days) instead of volume
            if (buildingId == Data.BuildingID.townhall)
            {
                int requiredDays = currentLevel * 7; // 7 days per TH level
                return workoutStreak >= requiredDays;
            }

            // Check if building has a muscle requirement
            if (!BuildingToMuscle.ContainsKey(buildingId))
            {
                return true; // No fitness requirement
            }

            MuscleGroup muscle = BuildingToMuscle[buildingId];
            int targetLevel = currentLevel + 1;

            if (!LevelRequirements.ContainsKey(targetLevel))
            {
                return false; // Max level reached
            }

            float requiredVolume = LevelRequirements[targetLevel];
            float currentVolume = muscleVolumes[(int)muscle];

            return currentVolume >= requiredVolume;
        }

        /// <summary>
        /// Get the volume requirement for upgrading a building
        /// </summary>
        public float GetUpgradeRequirement(Data.BuildingID buildingId, int targetLevel)
        {
            if (!LevelRequirements.ContainsKey(targetLevel))
                return float.MaxValue;
            
            return LevelRequirements[targetLevel];
        }

        /// <summary>
        /// Get the muscle group name for a building
        /// </summary>
        public string GetBuildingMuscle(Data.BuildingID buildingId)
        {
            if (BuildingToMuscle.ContainsKey(buildingId))
            {
                return BuildingToMuscle[buildingId].ToString();
            }
            return "None";
        }

        /// <summary>
        /// Get current volume for a muscle group
        /// </summary>
        public float GetMuscleVolume(MuscleGroup muscle)
        {
            return muscleVolumes[(int)muscle];
        }

        // ============================================================
        // RESOURCE GENERATION (FITNESS-BASED)
        // ============================================================
        
        /// <summary>
        /// Calculate gold earned from cardio
        /// 1 minute cardio = 10 gold
        /// </summary>
        public int CalculateGoldFromFitness()
        {
            return (int)(muscleVolumes[(int)MuscleGroup.Cardio] * 10);
        }

        /// <summary>
        /// Calculate elixir from strength training
        /// 1 kg volume = 1 elixir
        /// </summary>
        public int CalculateElixirFromFitness()
        {
            float totalStrength = 0;
            for (int i = 0; i < 7; i++) // All muscle groups except cardio
            {
                totalStrength += muscleVolumes[i];
            }
            return (int)totalStrength;
        }

        /// <summary>
        /// Calculate gems from streak
        /// 7-day streak = 10 gems, 30-day = 50 gems
        /// </summary>
        public int CalculateGemsFromStreak()
        {
            if (workoutStreak >= 30) return 50;
            if (workoutStreak >= 14) return 25;
            if (workoutStreak >= 7) return 10;
            return 0;
        }

        // ============================================================
        // RAID POWER (FITNESS-BASED)
        // ============================================================
        
        /// <summary>
        /// Calculate attack power based on today's workout volume
        /// </summary>
        public float GetAttackPower()
        {
            float total = 0;
            for (int i = 0; i < todayVolumes.Length; i++)
            {
                total += todayVolumes[i];
            }
            return total;
        }

        /// <summary>
        /// Calculate defense power based on total accumulated volume
        /// </summary>
        public float GetDefensePower()
        {
            float total = 0;
            for (int i = 0; i < muscleVolumes.Length; i++)
            {
                total += muscleVolumes[i];
            }
            return total / 10; // Scaled down for balance
        }

        // ============================================================
        // PERSISTENCE
        // ============================================================
        
        private void SaveFitnessData()
        {
            for (int i = 0; i < muscleVolumes.Length; i++)
            {
                PlayerPrefs.SetFloat($"muscle_{i}", muscleVolumes[i]);
                PlayerPrefs.SetFloat($"today_{i}", todayVolumes[i]);
                PlayerPrefs.SetFloat($"pr_{i}", personalRecords[i]); // Save PRs
            }
            PlayerPrefs.SetInt("recovery", recoveryScore);
            PlayerPrefs.SetInt("streak", workoutStreak);
            PlayerPrefs.SetString("lastWorkout", lastWorkoutDate.ToString());
            PlayerPrefs.Save();
        }

        private void LoadFitnessData()
        {
            for (int i = 0; i < muscleVolumes.Length; i++)
            {
                muscleVolumes[i] = PlayerPrefs.GetFloat($"muscle_{i}", 0);
                todayVolumes[i] = PlayerPrefs.GetFloat($"today_{i}", 0);
                personalRecords[i] = PlayerPrefs.GetFloat($"pr_{i}", 0); // Load PRs
            }
            recoveryScore = PlayerPrefs.GetInt("recovery", 100);
            workoutStreak = PlayerPrefs.GetInt("streak", 0);
            
            string lastDate = PlayerPrefs.GetString("lastWorkout", "");
            if (!string.IsNullOrEmpty(lastDate))
            {
                DateTime.TryParse(lastDate, out lastWorkoutDate);
            }

            // Reset today's volume if it's a new day
            if (lastWorkoutDate.Date != DateTime.Today)
            {
                for (int i = 0; i < todayVolumes.Length; i++)
                {
                    todayVolumes[i] = 0;
                }
                // Recovery increases overnight
                recoveryScore = Mathf.Min(100, recoveryScore + 20);
            }
        }

        /// <summary>
        /// Reset ALL fitness data to zero. Use this to clear garbage/corrupted data.
        /// </summary>
        public void ResetAllData()
        {
            // Clear all muscle volumes
            for (int i = 0; i < 8; i++)
            {
                muscleVolumes[i] = 0;
                todayVolumes[i] = 0;
                personalRecords[i] = 0;
                if (muscleCurrencies[i] != null)
                {
                    muscleCurrencies[i].volume = 0;
                    muscleCurrencies[i].todayVolume = 0;
                    muscleCurrencies[i].workoutCount = 0;
                }
            }
            
            // Reset other stats
            recoveryScore = 100;
            workoutStreak = 0;
            lastWorkoutDate = DateTime.MinValue;
            workoutHistory.Clear();
            
            // Clear PlayerPrefs
            for (int i = 0; i < 8; i++)
            {
                PlayerPrefs.DeleteKey($"muscle_{i}");
                PlayerPrefs.DeleteKey($"today_{i}");
                PlayerPrefs.DeleteKey($"pr_{i}");
            }
            PlayerPrefs.DeleteKey("recovery");
            PlayerPrefs.DeleteKey("streak");
            PlayerPrefs.DeleteKey("lastWorkout");
            PlayerPrefs.Save();
            
            Debug.Log("🔄 ALL FITNESS DATA RESET TO ZERO");
        }

        // Tracking previous levels for level-up detection
        private int[] _previousLevels = new int[8];

        /// <summary>
        /// Check if any muscle group leveled up and show notification.
        /// Call this after LogWorkout to detect level ups.
        /// </summary>
        public void CheckAndShowLevelUp(MuscleGroup muscle)
        {
            int muscleIndex = (int)muscle;
            int newLevel = GetMuscleLevel(muscle);
            
            if (_previousLevels[muscleIndex] > 0 && newLevel > _previousLevels[muscleIndex])
            {
                ShowLevelUpNotification(muscle, newLevel);
            }
            _previousLevels[muscleIndex] = newLevel;
        }

        /// <summary>
        /// Get the current level of a muscle group based on accumulated volume.
        /// </summary>
        public int GetMuscleLevel(MuscleGroup muscle)
        {
            float volume = muscleVolumes[(int)muscle];
            int level = 1;
            
            foreach (var req in LevelRequirements)
            {
                if (volume >= req.Value)
                    level = req.Key;
            }
            return level;
        }

        /// <summary>
        /// Show a level up notification popup.
        /// </summary>
        public void ShowLevelUpNotification(MuscleGroup muscle, int newLevel)
        {
            string muscleName = muscle.ToString();
            string message = $"🎉 {muscleName} reached Level {newLevel}!";
            
            Debug.Log($"🏆 LEVEL UP! {message}");
            
            // Create popup if UI_Fitness is available
            if (UI_Fitness.instance != null)
            {
                UI_Fitness.instance.ShowLevelUpPopup(muscleName, newLevel);
            }
        }

        // ============================================================
        // SERVER SYNC
        // ============================================================
        
        private void SendWorkoutToServer(MuscleGroup muscle, float volume, int reps)
        {
            Packet packet = new Packet();
            packet.Write((int)Player.RequestsID.FITNESS_LOG);
            packet.Write((int)muscle);
            packet.Write(volume);
            packet.Write(reps);
            Sender.TCP_Send(packet);
        }

        public void UpdateFromSync(float[] volumes, int streak, int recovery)
        {
            if (volumes.Length != 8) return;
            
            for (int i = 0; i < 8; i++)
            {
                muscleVolumes[i] = volumes[i];
                if (muscleCurrencies[i] != null)
                {
                    muscleCurrencies[i].volume = volumes[i];
                }
            }
            workoutStreak = streak;
            recoveryScore = recovery;
            
            // Save to local
            SaveFitnessData();
            Debug.Log("✅ Fitness Stats Synced from Server");
        }

        public void SyncFitnessFromServer()
        {
            // BIO-CLASH: Check if network is ready before syncing
            // This prevents NullReferenceException on startup when connection isn't established yet
            if (Client.instance == null || !Client.instance.isConnected)
            {
                Debug.Log("Fitness sync deferred - waiting for network connection");
                return;
            }
            
            Packet packet = new Packet();
            packet.Write((int)Player.RequestsID.FITNESS_STATS);
            Sender.TCP_Send(packet);
        }

        // ============================================================
        // BATTLE INTEGRATION - ATTACK POWER FROM FITNESS
        // ============================================================
        
        /// <summary>
        /// Get the attack power multiplier based on total fitness volume.
        /// Higher total volume = more damage in battles.
        /// Base is 1.0x, can scale up to 2.5x at max fitness.
        /// </summary>
        public float GetAttackPowerMultiplier()
        {
            float totalVolume = GetTotalVolume();
            
            // Scaling: 0 volume = 1.0x, 100,000 volume = 2.0x, 500,000+ = 2.5x
            if (totalVolume <= 0) return 1.0f;
            if (totalVolume >= 500000) return 2.5f;
            
            // Linear scaling from 1.0 to 2.5 over 500,000 volume
            return 1.0f + (totalVolume / 500000f) * 1.5f;
        }

        /// <summary>
        /// Get the total volume across all muscle groups (excluding cardio).
        /// Used for attack power calculations.
        /// </summary>
        public float GetTotalVolume()
        {
            float total = 0;
            for (int i = 0; i < 7; i++) // Exclude cardio (index 7)
            {
                total += muscleVolumes[i];
            }
            return total;
        }

        /// <summary>
        /// Get the fitness level (1-50) based on total volume.
        /// Used for matchmaking and display.
        /// </summary>
        public int GetTotalFitnessLevel()
        {
            float totalVolume = GetTotalVolume();
            
            // Every 10,000 volume = 1 level, max 50
            int level = Mathf.FloorToInt(totalVolume / 10000f) + 1;
            return Mathf.Clamp(level, 1, 50);
        }

        /// <summary>
        /// Get defense multiplier based on recovery score.
        /// Higher recovery = less damage taken.
        /// </summary>
        public float GetDefenseMultiplier()
        {
            // Recovery 100 = 1.3x defense, Recovery 0 = 0.7x defense
            return 0.7f + (recoveryScore / 100f) * 0.6f;
        }
    }
}
