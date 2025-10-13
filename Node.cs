using System.Windows;

namespace BanachTarskiAnimation;

public class Node(string word, int depth, int orderIndex)
{
    public string Word { get; } = word;

    public int Depth { get; } = depth;

    public int OrderIndex { get; } = orderIndex;

    public char LastChar => Word.Length == 0 ? '\0' : Word[^1];

    public List<Node> Children { get; } = [];

    public Point Pos { get; set; }
}