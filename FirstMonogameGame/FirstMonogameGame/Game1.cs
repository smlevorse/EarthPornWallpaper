#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.IO;
#endregion

namespace FirstMonogameGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //fields
        GameState state;
        SpriteFont ariel14;
        Texture2D playerTexture;
        Texture2D collectibleTexture;

        Player player;
        List<Collectible> collectibles;

        int level;
        double timer;
        KeyboardState kbState;
        KeyboardState previousKbState;

        Random rand;

        enum GameState
        {
            Menu,
            Game,
            GameOver
        }

        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            player = new Player(GraphicsDevice.Viewport.Width/2 - 25, GraphicsDevice.Viewport.Height/2 - 25, 50, 50);
            collectibles = new List<Collectible>();
            state = GameState.Menu;
            rand = new Random();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //load the font
            ariel14 = Content.Load<SpriteFont>("Arial14");

            //load textures
            Stream textureStream = TitleContainer.OpenStream("Content/250px-607Litwick.png");
            playerTexture = Texture2D.FromStream(GraphicsDevice, textureStream);
            textureStream.Close();
            textureStream = TitleContainer.OpenStream("Content/FireBall-psd32690.png");
            collectibleTexture = Texture2D.FromStream(GraphicsDevice, textureStream);
            textureStream.Close();

            //set the player's texture
            player.Texture = playerTexture;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            //store old state
            previousKbState = kbState;
            kbState = Keyboard.GetState();
            switch (state)
            {
                case GameState.Menu:
                    if (SingleKeyPress(Keys.Enter))
                    {
                        state = GameState.Game;
                        ResetGame();
                    }
                    break;
                case GameState.Game:
                    //decrement timer
                    timer -= gameTime.ElapsedGameTime.TotalSeconds;
                    //process movement
                    if (kbState.IsKeyDown(Keys.Up))
                        player.Y -= 5;
                    if (kbState.IsKeyDown(Keys.Down))
                        player.Y += 5;
                    if (kbState.IsKeyDown(Keys.Left))
                        player.X -= 5;
                    if (kbState.IsKeyDown(Keys.Right))
                        player.X += 5;
                    //wrap screen
                    ScreenWrap(player);
                    //detect collisions
                    foreach(Collectible thisCollectible in collectibles)
                    {
                        if(thisCollectible.CheckColision(player))
                        {
                            thisCollectible.Active = false;
                            player.LevelScore++;
                            player.TotalScore++;
                        }
                    }
                    //detect time up
                    if (timer < 0)
                        state = GameState.GameOver;
                    if (player.LevelScore == collectibles.Count)
                    {
                        NextLevel();
                    }
                    break;
                case GameState.GameOver:
                    if(SingleKeyPress(Keys.Enter))
                    {
                        state = GameState.Menu;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.RosyBrown);

            spriteBatch.Begin();
            // TODO: Add your drawing code here
            switch (state)
            {
                case GameState.Menu:
                    spriteBatch.DrawString(ariel14, "Litwick's Fire Adventures", new Vector2(50, 50), Color.White);
                    break;
                case GameState.Game:
                    //draw the player
                    player.Draw(spriteBatch);
                    //draw collectiobles
                    foreach(Collectible thisCollectible in collectibles)
                    {
                        thisCollectible.Draw(spriteBatch);
                    }
                    //draw info
                    spriteBatch.DrawString(ariel14, "Level      : " + level, new Vector2(50, 100), Color.White);
                    spriteBatch.DrawString(ariel14, "Level score: " + player.LevelScore, new Vector2(50, 150), Color.White);
                    spriteBatch.DrawString(ariel14, "Time left  : " + String.Format("{0:0.00}", timer), new Vector2(50, 200), Color.White);
                    break;
                case GameState.GameOver:
                    spriteBatch.DrawString(ariel14, "Game Over", new Vector2(50, 50), Color.White);
                    spriteBatch.DrawString(ariel14, "Total score: " + player.TotalScore, new Vector2(50, 150), Color.White);
                    break;
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Helper Methods

        /// <summary>
        /// Creates a new level and changes the level
        /// </summary>
        public void NextLevel()
        {
            int x, y;
            level++;    
            timer = 10;
            player.LevelScore = 0;
            player.X = GraphicsDevice.Viewport.Width / 2 - player.Width / 2;
            player.Y = GraphicsDevice.Viewport.Height / 2 - player.Height / 2;
            collectibles.Clear();
            int numCollectibles = 3 * level + 5;
            //generate collectibles
            for(int i = 0; i < numCollectibles; i++)
            {
                x = rand.Next(0, GraphicsDevice.Viewport.Width - 20);
                y = rand.Next(0, GraphicsDevice.Viewport.Height - 20);
                Collectible newCollectible = new Collectible(x, y, 20, 20);
                newCollectible.Texture = collectibleTexture;
                collectibles.Add(newCollectible);
            }
        }

        /// <summary>
        /// Resets the game
        /// </summary>
        public void ResetGame()
        {
            level = 0;
            player.TotalScore = 0;
            NextLevel();
        }

        /// <summary>
        /// Keeps the game object on the screen
        /// </summary>
        /// <param name="gObject">the game object to check</param>
        void ScreenWrap(GameObject gObject)
        {
            //check for going off the left and right of the screen
            if (gObject.X > GraphicsDevice.Viewport.Width - gObject.Width / 2)
                gObject.X = -1 * gObject.Width / 2;
            else if (gObject.X < -1 * gObject.Width / 2)
                gObject.X = GraphicsDevice.Viewport.Width - gObject.Width / 2;
            //check for going off of the top or bottom of the screen
            if (gObject.Y > GraphicsDevice.Viewport.Height - gObject.Height / 2)
                gObject.Y = -1 * gObject.Height / 2;
            else if (gObject.Y < -1 * gObject.Height / 2)
                gObject.Y = GraphicsDevice.Viewport.Height - gObject.Height / 2;
        }

        /// <summary>
        /// Checks if this is the first frame the given key is pressed
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if this is the first frame the key has been pressed</returns>
        public bool SingleKeyPress(Keys key)
        {
            if (kbState.IsKeyDown(Keys.Enter) && !previousKbState.IsKeyDown(Keys.Enter))
                return true;
            return false;
        }

        #endregion
    }
}
