using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System.IO;
using com.shephertz.app42.paas.sdk.windows;
using System.Windows.Threading;
using Platformer.App42;
using com.shephertz.app42.paas.sdk.windows.social;

namespace Platformer
{
    public partial class GamePage : PhoneApplicationPage
    {
        ContentManager contentManager;
        GameTimer timer;
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        // Meta-level game state.
        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private TouchCollection touchState;
        private AccelerometerState accelerometerState;
        UIElementRenderer uiElementRenderer;
        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 5;
        private double scoreTillLastLevel = 0;
        public GamePage()
        {
            InitializeComponent();

            GlobalContext.totalScore = 0;
            Accelerometer.Initialize();
            // Get the content manager from the application
            contentManager = (Application.Current as App).Content;

            // Create a timer for this page
            timer = new GameTimer();
            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the sharing mode of the graphics device to turn on XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);
            uiElementRenderer = new UIElementRenderer(grdResult, 800, 480);
            // TODO: use this.content to load your game content here
            LoadContent();
            // Start the timer
            timer.Start();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Stop the timer
            timer.Stop();

            // Set the sharing mode of the graphics device to turn off XNA rendering
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);

            // Since in OnNavigatedTo we are loading next level so on FAS we have to reduce levelIndex by one
            --levelIndex;
            GlobalContext.totalScore = GlobalContext.totalScore - level.Score;
            level.Score = 0;
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Allows the page to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            // TODO: Add your update logic here
            // Handle polling for our input and handling high-level input
            HandleInput();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(e, keyboardState, gamePadState, touchState,
                         accelerometerState, TouchPanel.DisplayOrientation);
        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.CornflowerBlue);
            //graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            try
            {
                spriteBatch.Begin();
                level.Draw(e, spriteBatch);
                DrawHud();

                spriteBatch.End();
            }
            catch (Exception e1)
            {

            }
            // TODO: Add your drawing code here
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected void LoadContent()
        {

            // Load fonts
            hudFont = contentManager.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            winOverlay = contentManager.Load<Texture2D>("Overlays/you_win");
            loseOverlay = contentManager.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = contentManager.Load<Texture2D>("Overlays/you_died");

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(contentManager.Load<Song>("Sounds/Music"));
            }
            catch { }

            LoadNextLevel();
        }

        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            touchState = TouchPanel.GetState();
            accelerometerState = Accelerometer.GetState();

            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
            { } //Exit();

            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                gamePadState.IsButtonDown(Buttons.A) ||
                touchState.AnyTouch();

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (!level.ReachedExit)
                        ReloadCurrentLevel();
                    //    LoadNextLevel();
                    //else

                }
            }
            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1);
            if (levelIndex < numberOfLevels)
            {
                // Unloads the content for the current level before loading the next one.
                if (level != null)
                    level.Dispose();
                scoreTillLastLevel = GlobalContext.totalScore;
                // Load the level.
                string levelPath = string.Format("Content/{0}.txt", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    level = new Level(contentManager, fileStream, levelIndex);
            }
            else
            {
                btnContinue.Visibility = Visibility.Collapsed;
                tblCongratsMessage.Visibility = Visibility.Visible;
                //TODO:Game Over and submit the score.
            }
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        private void DrawHud()
        {
            Rectangle titleSafeArea = SharedGraphicsDeviceManager.Current.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);
            GlobalContext.totalScore = scoreTillLastLevel + level.Score;
            DrawShadowedString(hudFont, "TOTAL SCORE: " + GlobalContext.totalScore.ToString(), hudLocation + new Vector2(550.0f, 0.0f), Color.Yellow);
            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    uiElementRenderer.Render();
                    status = uiElementRenderer.Texture;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!level.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int tag = Convert.ToInt32((sender as Button).Tag.ToString());
            switch (tag)
            {
                case 0: if (level.ReachedExit)
                        LoadNextLevel();
                    break;
                case 1:
                    messageTB.Text = "Please wait...";
                    MessagePopup.Visibility = Visibility.Visible;
                    App42Api.SaveUserScore(GlobalContext.totalScore, SaveScoreCallback);
                    break;
                case 2:
                    if (GlobalContext.isFacebookAccountLinkedToApp42)
                    {
                        messageTB.Text = "Please wait...";
                        MessagePopup.Visibility = Visibility.Visible;
                        App42Api.ShareStatus("Hey!! I have scored " + GlobalContext.totalScore + " on Platformer.Check out this on marketplace" + string.Format("http://www.windowsphone.com/s?appid={0}", App.GetId()), ShareScoreCallback);
                    }
                    else
                    {
                        showMessage("Sorry!!you have to do logout and get logged in again");
                    }
                        break;
            }
            //  MessageBox.Show("Submit Score");
        }

        private void SaveScoreCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                showMessage("Exception,Please try again later");
            }
            else
            {
                com.shephertz.app42.paas.sdk.windows.game.Game game = (com.shephertz.app42.paas.sdk.windows.game.Game)response;
                if (game.IsResponseSuccess())
                {
                    showMessage("Score Saved Successfully");
                }
                else
                {
                    showMessage("Error,Please try again later");
                }
            }
        }
        private void ShareScoreCallback(object response, bool IsException)
        {
            if (IsException)
            {
                App42Exception exception = (App42Exception)response;
                showMessage("Exception,Please try again later");
            }
            else
            {
                Social social = (Social)response;
                if (social.IsResponseSuccess())
                {
                    showMessage("Score Shared Successfully");
                }
                else
                {
                    showMessage("Error,Please try again later");
                }
            }
        }
        void showMessage(string message)
        {
            messageTB.Text = message;
            MessagePopup.Visibility = Visibility.Visible;
            //uiElementRenderer.Render();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            MessagePopup.Visibility = Visibility.Collapsed;
            (sender as DispatcherTimer).Stop();
        }
    }
}