using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.Reflection;

namespace SeewoToolbox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Set window icon from embedded resource
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SeewoToolbox.Assets.icon.png";
            if (assembly.GetManifestResourceStream(resourceName) is { } stream)
            {
                Icon = new WindowIcon(new Bitmap(stream));
            }
        }
        catch
        {
            // Icon loading failed, use default
        }
    }
}
