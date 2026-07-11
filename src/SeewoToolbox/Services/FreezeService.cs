using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeewoToolbox.Services;

public static class FreezeService
{
    private const string BaseUrl = "http://127.0.0.1:6082";

    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15),
    };



    public static async Task FreezeDisksAsync(string[] driveLetters)
    {
        var drivesLower = driveLetters.Select(d => d.TrimEnd(':').ToLower()).ToArray();

        var content = new StringContent(
            JsonSerializer.Serialize(new { selectedDisks = drivesLower }),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/api/v1/excute_protect", content);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"冻结请求返回状态码 {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"冻结失败，无法调用希沃冰点还原。请确认希沃冰点服务正在运行。\n\n错误详情：{ex.Message}", ex);
        }
    }



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



    public static async Task ThawAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/api/v1/set?vol=0");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"解冻请求返回状态码 {(int)response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"解冻失败，无法调用希沃冰点还原。请确认希沃冰点服务正在运行。\n\n错误详情：{ex.Message}", ex);
        }
    }
}

