using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace DevelopersHub.RealtimeNetworking.Server
{
    /// <summary>
    /// Bio-Clash Fitness Engine
    /// "Your Body Builds Your Base" - Manages workout logging, volume tracking, and THE CODEX.
    /// </summary>
    public static class Fitness
    {
        // ============================================================
        // MUSCLE GROUPS
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
            Cardio = 7
        }

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
        public static readonly Dictionary<string, MuscleGroup> BuildingToMuscle = new Dictionary<string, MuscleGroup>
        {
            // PRIMARY 1:1 MAPPINGS (The Core CODEX)
            { "archertower", MuscleGroup.Chest },      // Chest → Archer Tower
            { "cannon", MuscleGroup.Back },            // Back → Cannon  
            { "wizardtower", MuscleGroup.Shoulders },  // Shoulders → Wizard Tower
            { "hiddentesla", MuscleGroup.Biceps },     // Biceps → Hidden Tesla
            { "mortor", MuscleGroup.Triceps },         // Triceps → Mortar
            { "infernotower", MuscleGroup.Legs },      // Legs → Inferno Tower
            { "wall", MuscleGroup.Core },              // Core → Walls
            { "xbow", MuscleGroup.Cardio },            // Cardio → X-Bow
            
            // SECONDARY MAPPINGS (share muscle groups with similar buildings)
            { "airdefense", MuscleGroup.Shoulders },   // Also Shoulders
            { "bombtower", MuscleGroup.Biceps },       // Also Biceps
            { "airsweeper", MuscleGroup.Back }         // Also Back
        };

        // Volume requirements per level (cumulative)
        public static readonly Dictionary<int, float> LevelRequirements = new Dictionary<int, float>
        {
            { 1, 0 },
            { 2, 500 },
            { 3, 1500 },
            { 4, 3500 },
            { 5, 7000 },
            { 6, 12000 },
            { 7, 20000 },
            { 8, 35000 },
            { 9, 55000 },
            { 10, 80000 }
        };

        // ============================================================
        // WORKOUT LOGGING
        // ============================================================

        /// <summary>
        /// Log a workout for a player.
        /// </summary>
        public static async Task<bool> LogWorkout(long playerId, int muscleGroup, float volume, int reps)
        {
            try
            {
                using (MySqlConnection connection = Database.GetMysqlConnection())
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        INSERT INTO fitness_logs (player_id, muscle_group, volume_kg, reps, logged_at)
                        VALUES (@playerId, @muscle, @volume, @reps, NOW())";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@playerId", playerId);
                        cmd.Parameters.AddWithValue("@muscle", muscleGroup);
                        cmd.Parameters.AddWithValue("@volume", volume);
                        cmd.Parameters.AddWithValue("@reps", reps);
                        
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Update player's total volume
                    await UpdatePlayerVolume(connection, playerId, muscleGroup, volume);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Terminal.Log($"Fitness.LogWorkout Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update player's cumulative volume for a muscle group.
        /// </summary>
        private static async Task UpdatePlayerVolume(MySqlConnection connection, long playerId, int muscleGroup, float volume)
        {
            string query = @"
                INSERT INTO player_fitness (player_id, muscle_group, total_volume)
                VALUES (@playerId, @muscle, @volume)
                ON DUPLICATE KEY UPDATE total_volume = total_volume + @volume";
            
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@playerId", playerId);
                cmd.Parameters.AddWithValue("@muscle", muscleGroup);
                cmd.Parameters.AddWithValue("@volume", volume);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ============================================================
        // STATS RETRIEVAL
        // ============================================================

        /// <summary>
        /// Get player's fitness stats (volume per muscle group).
        /// </summary>
        public static async Task<Dictionary<int, float>> GetPlayerStats(long playerId)
        {
            var stats = new Dictionary<int, float>();
            
            // Initialize all muscle groups to 0
            for (int i = 0; i < 8; i++)
            {
                stats[i] = 0;
            }

            try
            {
                using (MySqlConnection connection = Database.GetMysqlConnection())
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT muscle_group, total_volume 
                        FROM player_fitness 
                        WHERE player_id = @playerId";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@playerId", playerId);
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int muscle = reader.GetInt32("muscle_group");
                                float volume = reader.GetFloat("total_volume");
                                stats[muscle] = volume;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Terminal.Log($"Fitness.GetPlayerStats Error: {ex.Message}");
            }

            return stats;
        }

        /// <summary>
        /// Check if player meets fitness requirement for building upgrade.
        /// </summary>
        public static async Task<bool> CanUpgradeBuilding(long playerId, string buildingType, int targetLevel)
        {
            // Town Hall uses streak, not volume
            if (buildingType.ToLower() == "townhall")
            {
                int streak = await GetPlayerStreak(playerId);
                return streak >= (targetLevel * 7);
            }

            // Check if building has a muscle requirement
            if (!BuildingToMuscle.ContainsKey(buildingType.ToLower()))
            {
                return true; // No fitness requirement
            }

            if (!LevelRequirements.ContainsKey(targetLevel))
            {
                return false; // Max level
            }

            MuscleGroup muscle = BuildingToMuscle[buildingType.ToLower()];
            float requiredVolume = LevelRequirements[targetLevel];

            var stats = await GetPlayerStats(playerId);
            float currentVolume = stats.ContainsKey((int)muscle) ? stats[(int)muscle] : 0;

            return currentVolume >= requiredVolume;
        }

        /// <summary>
        /// Get player's workout streak (consecutive days).
        /// </summary>
        public static async Task<int> GetPlayerStreak(long playerId)
        {
            try
            {
                using (MySqlConnection connection = Database.GetMysqlConnection())
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT streak FROM player_fitness_meta 
                        WHERE player_id = @playerId";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@playerId", playerId);
                        
                        object result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get player's recovery score (0-100).
        /// </summary>
        public static async Task<int> GetRecoveryScore(long playerId)
        {
            try
            {
                using (MySqlConnection connection = Database.GetMysqlConnection())
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT recovery_score FROM player_fitness_meta 
                        WHERE player_id = @playerId";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@playerId", playerId);
                        
                        object result = await cmd.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : 100;
                    }
                }
            }
            catch
            {
                return 100;
            }
        }

        // ============================================================
        // RESOURCE CALCULATION (FITNESS-BASED)
        // ============================================================

        /// <summary>
        /// Calculate gold from cardio (1 min = 10 gold).
        /// </summary>
        public static async Task<int> CalculateGoldFromFitness(long playerId)
        {
            var stats = await GetPlayerStats(playerId);
            float cardioMinutes = stats.ContainsKey((int)MuscleGroup.Cardio) ? stats[(int)MuscleGroup.Cardio] : 0;
            return (int)(cardioMinutes * 10);
        }

        /// <summary>
        /// Calculate elixir from total lifting volume (1 kg = 1 elixir).
        /// </summary>
        public static async Task<int> CalculateElixirFromFitness(long playerId)
        {
            var stats = await GetPlayerStats(playerId);
            float totalVolume = 0;
            for (int i = 0; i < 7; i++) // Exclude cardio
            {
                if (stats.ContainsKey(i))
                {
                    totalVolume += stats[i];
                }
            }
            return (int)totalVolume;
        }

        /// <summary>
        /// Calculate attack power from today's workout volume.
        /// </summary>
        public static async Task<float> GetAttackPower(long playerId)
        {
            try
            {
                using (MySqlConnection connection = Database.GetMysqlConnection())
                {
                    await connection.OpenAsync();
                    
                    string query = @"
                        SELECT SUM(volume_kg) as today_volume 
                        FROM fitness_logs 
                        WHERE player_id = @playerId 
                        AND DATE(logged_at) = CURDATE()";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@playerId", playerId);
                        
                        object result = await cmd.ExecuteScalarAsync();
                        return result != DBNull.Value ? Convert.ToSingle(result) : 0;
                    }
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
