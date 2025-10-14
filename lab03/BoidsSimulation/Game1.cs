using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BoidsSimulation
{
    public class Boid
    {
        public Vector2 Position;
        public Vector2 Velocity;

        public Boid(float x, float y, Random random)
        {
            Position = new Vector2(x, y);
            Velocity = new Vector2(
                (float)(random.NextDouble() * 2 - 1),
                (float)(random.NextDouble() * 2 - 1)
            );
        }
    }

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D boidTexture;
        List<Boid> boids = new();
        Random random = new();

        float radius = 60f;
        float maxSpeed = 3f;
        float wCohesion = 0.005f, wAlignment = 0.05f, wSeparation = 0.1f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 700;
            graphics.ApplyChanges();

            for (int i = 0; i < 80; i++)
                boids.Add(new Boid(random.Next(1000), random.Next(700), random));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            boidTexture = new Texture2D(GraphicsDevice, 1, 1);
            boidTexture.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            foreach (var b in boids)
            {
                Vector2 cohesion = Vector2.Zero;
                Vector2 alignment = Vector2.Zero;
                Vector2 separation = Vector2.Zero;
                int count = 0;

                foreach (var other in boids)
                {
                    if (other == b) continue;
                    float dist = Vector2.Distance(b.Position, other.Position);
                    if (dist < radius)
                    {
                        cohesion += other.Position;
                        alignment += other.Velocity;
                        if (dist < 20)
                            separation -= (other.Position - b.Position);
                        count++;
                    }
                }

                if (count > 0)
                {
                    cohesion = ((cohesion / count) - b.Position) * wCohesion;
                    alignment = ((alignment / count) - b.Velocity) * wAlignment;
                    separation *= wSeparation;
                }

                b.Velocity += cohesion + alignment + separation;

                if (b.Velocity.Length() > maxSpeed)
                    b.Velocity = Vector2.Normalize(b.Velocity) * maxSpeed;

                b.Position += b.Velocity;

                // Обёртка по краям
                var screenWidth = graphics.PreferredBackBufferWidth;
                var screenHeight = graphics.PreferredBackBufferHeight;
                if (b.Position.X < 0) b.Position.X = screenWidth;
                if (b.Position.Y < 0) b.Position.Y = screenHeight;
                if (b.Position.X > screenWidth) b.Position.X = 0;
                if (b.Position.Y > screenHeight) b.Position.Y = 0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            foreach (var b in boids)
            {
                float angle = (float)Math.Atan2(b.Velocity.Y, b.Velocity.X);
                var rect = new Rectangle((int)b.Position.X, (int)b.Position.Y, 6, 3);

                spriteBatch.Draw(boidTexture, rect, null, Color.Cyan, angle,
                    new Vector2(0.5f, 0.5f), SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
