using Battle.Models;

namespace Battle.Logic;

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
