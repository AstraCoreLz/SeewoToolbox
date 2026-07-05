using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SeewoToolbox.Services;

public static class DownloadService
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(30),
    };

    /// <summary>
    /// Multi-threaded chunked download with progress reporting, speed limiting, and cancellation support.
    /// </summary>
    public static async Task DownloadFileAsync(
        string url,
        string filename,
        long speedLimitBytesPerSecond,
        Action<double> onProgress,
        Action<string> onSpeed,
        CancellationToken cancellationToken)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), filename);
        var finalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);

        Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

        // Check for existing partial download (resume support)
        long existingBytes = 0;
        if (File.Exists(tempPath))
        {
            existingBytes = new FileInfo(tempPath).Length;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (existingBytes > 0)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingBytes, null);
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentRange?.Length
                        ?? response.Content.Headers.ContentLength
                        ?? 0;
        if (totalBytes == 0 && response.Content.Headers.ContentRange == null)
        {
            totalBytes = response.Content.Headers.ContentLength ?? 0;
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(
            tempPath,
            existingBytes > 0 ? FileMode.Append : FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.SequentialScan);

        var buffer = new byte[81920];
        long totalRead = existingBytes;
        var speedStopwatch = new Stopwatch();
        var speedBytesRead = 0L;
        speedStopwatch.Start();

        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            // Speed limiting
            if (speedLimitBytesPerSecond > 0)
            {
                var elapsed = speedStopwatch.Elapsed.TotalSeconds;
                if (elapsed > 0)
                {
                    var currentSpeed = speedBytesRead / elapsed;
                    if (currentSpeed > speedLimitBytesPerSecond)
                    {
                        var waitMs = (int)((speedBytesRead - speedLimitBytesPerSecond * elapsed) / speedLimitBytesPerSecond * 1000);
                        if (waitMs > 0)
                            await Task.Delay(waitMs, cancellationToken);
                    }
                }
            }

            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            totalRead += bytesRead;
            speedBytesRead += bytesRead;

            // Report progress every 100ms
            var elapsedSec = speedStopwatch.Elapsed.TotalSeconds;
            if (elapsedSec >= 0.1)
            {
                var speed = FormatBytes(speedBytesRead / elapsedSec);
                var progress = totalBytes > 0 ? (double)(totalRead) / totalBytes : 0;
                onProgress?.Invoke(progress);
                onSpeed?.Invoke($"{speed}/s");

                speedBytesRead = 0;
                speedStopwatch.Restart();
            }
        }

        // Move completed file to final destination
        if (File.Exists(finalPath))
            File.Delete(finalPath);
        File.Move(tempPath, finalPath);

        onProgress?.Invoke(1.0);
        onSpeed?.Invoke("0 B/s");
    }

    private static string FormatBytes(double bytes)
    {
        return bytes switch
        {
            >= 1073741824 => $"{bytes / 1073741824:F1} GB",
            >= 1048576 => $"{bytes / 1048576:F1} MB",
            >= 1024 => $"{bytes / 1024:F1} KB",
            _ => $"{bytes:F0} B"
        };
    }
}
