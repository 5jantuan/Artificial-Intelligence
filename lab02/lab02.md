# Отчёт по лабораторной работе: Минимакс и Alpha-Beta для игры «Битва существ»

## 1. Цель работы

Научиться реализовывать оптимизированный алгоритм **Minimax с Alpha-Beta отсечением** для анализа игровых деревьев.  
Проанализировать эффективность алгоритмов в терминах:

- количества проверенных узлов,  
- количества отсечений (prunes),  
- времени выполнения.

---

## 2. Постановка задачи

- Игра: упрощённая “битва существ” между игроками **MAX** и **MIN**.  
- Игровой процесс:  

| Тип удара      | Урон       | Шанс промаха |
|----------------|------------|--------------|
| Мощный удар    | 3–8        | 30%          |
| Точный удар    | 2–5        | 0%           |

- Цель MAX: закончить с **наибольшим HP**, MIN — с наименьшим.  
- Дерево ходов:
  - **Глубина:** 6 уровней (по 3 хода на игрока)  
  - **Ширина:** 2 (каждый игрок может выбрать один из двух ходов)  
- Оценка позиции (Evaluator):  

```csharp
if (hpMin <= 0 && hpMax > 0) return 10000;
if (hpMax <= 0 && hpMin > 0) return -10000;
if (hpMax <= 0 && hpMin <= 0) return 0;
return hpMax - hpMin;
```

---

## 3. Структура проекта

```bash
CreatureBattleAI/
│
├── Program.cs           # Главная функция
├── Models/
│   └── Node.cs          # Модель игрового узла
├── Logic/
│   ├── MoveLogic.cs     # Построение дерева и применение ходов
│   ├── Minimax.cs       # Рекурсивный алгоритм Minimax
│   └── AlphaBeta.cs     # Рекурсивный Minimax с отсечениями
└── Utils/
    └── Evaluator.cs     # Функция оценки игровых состояний
```
Пояснение структуры:
- Models/ — структуры данных для игрового дерева (Node).
- Logic/ — алгоритмы и игровая механика.
- Utils/ — чистые функции, не зависящие от дерева.
- Program.cs — основной запуск, измерение времени и вывод результатов.

---

## 4. Основные элементы реализации

### 4.1 Node.cs

Хранит текущее состояние игры: HP игроков, кто ходит, глубина, список потомков, значение листа.

```csharp
namespace CreatureBattleAI.Models;

public class Node
{
    public int HpMax { get; set; }
    public int HpMin { get; set; }
    public string Player { get; set; } = "MAX";
    public int Depth { get; set; }
    public List<Node> Children { get; } = new();
    public int Value { get; set; }
    public bool IsLeaf { get; set; }
    public string Move { get; set; } = "";
}
```

---

### 4.2 MoveLogic.cs

- Применяет ход (power или precise) к текущему состоянию.
- Рекурсивно строит дерево возможных ходов до глубины 6.

```csharp
using CreatureBattleAI.Models;

namespace CreatureBattleAI.Logic;

public static class MoveLogic
{
    private static readonly Random random = new();

    public static (int newHpMax, int newHpMin) ApplyMove(Node node, string move)
    {
        int dmg = 0;
        if (move == "power")
        {
            bool miss = random.NextDouble() < 0.3;
            dmg = miss ? 0 : random.Next(3, 9);
        }
        else if (move == "precise")
        {
            dmg = random.Next(2, 6);
        }

        if (node.Player == "MAX")
            return (node.HpMax, node.HpMin - dmg);
        else
            return (node.HpMax - dmg, node.HpMin);
    }

    public static void BuildTree(Node node)
    {
        if (node.Depth == 0 || node.HpMax <= 0 || node.HpMin <= 0)
        {
            node.IsLeaf = true;
            node.Value = Utils.Evaluator.Evaluate(node.HpMax, node.HpMin);
            return;
        }

        foreach (var move in new[] { "power", "precise" })
        {
            var (newHpMax, newHpMin) = ApplyMove(node, move);
            var child = new Node
            {
                HpMax = newHpMax,
                HpMin = newHpMin,
                Player = node.Player == "MAX" ? "MIN" : "MAX",
                Depth = node.Depth - 1,
                Move = move
            };
            node.Children.Add(child);
            BuildTree(child);
        }
    }
}
```

---

### 4.3 Evaluator.cs

Функция оценки состояния, возвращает числовое значение позиции.

```csharp
namespace CreatureBattleAI.Utils;

public static class Evaluator
{
    public static int Evaluate(int hpMax, int hpMin)
    {
        if (hpMin <= 0 && hpMax > 0) return 10000;
        if (hpMax <= 0 && hpMin > 0) return -10000;
        if (hpMax <= 0 && hpMin <= 0) return 0;
        return hpMax - hpMin;
    }
}
```

---

### 4.4 Minimax.cs

Рекурсивный алгоритм Minimax:

```csharp
using CreatureBattleAI.Models;

namespace CreatureBattleAI.Logic;

public static class Minimax
{
    public static int Run(Node node, ref int visited)
    {
        visited++;
        if (node.IsLeaf) return node.Value;

        if (node.Player == "MAX")
        {
            int best = int.MinValue;
            foreach (var child in node.Children)
                best = Math.Max(best, Run(child, ref visited));
            return best;
        }
        else
        {
            int best = int.MaxValue;
            foreach (var child in node.Children)
                best = Math.Min(best, Run(child, ref visited));
            return best;
        }
    }
}
```

### 4.5 AlphaBeta.cs

Minimax с Alpha-Beta отсечением:

```csharp
using CreatureBattleAI.Models;

namespace CreatureBattleAI.Logic;

public static class AlphaBeta
{
    public static int Run(Node node, int alpha, int beta, ref int visited, ref int prunes)
    {
        visited++;
        if (node.IsLeaf) return node.Value;

        if (node.Player == "MAX")
        {
            int value = int.MinValue;
            foreach (var child in node.Children)
            {
                value = Math.Max(value, Run(child, alpha, beta, ref visited, ref prunes));
                alpha = Math.Max(alpha, value);
                if (alpha >= beta) { prunes++; break; }
            }
            return value;
        }
        else
        {
            int value = int.MaxValue;
            foreach (var child in node.Children)
            {
                value = Math.Min(value, Run(child, alpha, beta, ref visited, ref prunes));
                beta = Math.Min(beta, value);
                if (alpha >= beta) { prunes++; break; }
            }
            return value;
        }
    }
}
```

### 4.6 Program.cs

Запуск и измерение эффективности алгоритмов:

```csharp
using CreatureBattleAI.Models;
using CreatureBattleAI.Logic;
using System.Diagnostics;

namespace CreatureBattleAI;

class Program
{
    const int Depth = 6;
    const int InitHp = 20;

    static void Main()
    {
        var root = new Node { HpMax = InitHp, HpMin = InitHp, Player = "MAX", Depth = Depth };

        MoveLogic.BuildTree(root);

        int visited1 = 0;
        var sw1 = Stopwatch.StartNew();
        int result1 = Minimax.Run(root, ref visited1);
        sw1.Stop();

        int visited2 = 0, prunes = 0;
        var sw2 = Stopwatch.StartNew();
        int result2 = AlphaBeta.Run(root, int.MinValue, int.MaxValue, ref visited2, ref prunes);
        sw2.Stop();

        Console.WriteLine($"Minimax: {result1}, visited={visited1}, time={sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"AlphaBeta: {result2}, visited={visited2}, prunes={prunes}, time={sw2.ElapsedMilliseconds}ms");
    }
}
```

## 5. Результаты экспериментов

| Запуск | Minimax | Узлы (Minimax) | AlphaBeta | Узлы (AlphaBeta) | Отсечения | Время (ms) Minimax / AlphaBeta |
|--------|---------|----------------|-----------|-----------------|-----------|-------------------------------|
| 1      | 0       | 125            | 0         | 76              | 19        | 0,36 / 0,22                   |
| 2      | -4      | 127            | -4        | 85              | 16        | 0,37 / 0,23                   |
| 3      | -2      | 127            | -2        | 67              | 15        | 0,36 / 0,23                   |
