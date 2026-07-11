using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.Input;
using System.Reflection;

namespace SeewoToolbox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SeewoToolbox.Assets.icon.png";
            if (assembly.GetManifestResourceStream(resourceName) is { } stream)
            {
                Icon = new WindowIcon(new Bitmap(stream));
            }
        }
        catch { }

        SystemDecorations = SystemDecorations.None;
        TryEnableMica();
        SetupCustomTitleBar();
    }

    private void TryEnableMica()
    {
        try
        {
            TransparencyLevelHint = new[] { WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur };
        }
        catch { }
    }

    private void SetupCustomTitleBar()
    {
        var minBtn = this.FindControl<Button>("BtnMinimize");
        if (minBtn != null)
            minBtn.Click += (_, _) => WindowState = WindowState.Minimized;

        var closeBtn = this.FindControl<Button>("BtnClose");
        if (closeBtn != null)
            closeBtn.Click += (_, _) => Close();

        var dragArea = this.FindControl<Border>("TitleBarArea");
        if (dragArea != null)
        {
            dragArea.PointerPressed += (_, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    BeginMoveDrag(e);
            };

            dragArea.DoubleTapped += (_, _) =>
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            };
        }
    }
}
