using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pixel_Roguelike
{
    public enum ItemType
    {
        Coin,
        Potion,
        Chest
    }

    public enum ChestState
    {
        Closed,
        Open
    }

    public class Item
    {
        public Point GridPos;
        public ItemType Type;
        public ChestState State = ChestState.Closed;
        public bool WasOpened = false;

        public Item(int x, int y, ItemType type)
        {
            GridPos = new Point(x, y);
            Type = type;
        }
    }

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
        private Texture2D _texIconsSheet;
        private Texture2D _texChestClosed;
        private Texture2D _texChestOpen;

        // Player Position & Input
        private Point _playerGridPos = new Point(2, 2);
        private KeyboardState _previousKeyboardState;

        // Items & Inventory
        private List<Item> _items = new List<Item>();
        private int _goldCollected = 0;
        private int _potionsCollected = 0;

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
            // Spawn items on the grass grid
            _items.Add(new Item(4, 3, ItemType.Coin));
            _items.Add(new Item(8, 5, ItemType.Chest)); // Golden Chest item
            _items.Add(new Item(6, 7, ItemType.Potion));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            string contentFolder = Content.RootDirectory;

            // Load grass tileset from High Forest 2.0
            _texForestSheet = Texture2D.FromFile(GraphicsDevice,
                Path.Combine(contentFolder, "Legacy-Fantasy - High Forest 2.0", "Assets", "Tiles.png"));

            // Load hero character idle sheet from High Forest 2.0
            _texDungeonSheet = Texture2D.FromFile(GraphicsDevice,
                Path.Combine(contentFolder, "Legacy-Fantasy - High Forest 2.0", "Character", "Idle", "Idle-Sheet.png"));

            // Load 0x72 dungeon sheet for coin/potion icons
            _texIconsSheet = Texture2D.FromFile(GraphicsDevice,
                Path.Combine(contentFolder, "0x72_16x16DungeonTileset.v5", "0x72_16x16DungeonTileset.v5.png"));

            // Load golden chest textures (closed and open variants)
            _texChestClosed = Texture2D.FromFile(GraphicsDevice,
                Path.Combine(contentFolder, "0x72_16x16DungeonTileset.v5", "items", "chest_golden_closed.png"));

            _texChestOpen = Texture2D.FromFile(GraphicsDevice,
                Path.Combine(contentFolder, "0x72_16x16DungeonTileset.v5", "items", "chest_golden_open_full.png"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState currentKeyboardState = Keyboard.GetState();
            bool playerMoved = false;

            // Turn-Based Movement (1 tile per key press)
            if ((currentKeyboardState.IsKeyDown(Keys.W) && _previousKeyboardState.IsKeyUp(Keys.W)) ||
                (currentKeyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up)))
            {
                if (_playerGridPos.Y > 0) { _playerGridPos.Y--; playerMoved = true; }
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.S) && _previousKeyboardState.IsKeyUp(Keys.S)) ||
                     (currentKeyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down)))
            {
                if (_playerGridPos.Y < GRID_ROWS - 1) { _playerGridPos.Y++; playerMoved = true; }
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.A) && _previousKeyboardState.IsKeyUp(Keys.A)) ||
                     (currentKeyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left)))
            {
                if (_playerGridPos.X > 0) { _playerGridPos.X--; playerMoved = true; }
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.D) && _previousKeyboardState.IsKeyUp(Keys.D)) ||
                     (currentKeyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right)))
            {
                if (_playerGridPos.X < GRID_COLS - 1) { _playerGridPos.X++; playerMoved = true; }
            }

            if (playerMoved)
            {
                UpdateChestStates(_playerGridPos);
                CheckItemPickup(_playerGridPos);
            }

            _previousKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        private void UpdateChestStates(Point playerPos)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].Type == ItemType.Chest)
                {
                    // Calculate grid distance (Manhattan distance)
                    int distance = Math.Abs(_items[i].GridPos.X - playerPos.X) + Math.Abs(_items[i].GridPos.Y - playerPos.Y);

                    // If hero moves adjacent to chest (1 tile away), open it
                    if (distance == 1)
                    {
                        _items[i].State = ChestState.Open;
                        _items[i].WasOpened = true;
                    }
                    // If chest was already opened and hero steps away (distance > 1), remove it
                    else if (_items[i].WasOpened && distance > 1)
                    {
                        _goldCollected += 100; // Collect chest loot upon vanishing
                        _items.RemoveAt(i);
                    }
                }
            }
        }

        private void CheckItemPickup(Point playerPos)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].GridPos == playerPos)
                {
                    if (_items[i].Type == ItemType.Coin)
                    {
                        _goldCollected += 10;
                        _items.RemoveAt(i);
                    }
                    else if (_items[i].Type == ItemType.Potion)
                    {
                        _potionsCollected += 1;
                        _items.RemoveAt(i);
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // 1. Draw Seamless Inner Grass Fill
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

            // 2. Draw Items on Grid
            Rectangle coinSourceRect = new Rectangle(288, 272, 16, 16);
            Rectangle potionSourceRect = new Rectangle(288, 224, 16, 16);

            foreach (var item in _items)
            {
                Rectangle itemDestRect = new Rectangle(
                    item.GridPos.X * DISPLAY_TILE_SIZE,
                    item.GridPos.Y * DISPLAY_TILE_SIZE,
                    DISPLAY_TILE_SIZE,
                    DISPLAY_TILE_SIZE
                );

                if (item.Type == ItemType.Chest)
                {
                    // Render open or closed chest based on state
                    Texture2D chestTex = (item.State == ChestState.Open) ? _texChestOpen : _texChestClosed;
                    _spriteBatch.Draw(chestTex, itemDestRect, Color.White);
                }
                else
                {
                    Rectangle srcRect = (item.Type == ItemType.Coin) ? coinSourceRect : potionSourceRect;
                    _spriteBatch.Draw(_texIconsSheet, itemDestRect, srcRect, Color.White);
                }
            }

            // 3. Draw Hero Character
            Rectangle heroSourceRect = new Rectangle(0, 0, 48, 48);
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