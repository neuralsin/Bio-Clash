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
        /// </summary>
        public void LogWorkout(MuscleGroup muscle, float weight, int reps)
        {
            float volume = weight * reps;
            muscleVolumes[(int)muscle] += volume;
            todayVolumes[(int)muscle] += volume;
            
            // Update muscle currency
            if (muscleCurrencies[(int)muscle] != null)
            {
                muscleCurrencies[(int)muscle].volume = muscleVolumes[(int)muscle];
                muscleCurrencies[(int)muscle].todayVolume = todayVolumes[(int)muscle];
                muscleCurrencies[(int)muscle].workoutCount++;
            }
            
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
            if (volume > 500)
            {
                recoveryScore = Mathf.Max(0, recoveryScore - 10);
            }

            // Save to local storage
            SaveFitnessData();

            // Sync with server
            SendWorkoutToServer(muscle, volume, reps);

            Debug.Log($"💪 Logged: {volume}kg {muscle}. Total: {muscleVolumes[(int)muscle]}kg");
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

        public void SyncFitnessFromServer()
        {
            Packet packet = new Packet();
            packet.Write((int)Player.RequestsID.FITNESS_STATS);
            Sender.TCP_Send(packet);
        }
    }
}
