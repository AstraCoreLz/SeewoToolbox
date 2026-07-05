using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SeewoToolboxLauncher;

/// <summary>
/// Lightweight launcher that checks if .NET 6 Desktop Runtime is installed.
/// If present: launches SeewoToolbox.exe.
/// If absent: opens browser to the .NET 6 download page.
/// </summary>
class Program
{
    private const string DotNetDownloadUrl =
        "https://dotnet.microsoft.com/download/dotnet/6.0/runtime/desktop/x64";

    private const string MainExeName = "SeewoToolbox.exe";

    static int Main(string[] args)
    {
        try
        {
            if (IsDotNet6DesktopRuntimeInstalled())
            {
                LaunchMainApp(args);
                return 0;
            }
            else
            {
                OpenDotNetDownloadPage();
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Launcher] Error: {ex.Message}");
            return 2;
        }
    }

    /// <summary>
    /// Checks if .NET 6 Desktop Runtime is installed by looking for the host FXR directory
    /// or by checking the registry (Windows).
    /// </summary>
    static bool IsDotNet6DesktopRuntimeInstalled()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;

        // Method 1: Check file existence in standard install location
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var hostFxrPath = Path.Combine(programFiles, "dotnet", "host", "fxr");

        if (Directory.Exists(hostFxrPath))
        {
            var dirs = Directory.GetDirectories(hostFxrPath, "6.0.*");
            if (dirs.Length > 0)
                return true;
        }

        // Method 2: Check via registry for installed .NET runtimes
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine
                .OpenSubKey(@"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App");

            if (key != null)
            {
                var valueNames = key.GetValueNames();
                foreach (var name in valueNames)
                {
                    if (name.StartsWith("6.0"))
                        return true;
                }
            }
        }
        catch
        {
            // Registry check failed, fall through
        }

        // Method 3: Try running `dotnet --list-runtimes` as last resort
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);
                return output.Contains("Microsoft.WindowsDesktop.App 6.");
            }
        }
        catch
        {
            // dotnet CLI not found
        }

        return false;
    }

    /// <summary>
    /// Launches the main SeewoToolbox.exe from the same directory.
    /// </summary>
    static void LaunchMainApp(string[] args)
    {
        var appDir = AppContext.BaseDirectory;

        // Look for SeewoToolbox.exe in the parent directory (standard publish layout)
        var exePath = Path.Combine(Directory.GetParent(appDir)?.FullName ?? appDir, MainExeName);

        if (!File.Exists(exePath))
        {
            // Try same directory
            exePath = Path.Combine(appDir, MainExeName);
        }

        if (!File.Exists(exePath))
        {
            Console.Error.WriteLine($"[Launcher] Cannot find {MainExeName}");
            OpenDotNetDownloadPage();
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(exePath),
        });
    }

    /// <summary>
    /// Opens the .NET 6 Desktop Runtime download page in the default browser.
    /// </summary>
    static void OpenDotNetDownloadPage()
    {
        Console.WriteLine("[Launcher] .NET 6 Desktop Runtime not found. Opening download page...");

        // Try to show a message box on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                // Use a simple approach - MessageBox via user32.dll
                NativeMethods.MessageBox(IntPtr.Zero,
                    ".NET 6 Desktop Runtime is required but not installed.\nClick OK to open the download page.",
                    "SeewoToolbox - Runtime Required",
                    0x30); // MB_OK | MB_ICONEXCLAMATION
            }
            catch
            {
                Console.WriteLine("[Launcher] .NET 6 Desktop Runtime not installed. Please download from:");
                Console.WriteLine(DotNetDownloadUrl);
            }
        }

        // Open browser
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = DotNetDownloadUrl,
                UseShellExecute = true,
            });
        }
        catch
        {
            Console.WriteLine($"[Launcher] Please visit: {DotNetDownloadUrl}");
        }
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    }
}
