using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Pixel_Roguelike
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Full Screen Grid Settings (13x10 grid on 800x640 display)
        private const int GRID_COLS = 13;
        private const int GRID_ROWS = 10;
        private const int DISPLAY_TILE_SIZE = 64; // 16x16 scaled 4x

        // Textures
        private Texture2D _texForestSheet;
        private Texture2D _texDungeonSheet;

        // Player Position on Grid
        private Point _playerGridPos = new Point(2, 2);
        private KeyboardState _previousKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 640;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            string contentFolder = Content.RootDirectory;

            // Load tilesets from relative Content subfolders
            _texForestSheet = Texture2D.FromFile(GraphicsDevice,
                Path.Combine(contentFolder, "Legacy-Fantasy - High Forest 2.0", "Assets", "Tiles.png"));

            _texDungeonSheet = Texture2D.FromFile(GraphicsDevice,
                Path.Combine(contentFolder, "0x72_16x16DungeonTileset.v5", "0x72_16x16DungeonTileset.v5.png"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Turn-Based Movement (1 tile per key press)
            if ((currentKeyboardState.IsKeyDown(Keys.W) && _previousKeyboardState.IsKeyUp(Keys.W)) ||
                (currentKeyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up)))
            {
                if (_playerGridPos.Y > 0) _playerGridPos.Y--;
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.S) && _previousKeyboardState.IsKeyUp(Keys.S)) ||
                     (currentKeyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down)))
            {
                if (_playerGridPos.Y < GRID_ROWS - 1) _playerGridPos.Y++;
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.A) && _previousKeyboardState.IsKeyUp(Keys.A)) ||
                     (currentKeyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left)))
            {
                if (_playerGridPos.X > 0) _playerGridPos.X--;
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.D) && _previousKeyboardState.IsKeyUp(Keys.D)) ||
                     (currentKeyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right)))
            {
                if (_playerGridPos.X < GRID_COLS - 1) _playerGridPos.X++;
            }

            _previousKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // 1. Draw Seamless Inner Grass Fill (Coordinates 16, 16 eliminate vertical edge gaps)
            Rectangle grassSourceRect = new Rectangle(16, 16, 16, 16);

            for (int r = 0; r < GRID_ROWS; r++)
            {
                for (int c = 0; c < GRID_COLS; c++)
                {
                    Rectangle destRect = new Rectangle(
                        c * DISPLAY_TILE_SIZE,
                        r * DISPLAY_TILE_SIZE,
                        DISPLAY_TILE_SIZE,
                        DISPLAY_TILE_SIZE
                    );

                    _spriteBatch.Draw(_texForestSheet, destRect, grassSourceRect, Color.White);
                }
            }

            // 2. Draw Hero Character
            Rectangle heroSourceRect = new Rectangle(128, 0, 16, 16);
            Rectangle playerDestRect = new Rectangle(
                _playerGridPos.X * DISPLAY_TILE_SIZE,
                _playerGridPos.Y * DISPLAY_TILE_SIZE,
                DISPLAY_TILE_SIZE,
                DISPLAY_TILE_SIZE
            );

            _spriteBatch.Draw(_texDungeonSheet, playerDestRect, heroSourceRect, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}