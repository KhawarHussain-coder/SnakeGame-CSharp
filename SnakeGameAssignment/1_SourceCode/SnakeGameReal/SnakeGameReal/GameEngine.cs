// GameEngine.cs - COMPLETELY FIXED
using System;
using System.Drawing;

namespace SnakeGame
{
    /// <summary>
    /// Directions the snake can move
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }

    /// <summary>
    /// Main game logic controller
    /// </summary>
    public class GameEngine
    {
        // Game objects
        public Snake Snake { get; private set; }
        public Food Food { get; private set; }

        // Game state
        public int Score { get; private set; }
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }
        public string snakeName { get; private set; }
        public bool IsGameOver { get; private set; }
        public int Level { get; private set; }
        public int BaseSpeed { get; private set; }
        public int FoodEatenCount { get; private set; }

        // FIX: High score is now static to persist across entire session
        private static int sessionHighScore = 0;

        // Internal state
        private Random random;
        private Direction currentDirection;
        private Direction nextDirection;
        private int speedBoostTimer; // For fast food effect
        private int speedSlowTimer;  // For slow food effect
        private const int FOOD_PER_LEVEL = 5; // Increase level every 5 foods

        // Events for UI updates
        public event Action<int> ScoreUpdated;
        public event Action<int> LevelUpdated;
        public event Action<FoodType> FoodEaten;
        public event Action GameOver;
        public event Action<int> SpeedChanged; // For speed effect notifications

        // FIX: Added event for high score updates
        public event Action<int> HighScoreUpdated;

        /// <summary>
        /// Initializes a new game engine
        /// </summary>
        public GameEngine(int gridWidth, int gridHeight, string snakeName = "Teacher")
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            random = new Random();

            // Pass snake name to InitializeGame
            this.snakeName = snakeName;
            InitializeGame();
        }

        /// <summary>
        /// Sets up a new game
        /// </summary>
        private void InitializeGame()
        {
            // Start snake in the middle of the grid
            int startX = GridWidth / 2;
            int startY = GridHeight / 2;

            Snake = new Snake(startX, startY, snakeName);
            currentDirection = Direction.Right;
            nextDirection = Direction.Right;

            // Generate first food
            GenerateFood();

            // Reset game state - BUT NOT HIGH SCORE
            Score = 0;
            FoodEatenCount = 0;
            Level = 1;
            BaseSpeed = 150; // Starting speed (milliseconds)
            IsGameOver = false;
            speedBoostTimer = 0;
            speedSlowTimer = 0;

            // FIX: Always notify UI of the current session high score
            HighScoreUpdated?.Invoke(sessionHighScore);

            // Notify UI of initial level
            LevelUpdated?.Invoke(Level);
        }

        /// <summary>
        /// Updates game state (called every timer tick)
        /// </summary>
        public void Update()
        {
            if (IsGameOver) return;

            // Update speed effects
            UpdateSpeedEffects();

            // Apply the buffered direction (prevents multiple direction changes per frame)
            if (nextDirection != Direction.None)
            {
                currentDirection = nextDirection;
            }

            // Move the snake
            Snake.Move(currentDirection);

            // Get the head position for collision checks
            var head = Snake.Head;

            // Check for wall collision
            if (head.X < 0 || head.X >= GridWidth || head.Y < 0 || head.Y >= GridHeight)
            {
                EndGame();
                return;
            }

            // Check for self collision
            if (Snake.CheckSelfCollision())
            {
                EndGame();
                return;
            }

            // Check for food collision
            if (Food != null && head.X == Food.X && head.Y == Food.Y)
            {
                EatFood();
            }
        }

        /// <summary>
        /// Handles food consumption
        /// </summary>
        private void EatFood()
        {
            // Get points based on food type
            int points = Food.GetPoints();

            // Apply food type effects
            switch (Food.Type)
            {
                case FoodType.FastFood:
                    speedBoostTimer = 20; // 20 game ticks of speed boost
                    SpeedChanged?.Invoke(1); // Notify speed increase
                    break;

                case FoodType.SlowFood:
                    speedSlowTimer = 20; // 20 game ticks of speed reduction
                    SpeedChanged?.Invoke(-1); // Notify speed decrease
                    break;
            }

            // Grow snake and update score
            Snake.Grow();
            Score += points;
            FoodEatenCount++;

            // FIX: Check and update high score BEFORE notifying UI
            // This now updates the static session high score
            if (Score > sessionHighScore)
            {
                sessionHighScore = Score;
                HighScoreUpdated?.Invoke(sessionHighScore);
            }

            // Notify UI of score update and food type
            ScoreUpdated?.Invoke(Score);
            FoodEaten?.Invoke(Food.Type);

            // Check for level up - FIX: Leveling up should NOT affect high score
            if (FoodEatenCount >= FOOD_PER_LEVEL)
            {
                Level++;
                FoodEatenCount = 0;
                BaseSpeed = Math.Max(50, BaseSpeed - 20); // Increase speed with level
                LevelUpdated?.Invoke(Level);
            }

            // Generate new food
            GenerateFood();
        }

        /// <summary>
        /// Updates temporary speed effects
        /// </summary>
        private void UpdateSpeedEffects()
        {
            if (speedBoostTimer > 0) speedBoostTimer--;
            if (speedSlowTimer > 0) speedSlowTimer--;

            // Reset speed effect notification when both timers expire
            if (speedBoostTimer == 0 && speedSlowTimer == 0)
            {
                SpeedChanged?.Invoke(0); // Speed back to normal
            }
        }

        /// <summary>
        /// Generates food at a random empty position
        /// </summary>
        private void GenerateFood()
        {
            int x, y;
            bool positionValid;
            int attempts = 0;

            do
            {
                x = random.Next(GridWidth);
                y = random.Next(GridHeight);
                positionValid = true;

                // Check if food would spawn on the snake
                foreach (var segment in Snake.Body)
                {
                    if (segment.X == x && segment.Y == y)
                    {
                        positionValid = false;
                        break;
                    }
                }

                attempts++;
                // Emergency fallback to prevent infinite loop
                if (attempts > 100)
                {
                    x = 5;
                    y = 5;
                    positionValid = true;
                    break;
                }
            } while (!positionValid);

            // Determine food type based on probability
            FoodType type;
            int chance = random.Next(100);

            if (chance < 60)       // 60% normal food
                type = FoodType.Normal;
            else if (chance < 80)  // 20% bonus food
                type = FoodType.Bonus;
            else if (chance < 90)  // 10% fast food
                type = FoodType.FastFood;
            else                   // 10% slow food
                type = FoodType.SlowFood;

            Food = new Food(x, y, type);
        }

        /// <summary>
        /// Changes snake direction with 180-degree turn prevention
        /// </summary>
        public void ChangeDirection(Direction newDirection)
        {
            // Prevent 180-degree turns (snake can't reverse direction instantly)
            if ((currentDirection == Direction.Up && newDirection == Direction.Down) ||
                (currentDirection == Direction.Down && newDirection == Direction.Up) ||
                (currentDirection == Direction.Left && newDirection == Direction.Right) ||
                (currentDirection == Direction.Right && newDirection == Direction.Left))
            {
                return;
            }

            // Buffer the new direction (applied in next Update call)
            nextDirection = newDirection;
        }

        /// <summary>
        /// Gets current game speed considering effects
        /// </summary>
        public int GetGameSpeed()
        {
            int speed = BaseSpeed;

            // Apply speed effects
            if (speedBoostTimer > 0)
                speed = Math.Max(30, speed - 50); // Faster
            else if (speedSlowTimer > 0)
                speed = Math.Min(300, speed + 100); // Slower

            return speed;
        }

        /// <summary>
        /// Ends the current game
        /// </summary>
        private void EndGame()
        {
            IsGameOver = true;
            GameOver?.Invoke();
        }

        /// <summary>
        /// Restarts the game
        /// </summary>
        public void Restart()
        {
            InitializeGame();
        }

        /// <summary>
        /// FIX: Added method to get current session high score
        /// </summary>
        public int GetSessionHighScore()
        {
            return sessionHighScore;
        }
    }
}