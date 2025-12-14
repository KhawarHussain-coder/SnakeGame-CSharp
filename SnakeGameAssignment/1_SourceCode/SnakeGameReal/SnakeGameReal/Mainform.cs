// MainForm.cs - WITH SESSION HIGH SCORE, FIXED GAME OVER, AND 3-2-1-GO COUNTDOWN
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace SnakeGame
{
    /// <summary>
    /// Main game form - Clean modern layout with countdown and game over message
    /// </summary>
    public class MainForm : Form
    {

        // Game engine and timer
        private GameEngine gameEngine;
        private Timer gameTimer;
        private Timer countdownTimer;
        // Sound variables - Only game over sound
        private System.Media.SoundPlayer gameOverSound;
        private string soundFolder = "Sounds";
        private bool soundsEnabled = true;
        // Game state
        private int score;
        private int highScore = 0;          // Persistent high score
        private int sessionHighScore = 0;   // Current session high score
        private bool isPaused;
        private bool gameOver;
        private int level;
        private string highScoreFile = "highscore.txt";
        private int countdownValue = 3; // 3, 2, 1, GO countdown
        private bool isCountingDown = false;

        // Game constants
        private const int GridSize = 20;
        private const int CellSize = 20;
        private const int GameAreaWidth = 400;
        private const int GameAreaHeight = 400;

        // UI Controls - CLEAN LAYOUT
        private Panel gamePanel;

        // Score Panel - TOP BAR
        private Panel scorePanel;
        private Label lblScoreTitle;
        private Label lblScoreValue;
        private Label lblHighScoreTitle;
        private Label lblHighScoreValue;
        private Label lblSessionHighScoreTitle;
        private Label lblSessionHighScoreValue;
        private Label lblLevelTitle;
        private Label lblLevelValue;
        private Label lblFoodCounter;

        // Control Panel - RIGHT SIDE
        private Panel controlPanel;
        private Button btnStart;
        private Button btnPause;
        private Button btnRestart;
        private Button btnSettings;
        private Label lblSpeedEffect;

        // Info Panel - BOTTOM
        private Panel infoPanel;
        private Label lblInstructions;

        // Settings Panel - MODAL
        private Panel settingsPanel;
        private Label lblSettingsTitle;
        private ComboBox cmbSnakeColor;
        private ComboBox cmbBackgroundColor;
        private CheckBox chkShowGrid;
        private Button btnCloseSettings;

        // Game Over Panel
        private Panel gameOverPanel;
        private Label lblGameOverTitle;
        private Label lblGameOverScore;
        private Label lblHighScoreAchieved;
        private Label lblCollisionMessage;

        // Food Info Panel
        private Panel foodInfoPanel;

        // Countdown Label
        private Label lblCountdown;

        // Game state
        private bool gameRunning = false;
        private bool settingsOpen = false;

        // Customization
        private Color snakeColor = Color.LimeGreen;
        private Color snakeHeadColor = Color.Green;
        private Color gridColor = Color.FromArgb(40, 40, 40);
        private bool showGrid = true;
        private Color backgroundColor = Color.FromArgb(240, 240, 245);
        private Color panelColor = Color.White;
        private Color accentColor = Color.FromArgb(30, 144, 255);

        /// <summary>
        /// Main form constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            LoadHighScore();
            InitializeGame();
            InitializeSound();
            this.KeyPreview = true;
        }

        /// <summary>
        /// Initializes game over sound effect
        /// </summary>
        private void InitializeSound()
        {
            try
            {
                // Create sounds directory if it doesn't exist
                if (!Directory.Exists(soundFolder))
                {
                    Directory.CreateDirectory(soundFolder);
                }

                // Try to load game over sound from file
                string gameOverSoundPath = Path.Combine(soundFolder, "game-over-kid-voice-clip-352738.wav");

                // Check if the sound file exists at the specified location
                if (File.Exists(gameOverSoundPath))
                {
                    gameOverSound = new System.Media.SoundPlayer(gameOverSoundPath);
                    soundsEnabled = true;
                }
                else
                {
                    // File doesn't exist, disable sounds
                    gameOverSound = null;
                    soundsEnabled = false;
                    Console.WriteLine($"Game over sound file not found at: {gameOverSoundPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing sound: {ex.Message}");
                soundsEnabled = false;
                gameOverSound = null;
            }
        }

        /// <summary>
        /// Load high score from file
        /// </summary>
        private void LoadHighScore()
        {
            try
            {
                if (File.Exists(highScoreFile))
                {
                    string scoreText = File.ReadAllText(highScoreFile);
                    if (int.TryParse(scoreText, out int loadedScore))
                    {
                        highScore = loadedScore;
                        sessionHighScore = loadedScore; // Initialize session high score
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading high score: {ex.Message}");
                highScore = 0;
                sessionHighScore = 0;
            }
        }

        /// <summary>
        /// Save high score to file
        /// </summary>
        private void SaveHighScore()
        {
            try
            {
                File.WriteAllText(highScoreFile, highScore.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving high score: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles arrow key input for snake control
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (gameRunning && !gameOver && gameEngine != null && !isCountingDown)
            {
                switch (keyData)
                {
                    case Keys.Up:
                        gameEngine.ChangeDirection(Direction.Up);
                        return true;

                    case Keys.Down:
                        gameEngine.ChangeDirection(Direction.Down);
                        return true;

                    case Keys.Left:
                        gameEngine.ChangeDirection(Direction.Left);
                        return true;

                    case Keys.Right:
                        gameEngine.ChangeDirection(Direction.Right);
                        return true;

                    case Keys.Space:
                        TogglePause();
                        return true;

                    case Keys.Escape:
                        if (isPaused || gameOver)
                            RestartGame();
                        return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Initializes form components with clean layout
        /// </summary>
        private void InitializeComponent()
        {
            // Form Properties
            this.Text = "Snake Game";
            this.ClientSize = new Size(900, 650); // Increased width for session high score
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = backgroundColor;
            this.Font = new Font("Segoe UI", 9);

            // ========== SCORE PANEL (TOP) ==========
            scorePanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(860, 80), // Increased width
                BackColor = panelColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Score
            lblScoreTitle = new Label
            {
                Location = new Point(20, 15),
                Size = new Size(80, 25),
                Text = "SCORE",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblScoreValue = new Label
            {
                Location = new Point(20, 40),
                Size = new Size(80, 30),
                Text = "0",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // High Score (All-time)
            lblHighScoreTitle = new Label
            {
                Location = new Point(120, 15),
                Size = new Size(120, 25),
                Text = "ALL-TIME HIGH",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblHighScoreValue = new Label
            {
                Location = new Point(120, 40),
                Size = new Size(120, 30),
                Text = highScore.ToString(),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkOrange,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Session High Score
            lblSessionHighScoreTitle = new Label
            {
                Location = new Point(260, 15),
                Size = new Size(120, 25),
                Text = "SESSION HIGH",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSessionHighScoreValue = new Label
            {
                Location = new Point(260, 40),
                Size = new Size(120, 30),
                Text = sessionHighScore.ToString(),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Purple,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Level
            lblLevelTitle = new Label
            {
                Location = new Point(400, 15),
                Size = new Size(80, 25),
                Text = "LEVEL",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblLevelValue = new Label
            {
                Location = new Point(400, 40),
                Size = new Size(80, 30),
                Text = "1",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Food Counter
            lblFoodCounter = new Label
            {
                Location = new Point(500, 15),
                Size = new Size(150, 50),
                Text = "Food: 0/5",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Purple,
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Speed Effect
            lblSpeedEffect = new Label
            {
                Location = new Point(670, 15),
                Size = new Size(170, 50),
                Text = "",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                BackColor = Color.LightYellow,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Snake Name Label - CHANGED FROM "Ehtisham" TO "Teacher"
            var lblSnakeName = new Label
            {
                Location = new Point(670, 15),
                Size = new Size(200, 50),
                Text = "Snake: Teacher", // CHANGED HERE
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                TextAlign = ContentAlignment.MiddleLeft
            };

            scorePanel.Controls.AddRange(new Control[] {
                lblScoreTitle, lblScoreValue,
                lblHighScoreTitle, lblHighScoreValue,
                lblSessionHighScoreTitle, lblSessionHighScoreValue,
                lblLevelTitle, lblLevelValue,
                lblFoodCounter,
                lblSpeedEffect,
                lblSnakeName
            });

            // ========== GAME PANEL (CENTER LEFT) ==========
            gamePanel = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(GameAreaWidth, GameAreaHeight),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };
            gamePanel.Paint += GamePanel_Paint;

            // ========== COUNTDOWN LABEL ==========
            lblCountdown = new Label
            {
                Location = new Point(0, 0),
                Size = new Size(GameAreaWidth, GameAreaHeight),
                Text = "",
                Font = new Font("Segoe UI", 72, FontStyle.Bold),
                ForeColor = Color.Yellow,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            gamePanel.Controls.Add(lblCountdown);

            // ========== CONTROL PANEL (CENTER RIGHT) ==========
            controlPanel = new Panel
            {
                Location = new Point(440, 120),
                Size = new Size(440, 300), // Adjusted width
                BackColor = panelColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Control buttons
            btnStart = CreateModernButton("START GAME", new Point(30, 30), Color.FromArgb(76, 175, 80), Color.White, 180, 45);
            btnStart.Click += BtnStart_Click;

            btnPause = CreateModernButton("PAUSE", new Point(230, 30), Color.FromArgb(255, 193, 7), Color.Black, 180, 45);
            btnPause.Click += BtnPause_Click;
            btnPause.Enabled = false;

            btnRestart = CreateModernButton("RESTART", new Point(30, 95), Color.FromArgb(244, 67, 54), Color.White, 180, 45);
            btnRestart.Click += BtnRestart_Click;

            btnSettings = CreateModernButton("SETTINGS", new Point(230, 95), accentColor, Color.White, 180, 45);
            btnSettings.Click += BtnSettings_Click;

            // Food Info Panel
            foodInfoPanel = new Panel
            {
                Location = new Point(30, 160),
                Size = new Size(380, 120),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            CreateFoodInfoPanel();

            controlPanel.Controls.AddRange(new Control[] {
                btnStart, btnPause, btnRestart, btnSettings, foodInfoPanel
            });

            // ========== INFO PANEL (BOTTOM) ==========
            infoPanel = new Panel
            {
                Location = new Point(20, 540),
                Size = new Size(860, 90),
                BackColor = panelColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblInstructions = new Label
            {
                Location = new Point(20, 15),
                Size = new Size(820, 60),
                Text = "CONTROLS: Arrow Keys to Move • SPACE to Pause • ESC to Restart\n" +
                      "OBJECTIVE: Eat food to grow and score points. Avoid walls and yourself.\n" +
                      "LEVEL UP: Eat 5 foods to advance to next level. Speed increases each level.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DarkSlateGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoPanel.Controls.Add(lblInstructions);

            // ========== SETTINGS PANEL (MODAL OVERLAY) ==========
            settingsPanel = new Panel
            {
                Location = new Point(150, 150),
                Size = new Size(600, 350),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            lblSettingsTitle = new Label
            {
                Location = new Point(0, 20),
                Size = new Size(600, 40),
                Text = "GAME SETTINGS",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = accentColor,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Snake Color
            var lblSnakeColor = new Label
            {
                Location = new Point(50, 80),
                Size = new Size(200, 30),
                Text = "Snake Color:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbSnakeColor = new ComboBox
            {
                Location = new Point(300, 80),
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbSnakeColor.Items.AddRange(new object[] { "Green", "Blue", "Red", "Purple", "Orange" });
            cmbSnakeColor.SelectedIndex = 0;
            cmbSnakeColor.SelectedIndexChanged += CmbSnakeColor_Changed;

            // Grid Color
            var lblGridColor = new Label
            {
                Location = new Point(50, 130),
                Size = new Size(200, 30),
                Text = "Grid Color:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            cmbBackgroundColor = new ComboBox
            {
                Location = new Point(300, 130),
                Size = new Size(250, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbBackgroundColor.Items.AddRange(new object[] { "Gray Grid", "Dark Grid", "Blue Grid", "Green Grid", "No Grid" });
            cmbBackgroundColor.SelectedIndex = 0;
            cmbBackgroundColor.SelectedIndexChanged += CmbBackgroundColor_Changed;

            // Show Grid
            chkShowGrid = new CheckBox
            {
                Location = new Point(50, 180),
                Size = new Size(200, 30),
                Text = "Show Grid Lines",
                Font = new Font("Segoe UI", 11),
                Checked = true
            };
            chkShowGrid.CheckedChanged += ChkShowGrid_Changed;

            btnCloseSettings = CreateModernButton("CLOSE SETTINGS", new Point(225, 250), Color.Gray, Color.White, 150, 40);
            btnCloseSettings.Click += BtnCloseSettings_Click;

            settingsPanel.Controls.AddRange(new Control[] {
                lblSettingsTitle, lblSnakeColor, cmbSnakeColor,
                lblGridColor, cmbBackgroundColor, chkShowGrid,
                btnCloseSettings
            });

            // ========== GAME OVER PANEL ==========
            gameOverPanel = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(GameAreaWidth, GameAreaHeight),
                BackColor = Color.FromArgb(220, 0, 0, 0),
                Visible = false
            };

            lblGameOverTitle = new Label
            {
                Location = new Point(0, 80),
                Size = new Size(400, 60),
                Text = "GAME OVER",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblGameOverScore = new Label
            {
                Location = new Point(0, 150),
                Size = new Size(400, 40),
                Text = "Score: 0",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.Yellow,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblCollisionMessage = new Label
            {
                Location = new Point(0, 200),
                Size = new Size(400, 30),
                Text = "",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblHighScoreAchieved = new Label
            {
                Location = new Point(0, 240),
                Size = new Size(400, 40),
                Text = "🏆 NEW HIGH SCORE!",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Gold,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            gameOverPanel.Controls.AddRange(new Control[] {
                lblGameOverTitle, lblGameOverScore, lblCollisionMessage, lblHighScoreAchieved
            });

            // ========== ADD ALL CONTROLS TO FORM ==========
            this.Controls.Add(scorePanel);
            this.Controls.Add(gamePanel);
            this.Controls.Add(controlPanel);
            this.Controls.Add(infoPanel);
            this.Controls.Add(settingsPanel);
            this.Controls.Add(gameOverPanel);
            gamePanel.BringToFront(); // Ensure game panel is on top

            // ========== GAME TIMER ==========
            gameTimer = new Timer
            {
                Interval = 150
            };
            gameTimer.Tick += GameTimer_Tick;

            // ========== COUNTDOWN TIMER ==========
            countdownTimer = new Timer
            {
                Interval = 1000 // 1 second
            };
            countdownTimer.Tick += CountdownTimer_Tick;

            // Focus the form
            this.Click += (s, e) => this.Focus();
        }

        /// <summary>
        /// Plays game over sound
        /// </summary>
        private void PlayGameOverSound()
        {
            if (!soundsEnabled || gameOverSound == null) return;

            try
            {
                gameOverSound.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing game over sound: {ex.Message}");
            }
        }

        /// <summary>
        /// Countdown timer tick event
        /// </summary>
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (countdownValue > 1)
            {
                // Show 3, 2, 1
                lblCountdown.Text = countdownValue.ToString();
                lblCountdown.Visible = true;

                // Animation effect - numbers get larger
                lblCountdown.Font = new Font("Segoe UI", 80 + (3 - countdownValue) * 10, FontStyle.Bold);
                lblCountdown.ForeColor = Color.Yellow;

                countdownValue--;
                gamePanel.Invalidate();
            }
            else if (countdownValue == 1)
            {
                // Show "GO!" instead of "1"
                lblCountdown.Text = "GO!";
                lblCountdown.Font = new Font("Segoe UI", 100, FontStyle.Bold);
                lblCountdown.ForeColor = Color.LimeGreen;
                gamePanel.Invalidate();
                countdownValue--;
            }
            else
            {
                // Countdown finished - start the game
                countdownTimer.Stop();
                lblCountdown.Visible = false;
                isCountingDown = false;
                gameRunning = true;
                gameTimer.Start();

                // Focus for keyboard input
                this.Focus();
            }
        }

        /// <summary>
        /// Helper method to create modern buttons
        /// </summary>
        private Button CreateModernButton(string text, Point location, Color backColor, Color foreColor, int width = 0, int height = 0)
        {
            var btn = new Button
            {
                Location = location,
                Size = width > 0 && height > 0 ? new Size(width, height) : new Size(120, 40),
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                TabStop = false,
                Cursor = Cursors.Hand
            };

            // Add hover effect
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(
                Math.Min(backColor.R + 20, 255),
                Math.Min(backColor.G + 20, 255),
                Math.Min(backColor.B + 20, 255));
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            return btn;
        }

        /// <summary>
        /// Creates food info panel
        /// </summary>
        private void CreateFoodInfoPanel()
        {
            // Panel is now 120px tall
            var lblTitle = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(360, 25),
                Text = "FOOD TYPES",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Create food type indicators with comfortable spacing
            int yPos = 40;
            int spacing = 20;

            AddFoodInfoItem("● Normal (Student A)", "+10 points", Color.Red, yPos);
            yPos += spacing;
            AddFoodInfoItem("● Bonus (Student B)", "+50 points", Color.Gold, yPos);
            yPos += spacing;
            AddFoodInfoItem("● Speed (Student C)", "Increases speed", Color.HotPink, yPos);
            yPos += spacing;
            AddFoodInfoItem("● Slow (Student D)", "Decreases speed", Color.Cyan, yPos);

            foodInfoPanel.Controls.Add(lblTitle);
        }

        /// <summary>
        /// Adds a food info item to the panel
        /// </summary>
        private void AddFoodInfoItem(string name, string effect, Color color, int yPos)
        {
            var lblName = new Label
            {
                Location = new Point(20, yPos),
                Size = new Size(140, 20),
                Text = name,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblEffect = new Label
            {
                Location = new Point(170, yPos),
                Size = new Size(190, 20),
                Text = effect,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.DarkSlateGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            foodInfoPanel.Controls.Add(lblName);
            foodInfoPanel.Controls.Add(lblEffect);
        }

        /// <summary>
        /// Initializes a new game
        /// </summary>
        private void InitializeGame()
        {
            score = 0;
            level = 1;
            isPaused = false;
            gameOver = false;
            gameRunning = false;
            isCountingDown = false;
            countdownValue = 3; // Reset countdown

            // Update UI
            lblScoreValue.Text = "0";
            lblLevelValue.Text = "1";
            lblFoodCounter.Text = "Food: 0/5";
            lblSpeedEffect.Visible = false;
            lblHighScoreValue.Text = highScore.ToString();
            lblSessionHighScoreValue.Text = sessionHighScore.ToString();

            // Hide game over panel and countdown
            gameOverPanel.Visible = false;
            lblHighScoreAchieved.Visible = false;
            lblCollisionMessage.Text = "";
            lblCountdown.Visible = false;

            // Create new game engine
            gameEngine = new GameEngine(GridSize, GridSize);

            // Subscribe to events
            gameEngine.ScoreUpdated += GameEngine_ScoreUpdated;
            gameEngine.LevelUpdated += GameEngine_LevelUpdated;
            gameEngine.FoodEaten += GameEngine_FoodEaten;
            gameEngine.SpeedChanged += GameEngine_SpeedChanged;
            gameEngine.GameOver += GameEngine_GameOver;
            gameEngine.HighScoreUpdated += GameEngine_HighScoreUpdated;

            gamePanel.Invalidate();
        }

        // ========== EVENT HANDLERS ==========

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            DrawGame(e.Graphics);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!isPaused && !gameOver && gameRunning && !isCountingDown)
            {
                gameEngine.Update();
                gamePanel.Invalidate();
            }
        }

        /// <summary>
        /// Handles high score updates
        /// </summary>
        private void GameEngine_HighScoreUpdated(int newHighScore)
        {
            // Update all-time high score
            if (newHighScore > highScore)
            {
                highScore = newHighScore;
                lblHighScoreValue.Text = highScore.ToString();
                SaveHighScore();

                // Highlight all-time high score
                lblHighScoreValue.ForeColor = Color.DarkOrange;

                // Flash effect
                Timer flashTimer = new Timer { Interval = 200 };
                int flashCount = 0;
                flashTimer.Tick += (s, ev) =>
                {
                    flashCount++;
                    if (flashCount > 6)
                    {
                        flashTimer.Stop();
                        flashTimer.Dispose();
                        lblHighScoreValue.ForeColor = Color.DarkOrange;
                    }
                    else
                    {
                        lblHighScoreValue.ForeColor = (flashCount % 2 == 0) ? Color.DarkOrange : Color.Gold;
                    }
                };
                flashTimer.Start();
            }

            // Update session high score
            if (newHighScore > sessionHighScore)
            {
                sessionHighScore = newHighScore;
                lblSessionHighScoreValue.Text = sessionHighScore.ToString();

                // Highlight session high score
                lblSessionHighScoreValue.ForeColor = Color.Purple;

                // Flash effect for session high score
                Timer sessionFlashTimer = new Timer { Interval = 200 };
                int sessionFlashCount = 0;
                sessionFlashTimer.Tick += (s, ev) =>
                {
                    sessionFlashCount++;
                    if (sessionFlashCount > 6)
                    {
                        sessionFlashTimer.Stop();
                        sessionFlashTimer.Dispose();
                        lblSessionHighScoreValue.ForeColor = Color.Purple;
                    }
                    else
                    {
                        lblSessionHighScoreValue.ForeColor = (sessionFlashCount % 2 == 0) ? Color.Purple : Color.Magenta;
                    }
                };
                sessionFlashTimer.Start();
            }
        }

        private void GameEngine_ScoreUpdated(int newScore)
        {
            score = newScore;
            lblScoreValue.Text = score.ToString();
        }

        private void GameEngine_LevelUpdated(int newLevel)
        {
            level = newLevel;
            lblLevelValue.Text = level.ToString();
            gameTimer.Interval = gameEngine.GetGameSpeed();
        }

        private void GameEngine_FoodEaten(FoodType foodType)
        {
            int foodEaten = gameEngine.FoodEatenCount;
            int neededForNextLevel = 5 - (foodEaten % 5);
            lblFoodCounter.Text = $"Food: {foodEaten}/5 (Next: {neededForNextLevel})";
        }

        private void GameEngine_SpeedChanged(int effect)
        {
            if (effect > 0)
            {
                lblSpeedEffect.Text = "⚡ SPEED BOOST!";
                lblSpeedEffect.ForeColor = Color.DarkRed;
                lblSpeedEffect.BackColor = Color.LightPink;
                lblSpeedEffect.Visible = true;
            }
            else if (effect < 0)
            {
                lblSpeedEffect.Text = "🐌 SPEED SLOW!";
                lblSpeedEffect.ForeColor = Color.DarkBlue;
                lblSpeedEffect.BackColor = Color.LightCyan;
                lblSpeedEffect.Visible = true;
            }
            else
            {
                lblSpeedEffect.Visible = false;
            }
            gameTimer.Interval = gameEngine.GetGameSpeed();
        }

        /// <summary>
        /// Game over event - show collision message
        /// </summary>
        private void GameEngine_GameOver()
        {
            gameOver = true;
            gameRunning = false;
            gameTimer.Stop();

            // Play game over sound
            PlayGameOverSound();

            btnPause.Enabled = false;
            btnStart.Enabled = false;

            // Determine collision type for message
            string collisionMessage = GetCollisionMessage();

            // Show game over panel with collision message
            gameOverPanel.Visible = true;
            gameOverPanel.BringToFront(); // Ensure it's on top
            lblGameOverTitle.Text = "GAME OVER";
            lblGameOverScore.Text = $"Score: {score}";
            lblCollisionMessage.Text = collisionMessage;

            // Check if this was a new high score
            bool newAllTimeHigh = (score >= highScore && score > 0);
            bool newSessionHigh = (score >= sessionHighScore && score > 0);

            if (newAllTimeHigh)
            {
                lblHighScoreAchieved.Text = "🏆 NEW ALL-TIME HIGH SCORE!";
                lblHighScoreAchieved.Visible = true;
            }
            else if (newSessionHigh)
            {
                lblHighScoreAchieved.Text = "🎯 NEW SESSION HIGH SCORE!";
                lblHighScoreAchieved.Visible = true;
            }
            else
            {
                lblHighScoreAchieved.Visible = false;
            }

            gamePanel.Invalidate();
        }

        /// <summary>
        /// Determines the collision message based on what happened
        /// </summary>
        private string GetCollisionMessage()
        {
            if (gameEngine == null || gameEngine.Snake == null)
                return "Game Over!";

            var head = gameEngine.Snake.Head;

            // Check for wall collision
            if (head.X < 0 || head.X >= GridSize || head.Y < 0 || head.Y >= GridSize)
            {
                return "You hit the wall!";
            }

            // Check for self collision
            if (gameEngine.Snake.CheckSelfCollision())
            {
                return "You collided with yourself!";
            }

            return "Game Over!";
        }

        /// <summary>
        /// Draws the game graphics
        /// </summary>
        private void DrawGame(Graphics g)
        {
            if (gameEngine == null)
            {
                DrawStartScreen(g);
                return;
            }

            g.Clear(Color.Black);

            if (showGrid)
            {
                DrawGrid(g);
            }

            DrawSnake(g);
            DrawFood(g);
        }

        private void DrawStartScreen(Graphics g)
        {
            g.Clear(Color.Black);

            g.DrawString("SNAKE GAME",
                new Font("Segoe UI", 32, FontStyle.Bold),
                Brushes.Yellow,
                60, 150);

            g.DrawString("Press START to begin",
                new Font("Segoe UI", 14),
                Brushes.White,
                100, 220);
        }

        private void DrawGrid(Graphics g)
        {
            using (var gridPen = new Pen(gridColor))
            {
                for (int x = 0; x <= GridSize; x++)
                {
                    g.DrawLine(gridPen,
                        x * CellSize, 0,
                        x * CellSize, GameAreaHeight);
                }

                for (int y = 0; y <= GridSize; y++)
                {
                    g.DrawLine(gridPen,
                        0, y * CellSize,
                        GameAreaWidth, y * CellSize);
                }
            }
        }

        private void DrawSnake(Graphics g)
        {
            for (int i = 0; i < gameEngine.Snake.Body.Count; i++)
            {
                var segment = gameEngine.Snake.Body[i];
                var rect = new Rectangle(
                    segment.X * CellSize,
                    segment.Y * CellSize,
                    CellSize,
                    CellSize);

                if (i == 0) // Head
                {
                    g.FillRectangle(new SolidBrush(snakeHeadColor), rect);
                    g.DrawRectangle(new Pen(Color.DarkGreen, 2), rect);


                    g.DrawString(gameEngine.Snake.Name, // This should now be set to "Teacher" in the GameEngine
                        new Font("Arial", 8, FontStyle.Bold),
                        Brushes.White,
                        rect.X + 2,
                        rect.Y + 2);

                    // Eyes
                    int eyeSize = CellSize / 4;
                    g.FillEllipse(Brushes.White,
                        rect.X + eyeSize,
                        rect.Y + eyeSize,
                        eyeSize, eyeSize);
                    g.FillEllipse(Brushes.White,
                        rect.X + rect.Width - 2 * eyeSize,
                        rect.Y + eyeSize,
                        eyeSize, eyeSize);
                }
                else // Body
                {
                    g.FillRectangle(new SolidBrush(snakeColor), rect);
                    g.DrawRectangle(Pens.DarkGreen, rect);
                }
            }
        }

        private void DrawFood(Graphics g)
        {
            if (gameEngine.Food != null)
            {
                var foodRect = new Rectangle(
                    gameEngine.Food.X * CellSize + 2,
                    gameEngine.Food.Y * CellSize + 2,
                    CellSize - 4,
                    CellSize - 4);

                Brush foodBrush;
                switch (gameEngine.Food.Type)
                {
                    case FoodType.Bonus:
                        foodBrush = Brushes.Gold;
                        break;
                    case FoodType.FastFood:
                        foodBrush = Brushes.HotPink;
                        break;
                    case FoodType.SlowFood:
                        foodBrush = Brushes.Cyan;
                        break;
                    default:
                        foodBrush = Brushes.Red; // Normal food
                        break;
                }

                g.FillEllipse(foodBrush, foodRect);
                g.DrawEllipse(Pens.DarkRed, foodRect);

                // Draw student name on the food
                string displayText = gameEngine.Food.StudentName;
                var nameFont = new Font("Arial", 6, FontStyle.Bold);

                // Adjust based on food type
                if (gameEngine.Food.Type == FoodType.Bonus)
                {
                    g.DrawString("★",
                        new Font("Arial", 10),
                        Brushes.Yellow,
                        foodRect.X + 4,
                        foodRect.Y + 2);
                    displayText = "BONUS";
                }
                else if (gameEngine.Food.Type == FoodType.FastFood)
                {
                    displayText = "FAST";
                }
                else if (gameEngine.Food.Type == FoodType.SlowFood)
                {
                    displayText = "SLOW";
                }

                // Draw the text on the food
                g.DrawString(displayText,
                    nameFont,
                    Brushes.White,
                    foodRect.X + 2,
                    foodRect.Y + foodRect.Height / 2 - 6);
            }
        }

        // ========== BUTTON HANDLERS ==========

        private void BtnStart_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            TogglePause();
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            RestartGame();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            ToggleSettings();
        }

        private void BtnCloseSettings_Click(object sender, EventArgs e)
        {
            ToggleSettings();
        }

        // ========== SETTINGS HANDLERS ==========

        private void CmbSnakeColor_Changed(object sender, EventArgs e)
        {
            switch (cmbSnakeColor.SelectedItem.ToString())
            {
                case "Green":
                    snakeColor = Color.LimeGreen;
                    snakeHeadColor = Color.Green;
                    break;
                case "Blue":
                    snakeColor = Color.LightBlue;
                    snakeHeadColor = Color.Blue;
                    break;
                case "Red":
                    snakeColor = Color.Pink;
                    snakeHeadColor = Color.Red;
                    break;
                case "Purple":
                    snakeColor = Color.Violet;
                    snakeHeadColor = Color.Purple;
                    break;
                case "Orange":
                    snakeColor = Color.LightSalmon;
                    snakeHeadColor = Color.Orange;
                    break;
            }
            gamePanel.Invalidate();
        }

        private void CmbBackgroundColor_Changed(object sender, EventArgs e)
        {
            switch (cmbBackgroundColor.SelectedItem.ToString())
            {
                case "Gray Grid":
                    gridColor = Color.FromArgb(40, 40, 40);
                    showGrid = true;
                    chkShowGrid.Checked = true;
                    break;
                case "Dark Grid":
                    gridColor = Color.FromArgb(20, 20, 20);
                    showGrid = true;
                    chkShowGrid.Checked = true;
                    break;
                case "Blue Grid":
                    gridColor = Color.FromArgb(30, 30, 60);
                    showGrid = true;
                    chkShowGrid.Checked = true;
                    break;
                case "Green Grid":
                    gridColor = Color.FromArgb(30, 60, 30);
                    showGrid = true;
                    chkShowGrid.Checked = true;
                    break;
                case "No Grid":
                    showGrid = false;
                    chkShowGrid.Checked = false;
                    break;
            }
            gamePanel.Invalidate();
        }

        private void ChkShowGrid_Changed(object sender, EventArgs e)
        {
            showGrid = chkShowGrid.Checked;
            gamePanel.Invalidate();
        }

        // ========== GAME CONTROL METHODS ==========

        /// <summary>
        /// Starts the game with countdown
        /// </summary>
        private void StartGame()
        {
            if (gameOver || isCountingDown) return;

            btnStart.Enabled = false;
            btnPause.Enabled = true;

            // Start countdown
            isCountingDown = true;
            countdownValue = 3;
            lblCountdown.Text = countdownValue.ToString();
            lblCountdown.Visible = true;
            lblCountdown.Font = new Font("Segoe UI", 80, FontStyle.Bold);
            lblCountdown.ForeColor = Color.Yellow;

            // Set game timer speed
            gameTimer.Interval = gameEngine.GetGameSpeed();

            // Start countdown timer
            countdownTimer.Start();

            // Hide settings if open
            if (settingsOpen)
            {
                ToggleSettings();
            }

            this.Focus();
            gamePanel.Invalidate();
        }

        private void TogglePause()
        {
            if (isCountingDown) return;

            isPaused = !isPaused;
            btnPause.Text = isPaused ? "RESUME" : "PAUSE";
            btnPause.BackColor = isPaused ?
                Color.FromArgb(76, 175, 80) :
                Color.FromArgb(255, 193, 7);
        }

        /// <summary>
        /// Restarts the game
        /// </summary>
        private void RestartGame()
        {
            // Stop all timers
            gameTimer.Stop();
            countdownTimer.Stop();

            // Reset countdown state
            isCountingDown = false;
            lblCountdown.Visible = false;

            InitializeGame();

            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnPause.Text = "PAUSE";
            btnPause.BackColor = Color.FromArgb(255, 193, 7);

            this.Focus();
        }

        /// <summary>
        /// Toggles settings panel visibility
        /// </summary>
        private void ToggleSettings()
        {
            settingsOpen = !settingsOpen;
            settingsPanel.Visible = settingsOpen;
            settingsPanel.BringToFront();

            // Disable/enable other controls when settings are open
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl != settingsPanel)
                {
                    ctrl.Enabled = !settingsOpen;
                }
            }
        }
    }
}