using System.Windows.Input;
using System.Windows.Threading;

namespace BanachTarskiAnimation;

public partial class MainWindow
{
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1600)
        };
        _timer.Tick += (_, _) =>
        {
            if (Tree.AutoPlay) Tree.NextScene();
        };
        _timer.Start();

        Loaded += (_, _) => Tree.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Space:
            case Key.Right:
                Tree.NextScene();
                break;
            case Key.Left:
                Tree.PrevScene();
                break;
            case Key.OemPlus:
            case Key.Add:
                Tree.ChangeDepth(+1);
                break;
            case Key.OemMinus:
            case Key.Subtract:
                Tree.ChangeDepth(-1);
                break;
            case Key.R:
                Tree.ToggleAutoplay();
                break;
            case Key.Escape:
                Close();
                break;
        }
    }
}