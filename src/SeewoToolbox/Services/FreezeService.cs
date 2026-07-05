using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeewoToolbox.Services;

public static class FreezeService
{
    // Encrypted configuration - do not modify
    private static readonly string _cfg = "aHR0cDovLzEyNy4wLjAuMTo2MDgy";
    private static readonly string _fp = "L2FwaS92MS9leGN1dGVfcHJvdGVjdA==";
    private static readonly string _tp = "L2FwaS92MS9zZXQ/dm9sPTA=";

    private static string _base => Decode(_cfg);
    private static string _freezePath => Decode(_fp);
    private static string _thawPath => Decode(_tp);

    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15),
    };

    private static string Decode(string encoded)
    {
        try { return Encoding.UTF8.GetString(Convert.FromBase64String(encoded)); }
        catch { return string.Empty; }
    }

    /// <summary>
    /// Freeze specific drives.
    /// </summary>
    public static async Task FreezeDisksAsync(string[] driveLetters)
    {
        var drivesLower = driveLetters.Select(d => d.TrimEnd(':').ToLower()).ToArray();

        var content = new StringContent(
            JsonSerializer.Serialize(new { selectedDisks = drivesLower }),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_base}{_freezePath}", content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"冻结请求返回状态码 {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"冻结失败，无法调用保护服务。请确认服务正在运行。\n\n错误详情：{ex.Message}", ex);
        }
    }

    /// <summary>
    /// Freeze all drives (auto-detect all existing drives).
    /// </summary>
    public static async Task FreezeAllAsync()
    {
        var disks = new System.Collections.Generic.List<string>();
        foreach (var letter in "CDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            if (System.IO.Directory.Exists($"{letter}:"))
                disks.Add(letter.ToString());
        }

        if (disks.Count == 0)
            throw new Exception("未检测到任何磁盘驱动器");

        await FreezeDisksAsync(disks.ToArray());
    }

    /// <summary>
    /// Thaw all drives.
    /// </summary>
    public static async Task ThawAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_base}{_thawPath}");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"解冻请求返回状态码 {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"解冻失败，无法调用保护服务。请确认服务正在运行。\n\n错误详情：{ex.Message}", ex);
        }
    }
}
