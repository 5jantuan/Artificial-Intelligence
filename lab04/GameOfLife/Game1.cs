using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameOfLife
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        const int CellSize = 15;
        const int GridWidth = 40;
        const int GridHeight = 40;

        private int[,] grid = new int[GridHeight, GridWidth];
        private int[,] nextGrid = new int[GridHeight, GridWidth];
        private Random random = new Random();

        private Texture2D aliveTexture;
        private Texture2D deadTexture;

        private double timer = 0;
        private double updateInterval = 0.2; // секунды между поколениями

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GridWidth * CellSize;
            _graphics.PreferredBackBufferHeight = GridHeight * CellSize;
        }

        protected override void Initialize()
        {
            // Случайная генерация сетки
            for (int y = 0; y < GridHeight; y++)
                for (int x = 0; x < GridWidth; x++)
                    grid[y, x] = random.NextDouble() < 0.35 ? 1 : 0;

            // Добавляем Toad в центр
            AddToad(GridWidth / 2 - 2, GridHeight / 2 - 1);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            aliveTexture = new Texture2D(GraphicsDevice, 1, 1);
            aliveTexture.SetData(new[] { Color.YellowGreen });

            deadTexture = new Texture2D(GraphicsDevice, 1, 1);
            deadTexture.SetData(new[] { Color.Black });
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (timer >= updateInterval)
            {
                NextGeneration();
                timer = 0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    var tex = grid[y, x] == 1 ? aliveTexture : deadTexture;
                    _spriteBatch.Draw(tex, new Rectangle(x * CellSize, y * CellSize, CellSize - 1, CellSize - 1), Color.White);
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void NextGeneration()
        {
            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    int neighbors = CountNeighbors(x, y);
                    if (grid[y, x] == 1 && (neighbors == 2 || neighbors == 3))
                        nextGrid[y, x] = 1;
                    else if (grid[y, x] == 0 && neighbors == 3)
                        nextGrid[y, x] = 1;
                    else
                        nextGrid[y, x] = 0;
                }
            }

            // Обновляем текущую сетку
            Array.Copy(nextGrid, grid, grid.Length);
        }

        private int CountNeighbors(int x, int y)
        {
            int count = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < GridWidth && ny >= 0 && ny < GridHeight)
                        count += grid[ny, nx];
                }
            }
            return count;
        }

        private void AddToad(int startX, int startY)
        {
            // форма осциллятора Toad
            grid[startY, startX + 1] = 1;
            grid[startY, startX + 2] = 1;
            grid[startY, startX + 3] = 1;

            grid[startY + 1, startX] = 1;
            grid[startY + 1, startX + 1] = 1;
            grid[startY + 1, startX + 2] = 1;
        }
    }
}
