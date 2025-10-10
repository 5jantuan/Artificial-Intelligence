using Battle.Models;

namespace Battle.Logic;

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
