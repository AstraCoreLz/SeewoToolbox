using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SeewoToolbox.Services;

public static class InstallService
{



    public static async Task SilentInstallAsync(string url, string filename)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, filename);

        if (!File.Exists(filePath))
        {
            await DownloadService.DownloadFileAsync(
                url,
                filename,
                0,
                _ => { },
                _ => { },
                CancellationToken.None);
        }

        await SilentInstallFromFileAsync(filePath);
    }



    public static async Task SilentInstallFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"安装文件不存在: {filePath}", filePath);

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        string arguments = extension switch
        {
            ".exe" => "/S /silent /quiet /norestart",
            ".msi" => "/quiet /norestart",
            _ => throw new NotSupportedException($"不支持的安装文件格式: {extension}")
        };

        var processInfo = new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        using var process = Process.Start(processInfo);
        if (process == null)
            throw new Exception("无法启动安装进程");

        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(10));
        var exitTask = process.WaitForExitAsync();

        if (await Task.WhenAny(exitTask, timeoutTask) == timeoutTask)
        {
            try { process.Kill(); } catch {  }
            throw new TimeoutException("安装超时，已强制终止");
        }

        try { File.Delete(filePath); } catch {  }
    }
}

