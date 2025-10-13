using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BanachTarskiAnimation;

public class WordTreeView : Control
{
    private readonly Color _colorA = Color.FromRgb(80, 115, 205);
    private readonly Color _coloraA = Color.FromRgb(50, 90, 170);
    private readonly Color _colorB = Color.FromRgb(230, 150, 60);
    private readonly Color _colorbB = Color.FromRgb(200, 120, 40);
    private readonly Color _edgeColor = Color.FromArgb(70, 0, 0, 0);
    private readonly Color _nodeFill = Color.FromArgb(240, 240, 240, 240);
    private readonly Color _nodeStroke = Color.FromArgb(120, 60, 60, 60);
    private readonly Typeface _type = new("Segoe UI");

    private int _depth = 4;

    private Dictionary<string, Node> _nodes = new();
    private int _scene;

    static WordTreeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WordTreeView),
            new FrameworkPropertyMetadata(typeof(WordTreeView)));
    }

    public WordTreeView()
    {
        SnapsToDevicePixels = true;
        Focusable = true;
        Build();

        SizeChanged += (_, _) => InvalidateVisual();
    }

    private double NodeRadius => 10.0;
    private double LevelHeight => 100.0;

    public bool AutoPlay { get; private set; }

    public void NextScene()
    {
        _scene = (_scene + 1) % 5;
        InvalidateVisual();
    }

    public void PrevScene()
    {
        _scene = (_scene + 4) % 5;
        InvalidateVisual();
    }

    public void ToggleAutoplay()
    {
        AutoPlay = !AutoPlay;
        InvalidateVisual();
    }

    public void ChangeDepth(int d)
    {
        _depth = Math.Max(1, Math.Min(7, _depth + d));
        Build();
        InvalidateVisual();
    }

    private void Build()
    {
        _nodes = BuildReducedWordTree(_depth);
        LayoutNodes(RenderSize);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        LayoutNodes(RenderSize);

        var legendBand = new Rect(0, 0, RenderSize.Width, 140);
        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(8, 0, 0, 0)), null, legendBand);

        foreach (var n in _nodes.Values)
        foreach (var c in n.Children)
            dc.DrawLine(new Pen(new SolidColorBrush(_edgeColor), 1.2), n.Pos, c.Pos);

        foreach (var n in _nodes.Values.OrderBy(v => v.Depth)) DrawNode(dc, n);

        var sceneTitle = _scene switch
        {
            0 => "Szene 1: Reduzierter Wörterbaum von F₂",
            1 => "Szene 2: Partition F₂ = A ∪ B (nach erstem Buchstaben)",
            2 => "Szene 3: aA",
            3 => "Szene 4: bB",
            4 => "Szene 5: aA ∪ bB = F₂ (Ausschnitt)",
            _ => string.Empty
        };
        var status = $"Tiefe: {_depth}   Szene: {_scene + 1}/5   Auto: {(AutoPlay ? "an" : "aus")}   —   {sceneTitle}";
        DrawBottomRight(dc, status, 12, Brushes.DimGray);
    }

    private void DrawNode(DrawingContext dc, Node n)
    {
        var (fill, stroke) = ColorsFor(n);
        var fillBrush = new SolidColorBrush(fill);
        var strokePen = new Pen(new SolidColorBrush(stroke), 1.6);

        dc.DrawEllipse(fillBrush, strokePen, n.Pos, NodeRadius, NodeRadius);

        if (n.Depth > 4) return;

        var label = string.IsNullOrEmpty(n.Word) ? "ε" : n.Word;
        var ft = new FormattedText(label, CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight, _type, 11, Brushes.Black, 1.25);
        dc.DrawText(ft, new Point(n.Pos.X - ft.Width / 2, n.Pos.Y - NodeRadius - ft.Height - 2));
    }

    private (Color fill, Color stroke) ColorsFor(Node n)
    {
        var isA = InA(n.Word);
        var isB = InB(n.Word);
        var isaA = In_aA(n.Word);
        var isbB = In_bB(n.Word);

        var fill = _nodeFill;
        var stroke = _nodeStroke;

        switch (_scene)
        {
            case 0:
                break;
            case 1:
                if (isA)
                {
                    fill = _colorA;
                    stroke = Darken(_colorA, 0.6);
                }
                else if (isB)
                {
                    fill = _colorB;
                    stroke = Darken(_colorB, 0.6);
                }
                break;
            case 2:
                if (isaA)
                {
                    fill = _coloraA;
                    stroke = Darken(_coloraA, 0.6);
                }
                else
                {
                    fill = Color.FromRgb(230, 230, 230);
                }
                break;
            case 3:
                if (isbB)
                {
                    fill = _colorbB;
                    stroke = Darken(_colorbB, 0.6);
                }
                else
                {
                    fill = Color.FromRgb(230, 230, 230);
                }
                break;
            case 4:
                if (isaA && isbB)
                {
                    fill = Mix(_coloraA, _colorbB, 0.5);
                    stroke = Darken(fill, 0.6);
                }
                else if (isaA)
                {
                    fill = _coloraA;
                    stroke = Darken(_coloraA, 0.6);
                }
                else if (isbB)
                {
                    fill = _colorbB;
                    stroke = Darken(_colorbB, 0.6);
                }
                else
                {
                    fill = Color.FromRgb(230, 230, 230);
                }
                break;
        }

        return (fill, stroke);
    }

    private void LayoutNodes(Size size)
    {
        if (_nodes.Count == 0) return;

        var w = Math.Max(200, size.Width);
        double top = 150;

        var groups = _nodes.Values.GroupBy(n => n.Depth).OrderBy(g => g.Key);
        foreach (var grp in groups)
        {
            var count = grp.Count();
            var y = top + grp.Key * LevelHeight;
            var i = 0;
            foreach (var node in grp.OrderBy(n => n.OrderIndex))
            {
                var x = (i + 1) * (w / (count + 1));
                node.Pos = new Point(x, y);
                i++;
            }
        }
    }

    private static Dictionary<string, Node> BuildReducedWordTree(int depth)
    {
        var nodes = new Dictionary<string, Node>();
        var root = new Node(string.Empty, 0, 0);
        nodes[root.Word] = root;

        var q = new Queue<Node>();
        q.Enqueue(root);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur.Depth >= depth) continue;
            foreach (var c in new[] { 'a', 'A', 'b', 'B' })
            {
                if (IsInversePair(cur.LastChar, c)) continue;

                var childWord = cur.Word + c;
                var child = new Node(childWord, cur.Depth + 1, NextOrderIndex(cur.OrderIndex, c));
                if (!nodes.TryAdd(childWord, child)) continue;

                cur.Children.Add(child);
                q.Enqueue(child);
            }
        }

        return nodes;
    }

    private static bool IsInversePair(char prev, char next)
    {
        return (prev == 'a' && next == 'A') || (prev == 'A' && next == 'a') ||
               (prev == 'b' && next == 'B') || (prev == 'B' && next == 'b');
    }

    private static int NextOrderIndex(int parentOrder, char edge)
    {
        var edgeRank = edge switch { 'a' => 0, 'A' => 1, 'b' => 2, 'B' => 3, _ => 0 };
        return parentOrder * 4 + edgeRank + 1;
    }

    private static bool InA(string w)
    {
        if (w.Length == 0) return true; // ε ∈ A
        var c = w[0];
        return c == 'a' || c == 'A';
    }

    private static bool InB(string w)
    {
        if (w.Length == 0) return false;

        var c = w[0];
        return c == 'b' || c == 'B';
    }

    private static bool In_aA(string w)
    {
        var pre = Reduce("A" + w); // A = a^{-1}
        return InA(pre);
    }

    private static bool In_bB(string w)
    {
        var pre = Reduce("B" + w); // B = b^{-1}
        return InB(pre);
    }

    private static string Reduce(string w)
    {
        Span<char> buffer = stackalloc char[w.Length];

        var len = 0;
        foreach (var c in w)
            if (len > 0 && IsInversePair(buffer[len - 1], c))
                len--;
            else
                buffer[len++] = c;

        return new string(buffer[..len]);
    }

    private static Color Darken(Color c, double f)
    {
        var r = (byte)(c.R * f);
        var g = (byte)(c.G * f);
        var b = (byte)(c.B * f);

        return Color.FromRgb(r, g, b);
    }

    private static Color Mix(Color a, Color b, double t)
    {
        var r = (byte)(a.R * (1 - t) + b.R * t);
        var g = (byte)(a.G * (1 - t) + b.G * t);
        var bl = (byte)(a.B * (1 - t) + b.B * t);

        return Color.FromRgb(r, g, bl);
    }

    private void DrawBottomRight(DrawingContext dc, string text, double fontSize, Brush brush)
    {
        var ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _type, fontSize, brush, 1.25);
        var p = new Point(Math.Max(0, RenderSize.Width - ft.Width - 10), Math.Max(0, RenderSize.Height - ft.Height - 8));
        dc.DrawText(ft, p);
    }
}