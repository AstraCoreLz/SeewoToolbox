using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SeewoToolbox.Services;

public static class UpdateService
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10),
    };




    public static async Task<(bool hasUpdate, string latestVersion)> CheckUpdateAsync(
        string versionUrl,
        string currentVersion)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(versionUrl);
            var latestVersion = response.Trim();

            var comparison = CompareVersions(currentVersion, latestVersion);
            if (comparison < 0)
            {
                return (true, latestVersion);
            }

            return (false, latestVersion);
        }
        catch (Exception)
        {
            return (false, currentVersion);
        }
    }




    private static int CompareVersions(string v1, string v2)
    {
        var parts1 = v1.Split('.');
        var parts2 = v2.Split('.');

        var maxLen = Math.Max(parts1.Length, parts2.Length);
        for (int i = 0; i < maxLen; i++)
        {
            var num1 = i < parts1.Length && int.TryParse(parts1[i], out var n1) ? n1 : 0;
            var num2 = i < parts2.Length && int.TryParse(parts2[i], out var n2) ? n2 : 0;

            if (num1 != num2)
                return num1.CompareTo(num2);
        }

        return 0;
    }
}

