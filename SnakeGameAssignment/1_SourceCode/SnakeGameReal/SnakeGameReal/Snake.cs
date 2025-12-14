// Snake.cs
using System.Collections.Generic;
using System.Drawing;

namespace SnakeGame
{
    public class Snake
    {
        public List<Point> Body { get; private set; }
        public Point Head => Body[0];
        public Point Tail => Body[Body.Count - 1];
        public int Length => Body.Count;
        public string Name { get; private set; } // Added name property

        public Snake(int startX, int startY)
        {
            Body = new List<Point>();
            Name = "Ehtisham"; // Set the snake's name

            // Initialize with 3 segments as per requirements
            Body.Add(new Point(startX, startY));     // Head
            Body.Add(new Point(startX - 1, startY)); // Body segment 1
            Body.Add(new Point(startX - 2, startY)); // Body segment 2
        }

        /// <summary>
        /// Constructor with custom name
        /// </summary>
        public Snake(int startX, int startY, string name)
        {
            Body = new List<Point>();
            Name = name;

            // Initialize with 3 segments as per requirements
            Body.Add(new Point(startX, startY));     // Head
            Body.Add(new Point(startX - 1, startY)); // Body segment 1
            Body.Add(new Point(startX - 2, startY)); // Body segment 2
        }

        /// <summary>
        /// Moves the snake in the specified direction
        /// </summary>
        public void Move(Direction direction)
        {
            // Store current head position
            Point newHead = Head;

            // Calculate new head position based on direction
            switch (direction)
            {
                case Direction.Up:
                    newHead.Y--;
                    break;
                case Direction.Down:
                    newHead.Y++;
                    break;
                case Direction.Left:
                    newHead.X--;
                    break;
                case Direction.Right:
                    newHead.X++;
                    break;
            }

            // Move body segments (each segment takes position of previous one)
            for (int i = Body.Count - 1; i > 0; i--)
            {
                Body[i] = Body[i - 1];
            }

            // Update head with new position
            Body[0] = newHead;
        }

        /// <summary>
        /// Increases snake length by adding a new segment
        /// </summary>
        public void Grow()
        {
            // Add new segment at the tail position
            Body.Add(Tail);
        }

        /// <summary>
        /// Checks if snake collides with itself
        /// </summary>
        public bool CheckSelfCollision()
        {
            // Skip head (index 0) when checking for collisions
            for (int i = 1; i < Body.Count; i++)
            {
                if (Head.X == Body[i].X && Head.Y == Body[i].Y)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the snake's name
        /// </summary>
        public string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Sets a new name for the snake
        /// </summary>
        public void SetName(string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName))
            {
                Name = newName;
            }
        }

        /// <summary>
        /// Gets a string representation of the snake
        /// </summary>
        public override string ToString()
        {
            return $"{Name}'s Snake (Length: {Length})";
        }
    }
}