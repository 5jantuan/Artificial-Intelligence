using Battle.Models;
using Battle.Logic;
using System.Diagnostics;

namespace Battle;

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

        Console.WriteLine($"Minimax: {result1}, visited={visited1}, time={sw1.Elapsed.TotalMilliseconds:F5}ms");
        Console.WriteLine($"AlphaBeta: {result2}, visited={visited2}, prunes={prunes}, time={sw2.Elapsed.TotalMilliseconds:F5}ms");
    }
}
