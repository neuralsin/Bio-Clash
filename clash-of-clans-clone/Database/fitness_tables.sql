-- Bio-Clash Fitness Database Tables
-- Run this on your MySQL database to add fitness tracking

-- Workout history log
CREATE TABLE IF NOT EXISTS fitness_logs (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    player_id BIGINT NOT NULL,
    muscle_group INT NOT NULL COMMENT '0=Chest, 1=Back, 2=Shoulders, 3=Biceps, 4=Triceps, 5=Legs, 6=Core, 7=Cardio',
    volume_kg FLOAT NOT NULL COMMENT 'Weight * Reps for strength, Minutes for cardio',
    reps INT DEFAULT 1,
    logged_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_player (player_id),
    INDEX idx_muscle (muscle_group),
    INDEX idx_date (logged_at)
);

-- Player cumulative fitness stats
CREATE TABLE IF NOT EXISTS player_fitness (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    player_id BIGINT NOT NULL,
    muscle_group INT NOT NULL,
    total_volume FLOAT DEFAULT 0,
    UNIQUE KEY unique_player_muscle (player_id, muscle_group),
    INDEX idx_player (player_id)
);

-- Player fitness metadata (streak, recovery, etc)
CREATE TABLE IF NOT EXISTS player_fitness_meta (
    player_id BIGINT PRIMARY KEY,
    streak INT DEFAULT 0 COMMENT 'Consecutive days with workouts',
    recovery_score INT DEFAULT 100 COMMENT '0-100 recovery percentage',
    last_workout DATE COMMENT 'Date of last workout',
    total_workouts INT DEFAULT 0,
    INDEX idx_streak (streak DESC)
);

-- THE CODEX reference (optional - for lookup)
CREATE TABLE IF NOT EXISTS building_muscle_map (
    building_type VARCHAR(50) PRIMARY KEY,
    muscle_group INT NOT NULL,
    description VARCHAR(100)
);

INSERT IGNORE INTO building_muscle_map (building_type, muscle_group, description) VALUES
('cannon', 1, 'Back muscles - Rows and Pulldowns'),
('archertower', 0, 'Chest muscles - Bench and Fly'),
('mortar', 4, 'Triceps - Pushdowns and Extensions'),
('wizardtower', 2, 'Shoulders - Press and Raises'),
('infernotower', 5, 'Legs - Squats and Lunges'),
('hiddentesla', 3, 'Biceps - Curls'),
('wall', 6, 'Core - Planks and Crunches'),
('xbow', 7, 'Cardio - Running and Cycling'),
('eagleartillery', 5, 'Legs'),
('airdefense', 2, 'Shoulders');
