namespace Battle.Models;

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
