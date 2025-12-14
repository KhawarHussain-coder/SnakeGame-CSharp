// Food.cs
using System.Drawing;

namespace SnakeGame
{
    /// <summary>
    /// Types of food with different effects
    /// </summary>
    public enum FoodType
    {
        Normal,     // +10 points, normal speed
        Bonus,      // +50 points
        FastFood,   // +5 points, increases speed temporarily
        SlowFood    // +15 points, decreases speed temporarily
    }

    /// <summary>
    /// Represents food item in the game with student name
    /// </summary>
    public class Food
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Point Position => new Point(X, Y);
        public FoodType Type { get; private set; }
        public string StudentName { get; private set; } // Added student name property

        public Food(int x, int y, FoodType type = FoodType.Normal)
        {
            X = x;
            Y = y;
            Type = type;
            StudentName = GetStudentNameForFood(type); // Set student name based on food type
        }

        /// <summary>
        /// Constructor with custom student name
        /// </summary>
        public Food(int x, int y, FoodType type, string studentName)
        {
            X = x;
            Y = y;
            Type = type;
            StudentName = !string.IsNullOrWhiteSpace(studentName) ? studentName : GetStudentNameForFood(type);
        }

        /// <summary>
        /// Gets point value for this food type
        /// </summary>
        public int GetPoints()
        {
            switch (Type)
            {
                case FoodType.Normal:
                    return 10;
                case FoodType.Bonus:
                    return 50;
                case FoodType.FastFood:
                    return 5;
                case FoodType.SlowFood:
                    return 15;
                default:
                    return 10;
            }
        }

        /// <summary>
        /// Gets student name based on food type
        /// </summary>
        private string GetStudentNameForFood(FoodType type)
        {
            // Assign different student names to different food types
            switch (type)
            {
                case FoodType.Normal:
                    return "Student A";
                case FoodType.Bonus:
                    return "Student B";
                case FoodType.FastFood:
                    return "Student C";
                case FoodType.SlowFood:
                    return "Student D";
                default:
                    return "Student";
            }
        }

        /// <summary>
        /// Gets description of the food with student name
        /// </summary>
        public string GetDescription()
        {
            string effect = "";
            switch (Type)
            {
                case FoodType.Normal:
                    effect = "+10 points";
                    break;
                case FoodType.Bonus:
                    effect = "+50 points";
                    break;
                case FoodType.FastFood:
                    effect = "Speed Boost";
                    break;
                case FoodType.SlowFood:
                    effect = "Speed Slow";
                    break;
            }

            return $"{StudentName}: {effect}";
        }

        /// <summary>
        /// Gets string representation of the food
        /// </summary>
        public override string ToString()
        {
            return $"{StudentName}'s Food ({Type}) at ({X}, {Y})";
        }
    }
}