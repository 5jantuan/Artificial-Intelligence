using Battle.Models;

namespace Battle.Logic;

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
                if (alpha >= beta)
                {
                    prunes++;
                    break;
                }
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
                if (alpha >= beta)
                {
                    prunes++;
                    break;
                }
            }
            return value;
        }
    }
}
