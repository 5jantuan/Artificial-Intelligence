# Моделирование движения стаи птиц (Boids Simulation)

## Описание проекта

Данный проект реализует **моделирование движения стаи птиц в 2D-пространстве** с помощью алгоритма **Boids**, предложенного Крэйгом Рейнольдсом в 1986 году.  
Каждая «птица» (агент) движется по простым правилам, но в совокупности они создают реалистичное поведение стаи.

Симуляция выполнена с использованием **MonoGame** — фреймворка для разработки игр на C#.

---

## Цель работы

Научиться моделировать **координированное движение агентов** с применением правил Рейнольдса:

1. **Центрирование (Cohesion)** — движение к центру массы ближайших соседей.  
2. **Выравнивание (Alignment)** — согласование направления и скорости с соседями.  
3. **Разделение (Separation)** — избегание столкновений с соседями.

---

## Алгоритм Boids (Reynolds, 1986)

Для каждого агента (boid):

1. Находим соседей в пределах заданного радиуса (`radius`).
2. Рассчитываем три вектора:
   - **Cohesion** — тянет агента к центру массы соседей.
   - **Alignment** — заставляет выравнивать направление движения со средним направлением соседей.
   - **Separation** — отталкивает от агентов, которые находятся слишком близко.
3. Эти векторы суммируются с определёнными весами (`wCohesion`, `wAlignment`, `wSeparation`).
4. Общая скорость нормализуется, если превышает максимальную (`maxSpeed`).
5. Позиция агента обновляется с учётом новой скорости.

---

## Реализация

Проект состоит из одного файла `Game1.cs`, в котором определены:

- Класс **Boid** — описывает агента: его позицию (`Position`) и скорость (`Velocity`).
- Класс **Game1** — отвечает за инициализацию, обновление логики и отрисовку сцены.

---

## Основные параметры симуляции

| Параметр | Описание | Значение |
|-----------|-----------|----------|
| `radius` | Радиус взаимодействия между агентами | 60 |
| `maxSpeed` | Максимальная скорость агента | 3 |
| `wCohesion` | Вес влияния центрации | 0.005 |
| `wAlignment` | Вес выравнивания | 0.05 |
| `wSeparation` | Вес разделения | 0.1 |

---

## Ключевые моменты реализации

- **Обновление положения:**  
  Каждая птица движется, изменяя свою скорость под влиянием трёх правил.
  
- **Периодическая обёртка:**  
  Когда агент выходит за пределы экрана, он появляется с противоположной стороны — это создаёт «бесконечное» пространство.

- **Отрисовка:**  
  Используется `SpriteBatch` и простая текстура 1×1 пиксель. Каждый агент рисуется как маленький треугольник/прямоугольник, повёрнутый по направлению движения.


---

## Пример поведения агентов

- При небольшом количестве агентов — можно наблюдать хаотичное движение.
- При большом — формируются плотные стаи, волнообразные движения и «вихри».
- Настройка весов (`wCohesion`, `wAlignment`, `wSeparation`) позволяет регулировать характер поведения:
  - Увеличьте `wCohesion` — птицы будут сбиваться в стаю.
  - Увеличьте `wSeparation` — стая станет более «растянутой».
  - Увеличьте `wAlignment` — движение станет более скоординированным.

---

## Исходный код проекта (`Game1.cs`)

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BoidsSimulation
{
    // Класс, описывающий отдельного агента (boid)
    public class Boid
    {
        public Vector2 Position; // Позиция на экране
        public Vector2 Velocity; // Скорость и направление движения

        public Boid(float x, float y, Random random)
        {
            Position = new Vector2(x, y);
            Velocity = new Vector2(
                (float)(random.NextDouble() * 2 - 1),
                (float)(random.NextDouble() * 2 - 1)
            );
        }
    }

    // Основной игровой класс
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D boidTexture;
        List<Boid> boids = new(); // Список всех агентов
        Random random = new();

        float radius = 60f;           // Радиус взаимодействия
        float maxSpeed = 3f;          // Максимальная скорость
        float wCohesion = 0.005f;     // Вес центрирования
        float wAlignment = 0.05f;     // Вес выравнивания
        float wSeparation = 0.1f;     // Вес разделения

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

            // Создаем 80 случайных агентов
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

                // Проходим по всем остальным агентам
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

                // Ограничение максимальной скорости
                if (b.Velocity.Length() > maxSpeed)
                    b.Velocity = Vector2.Normalize(b.Velocity) * maxSpeed;

                b.Position += b.Velocity;

                // Обёртка по краям экрана
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

            // Рисуем каждого агента как маленький прямоугольник
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
```