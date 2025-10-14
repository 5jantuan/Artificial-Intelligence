using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BoidsSimulation
{
    // Класс, описывающий одну "птицу" (boid)
    public class Boid
    {
        public Vector2 Position;  // Позиция на экране
        public Vector2 Velocity;  // Скорость (направление + скорость движения)

        public Boid(float x, float y, Random random)
        {
            // Задаём начальную позицию
            Position = new Vector2(x, y);

            // Случайное направление скорости
            Velocity = new Vector2(
                (float)(random.NextDouble() * 2 - 1), // от -1 до 1
                (float)(random.NextDouble() * 2 - 1)
            );
        }
    }

    // Основной игровой класс MonoGame
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;  // Управление графикой
        SpriteBatch spriteBatch;         // Используется для рисования спрайтов
        Texture2D boidTexture;           // Текстура точки (boid)
        List<Boid> boids = new();        // Список всех boids
        Random random = new();           // Для случайных значений

        // Параметры поведения
        float radius = 60f;              // Радиус "зрения" — дальность, на которой boid видит соседей
        float maxSpeed = 3f;             // Максимальная скорость
        float wCohesion = 0.005f;        // Вес притяжения (сближения)
        float wAlignment = 0.05f;        // Вес выравнивания направления
        float wSeparation = 0.1f;        // Вес отталкивания

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true; // Чтобы видеть курсор мыши
        }

        protected override void Initialize()
        {
            // Настраиваем размер окна
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 700;
            graphics.ApplyChanges();

            // Создаём 80 boids со случайными координатами
            for (int i = 0; i < 80; i++)
                boids.Add(new Boid(random.Next(1000), random.Next(700), random));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Инициализация спрайтбатча
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Создаём белую текстуру 1×1 пиксель
            // (будем растягивать её при рисовании)
            boidTexture = new Texture2D(GraphicsDevice, 1, 1);
            boidTexture.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            // Закрыть игру по ESC
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // --- Главный цикл симуляции ---
            foreach (var b in boids)
            {
                // Силы поведения
                Vector2 cohesion = Vector2.Zero;   // Притяжение к центру группы
                Vector2 alignment = Vector2.Zero;  // Выравнивание направления
                Vector2 separation = Vector2.Zero; // Отталкивание от соседей
                int count = 0;                     // Количество видимых соседей

                // Проверяем всех остальных boids
                foreach (var other in boids)
                {
                    if (other == b) continue; // Не сравниваем самого себя

                    // Расстояние между двумя boids
                    float dist = Vector2.Distance(b.Position, other.Position);

                    // Если другой boid в пределах радиуса зрения
                    if (dist < radius)
                    {
                        // Для притяжения: запоминаем позицию соседа
                        cohesion += other.Position;

                        // Для выравнивания: суммируем скорость соседей
                        alignment += other.Velocity;

                        // Для разделения: если слишком близко — отталкиваемся
                        if (dist < 20)
                            separation -= (other.Position - b.Position);

                        count++;
                    }
                }

                // Если есть соседи — рассчитываем средние направления
                if (count > 0)
                {
                    // Средняя позиция минус текущая позиция — вектор к центру группы
                    cohesion = ((cohesion / count) - b.Position) * wCohesion;

                    // Средняя скорость соседей минус моя скорость — вектор для выравнивания
                    alignment = ((alignment / count) - b.Velocity) * wAlignment;

                    // Усиливаем или ослабляем отталкивание
                    separation *= wSeparation;
                }

                // Суммируем все три вектора поведения
                b.Velocity += cohesion + alignment + separation;

                // Ограничиваем максимальную скорость
                if (b.Velocity.Length() > maxSpeed)
                    b.Velocity = Vector2.Normalize(b.Velocity) * maxSpeed;

                // Обновляем позицию
                b.Position += b.Velocity;

                // --- Проверка границ экрана ---
                var screenWidth = graphics.PreferredBackBufferWidth;
                var screenHeight = graphics.PreferredBackBufferHeight;

                // Если boid выходит за экран — переносим его на противоположную сторону
                if (b.Position.X < 0) b.Position.X = screenWidth;
                if (b.Position.Y < 0) b.Position.Y = screenHeight;
                if (b.Position.X > screenWidth) b.Position.X = 0;
                if (b.Position.Y > screenHeight) b.Position.Y = 0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Очищаем экран чёрным цветом
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Отрисовка каждого boid
            foreach (var b in boids)
            {
                // Вычисляем угол поворота спрайта по направлению движения
                float angle = (float)Math.Atan2(b.Velocity.Y, b.Velocity.X);

                // Создаём прямоугольник для рисования
                var rect = new Rectangle((int)b.Position.X, (int)b.Position.Y, 6, 3);

                // Рисуем белую текстуру как "птичку" (маленький прямоугольник)
                spriteBatch.Draw(
                    boidTexture,
                    rect,
                    null,
                    Color.Cyan,          // Цвет boid
                    angle,               // Поворот в сторону движения
                    new Vector2(0.5f, 0.5f), // Точка вращения — центр
                    SpriteEffects.None,
                    0f
                );
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
