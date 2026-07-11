using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;
using Avalonia.Media;
using SeewoToolbox.Models;
using Avalonia.Rendering.Composition;
using SeewoToolbox.Services;

namespace SeewoToolbox.ViewModels;

public class MainViewModel : ViewModelBase
{
    private const string CurrentVersion = "3.1.0.102";

    private string _updateStatus = string.Empty;
    private bool _isDarkTheme;
    private bool _isUpdateChecking;
    private bool _selectAllSeewo;
    private bool _showSeewoSetup;
    private bool _showMoreTools;
    private double _downloadProgress;
    private string _downloadSpeed = "0 B/s";
    private int _speedLimit;
    private string _downloadLog = string.Empty;
    private bool _isDownloading;

    public bool IsDownloading
    {
        get => _isDownloading;
        set => Set(ref _isDownloading, value);
    }

    public string UpdateStatus
    {
        get => _updateStatus;
        set => Set(ref _updateStatus, value);
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (Set(ref _isDarkTheme, value))
            {
                var app = Application.Current;
                if (app != null)
                {
                    app.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
                    UpdateThemeResources(value);
                }
                RaisePropertyChanged(nameof(ThemeIcon));
            }
        }
    }

    public string ThemeIcon => IsDarkTheme ? "Light" : "Dark";

    public bool IsUpdateChecking
    {
        get => _isUpdateChecking;
        set => Set(ref _isUpdateChecking, value);
    }

    public bool SelectAllSeewo
    {
        get => _selectAllSeewo;
        set
        {
            if (Set(ref _selectAllSeewo, value))
            {
                foreach (var item in SeewoSoftwareList)
                    item.IsSelected = value;
            }
        }
    }

    public bool ShowSeewoSetup
    {
        get => _showSeewoSetup;
        set => Set(ref _showSeewoSetup, value);
    }

    public bool ShowMoreTools
    {
        get => _showMoreTools;
        set => Set(ref _showMoreTools, value);
    }

    public bool ShowMainPage => !ShowSeewoSetup && !ShowMoreTools;

    public double DownloadProgress
    {
        get => _downloadProgress;
        set => Set(ref _downloadProgress, value);
    }

    public string DownloadSpeed
    {
        get => _downloadSpeed;
        set => Set(ref _downloadSpeed, value);
    }

    public int SpeedLimit
    {
        get => _speedLimit;
        set => Set(ref _speedLimit, value);
    }

    public string DownloadLog
    {
        get => _downloadLog;
        set => Set(ref _downloadLog, value);
    }

    public ObservableCollection<SoftwareItem> SeewoSoftwareList { get; } = new()
    {
        new SoftwareItem { Name = "希沃白板5", Url = "https://e.seewo.com/download/file?code=EasiNote5&version=5.2.1.9634", Filename = "EasiNote5.exe" },
        new SoftwareItem { Name = "希沃视频展台", Url = "https://e.seewo.com/download/file?code=EasiCamera&version=2.0.10.3451", Filename = "EasiCamera.exe" },
        new SoftwareItem { Name = "班级授课助手", Url = "https://e.seewo.com/download/file?code=EasiCare_PC&version=2.1.0.3239", Filename = "EasiCare_PC.exe" },
        new SoftwareItem { Name = "希沃管家", Url = "https://e.seewo.com/download/file?code=SeewoServiceSetup&&version=1.6.3.3929", Filename = "SeewoServiceSetup.exe" },
        new SoftwareItem { Name = "轻录播", Url = "https://imlizhi-store-https.seewo.com/EasirecorderSetup_1.1.0.694(20251103144530).exe", Filename = "EasirecorderSetup.exe" },
        new SoftwareItem { Name = "PPT小助手", Url = "https://lz.qaiu.top/parser?url=https://wwsb.lanzoul.com/iRcc63b9dgkd&pwd=1vrq", Filename = "PPTAssistant.exe" },
        new SoftwareItem { Name = "WPS Office最新版", Url = "https://lz.qaiu.top/parser?url=https://www.ilanzou.com/s/4dENCCSr", Filename = "WPSOffice.exe" },
        new SoftwareItem { Name = "Office 2016四合一带激活", Url = "https://lz.qaiu.top/parser?url=https://www.ilanzou.com/s/ipNn2Mto", Filename = "OfficeSetup.exe" },
        new SoftwareItem { Name = "希沃桌面壁纸", Url = "https://lz.qaiu.top/parser?url=https://wwsb.lanzoul.com/iYQEG3b8bcij&pwd=292z", Filename = "SeewoWallpaper.jpg" },
    };

    public ICommand ToggleThemeCommand { get; }
    public ICommand NavigateToSeewoSetupCommand { get; }
    public ICommand NavigateToMoreToolsCommand { get; }
    public ICommand NavigateBackCommand { get; }
    public ICommand CheckUpdateCommand { get; }
    public ICommand InstallWeChatCommand { get; }
    public ICommand InstallQQCommand { get; }
    public ICommand InstallPCManagerCommand { get; }
    public ICommand FreezeAllDisksCommand { get; }
    public ICommand FreezeCDriveCommand { get; }
    public ICommand ThawDisksCommand { get; }
    public ICommand StartSeewoSetupCommand { get; }
    public ICommand ActivateWindowsCommand { get; }
    public ICommand RebootToRecoveryCommand { get; }
    public ICommand DownloadOriginalWindowsCommand { get; }
    public ICommand OpenAuthorHomepageCommand { get; }
    public ICommand ContactAuthorCommand { get; }
    public ICommand OpenAstraCoreCommand { get; }
    public ICommand AdvancedConfigCommand { get; }
    public ICommand InstallClassIslandCommand { get; }

    public MainViewModel()
    {
        Telemetry.Start();

        ToggleThemeCommand = new RelayCommand(_ => { IsDarkTheme = !IsDarkTheme; Telemetry.Ping("toggle_theme"); });

        NavigateToSeewoSetupCommand = new RelayCommand(async _ =>
        {
            await NavigateWithAnimation(true, false);
        });

        NavigateToMoreToolsCommand = new RelayCommand(async _ =>
        {
            await NavigateWithAnimation(false, true);
        });

        NavigateBackCommand = new RelayCommand(async _ =>
        {
            await NavigateWithAnimation(false, false);
        });

        CheckUpdateCommand = new RelayCommand(async _ => { Telemetry.Ping("check_update"); await CheckForUpdateAsync(); });
        InstallWeChatCommand = new RelayCommand(async _ => { Telemetry.Ping("install_wechat"); await InstallWeChatAsync(); });
        InstallQQCommand = new RelayCommand(async _ => { Telemetry.Ping("install_qq"); await ShowInstallDialogAsync("QQ", "https://dldir1v6.qq.com/qqfile/qq/PCQQ9.7.25/QQ9.7.25.29415.exe", "QQ9.7.25.29415.exe"); });
        InstallPCManagerCommand = new RelayCommand(async _ => { Telemetry.Ping("install_pcmanager"); await ShowInstallDialogAsync("腾讯电脑管家", "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload_home_1981.exe", "QQPCDownload_home_1981.exe"); });

        FreezeAllDisksCommand = new RelayCommand(async _ => { Telemetry.Ping("freeze_all"); await FreezeDisksAsync(true); });
        FreezeCDriveCommand = new RelayCommand(async _ => { Telemetry.Ping("freeze_c"); await FreezeDisksAsync(false); });
        ThawDisksCommand = new RelayCommand(async _ => { Telemetry.Ping("thaw"); await ThawDisksAsync(); });
        StartSeewoSetupCommand = new RelayCommand(async _ => { Telemetry.Ping("seewo_setup"); await StartSeewoSetupAsync(); });

        ActivateWindowsCommand = new RelayCommand(async _ => { Telemetry.Ping("activate_win"); await ShowInstallDialogAsync("Windows激活工具", "https://lz.qaiu.top/parser?url=https://www.ilanzou.com/s/lGZnCdtr", "windows_activator.exe"); });

        RebootToRecoveryCommand = new RelayCommand(async _ => { Telemetry.Ping("reboot_recovery"); await RebootToRecoveryAsync(); });

        DownloadOriginalWindowsCommand = new RelayCommand(_ =>
        {
            Telemetry.Ping("download_win");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://hellowindows.cn/", UseShellExecute = true });
        });
        OpenAuthorHomepageCommand = new RelayCommand(_ =>
        {
            Telemetry.Ping("open_homepage");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://space.bilibili.com/2019671591", UseShellExecute = true });
        });
        ContactAuthorCommand = new RelayCommand(async _ =>
        {
            try
            {
                var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(
                    (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow);
                if (topLevel != null)
                {
                    var clipboard = topLevel.Clipboard;
                    await clipboard.SetTextAsync("2218243488");
                }
                await ShowInfoDialogAsync("联系作者", "QQ号 2218243488 已复制到剪贴板\n请打开QQ添加好友");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "tencent://AddContact/?fromId=45&fromSubId=1&subcmd=all&uin=2218243488", UseShellExecute = true });
            }
            catch
            {
                await ShowInfoDialogAsync("联系作者", "QQ号：2218243488\n请手动复制并打开QQ添加好友");
            }
        });
        OpenAstraCoreCommand = new RelayCommand(_ =>
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://lz-s.cn/", UseShellExecute = true });
        });
        AdvancedConfigCommand = new RelayCommand(async _ =>
        {
            await ShowInfoDialogAsync("功能暂不可用", "高级配置功能当前暂不可用，请使用一键冻结功能。");
        });
        InstallClassIslandCommand = new RelayCommand(async _ => { Telemetry.Ping("install_classisland"); await InstallClassIslandAsync(); });
    }

    private static void UpdateThemeResources(bool isDark)
    {
        var r = Application.Current?.Resources;
        if (r == null) return;

        if (isDark)
        {
            r["PageBackgroundBrush"] = new SolidColorBrush(Color.Parse("#0D0D14"));
            r["CardBackgroundBrush"] = new SolidColorBrush(Color.Parse("#16161F"));
            r["ButtonHoverBrush"] = new SolidColorBrush(Color.Parse("#252535"));
            r["ButtonPressedBrush"] = new SolidColorBrush(Color.Parse("#2A2A40"));
            r["HeaderForegroundBrush"] = new SolidColorBrush(Color.Parse("#EAEAF2"));
            r["SubtextForegroundBrush"] = new SolidColorBrush(Color.Parse("#8888A0"));
            r["AccentBrush"] = new SolidColorBrush(Color.Parse("#9B8AFB"));
            r["AccentLightBrush"] = new SolidColorBrush(Color.Parse("#B8AAFF"));
            r["AccentDarkBrush"] = new SolidColorBrush(Color.Parse("#7B68EE"));
            r["CardBorderBrush"] = new SolidColorBrush(Color.Parse("#2A2A3E"));
            r["CardShadowBrush"] = new SolidColorBrush(Color.Parse("#20000000"));
            r["DialogBackgroundBrush"] = new SolidColorBrush(Color.Parse("#16161F"));
            r["DialogMaskBrush"] = new SolidColorBrush(Color.Parse("#80000000"));
            r["WindowBorderBrush"] = new SolidColorBrush(Color.Parse("#33334A"));
        }
        else
        {
            r["PageBackgroundBrush"] = new SolidColorBrush(Color.Parse("#F5F5FA"));
            r["CardBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
            r["ButtonHoverBrush"] = new SolidColorBrush(Color.Parse("#F0EFF8"));
            r["ButtonPressedBrush"] = new SolidColorBrush(Color.Parse("#E8E7F5"));
            r["HeaderForegroundBrush"] = new SolidColorBrush(Color.Parse("#1A1A2E"));
            r["SubtextForegroundBrush"] = new SolidColorBrush(Color.Parse("#8E8EA0"));
            r["AccentBrush"] = new SolidColorBrush(Color.Parse("#7B68EE"));
            r["AccentLightBrush"] = new SolidColorBrush(Color.Parse("#9B8AFB"));
            r["AccentDarkBrush"] = new SolidColorBrush(Color.Parse("#6C5CE7"));
            r["CardBorderBrush"] = new SolidColorBrush(Color.Parse("#E5E5F0"));
            r["CardShadowBrush"] = new SolidColorBrush(Color.Parse("#1000001A"));
            r["DialogBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
            r["DialogMaskBrush"] = new SolidColorBrush(Color.Parse("#60000000"));
            r["WindowBorderBrush"] = new SolidColorBrush(Color.Parse("#D0D0E0"));
        }
    }

    private async Task NavigateWithAnimation(bool showSeewo, bool showMore)
    {


        ShowSeewoSetup = showSeewo;
        ShowMoreTools = showMore;
        RaisePropertyChanged(nameof(ShowMainPage));

        await Task.Delay(50);
    }

    private async Task CheckForUpdateAsync()
    {
        IsUpdateChecking = true;
        UpdateStatus = "正在检查更新...";
        try
        {
            var (hasUpdate, latestVersion) = await UpdateService.CheckUpdateAsync("https://seewo.icu/version.txt", CurrentVersion);
            UpdateStatus = hasUpdate ? $"发现新版本 v{latestVersion}" : $"当前已是最新版本 v{CurrentVersion}";
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("检查更新失败", $"无法检查更新，请检查网络连接后重试。\n\n错误详情：{ex.Message}");
        }
        finally
        {
            IsUpdateChecking = false;
        }
    }

    private async Task InstallWeChatAsync()
    {
        var choice = await ShowThreeChoiceDialogAsync("选择微信版本", "请选择微信安装版本：", "安装32位(3.9.12)", "安装64位(最新版)", "取消");
        if (choice == 0) return;

        string url, filename, version;
        if (choice == 1)
        {
            url = "https://dldir1v6.qq.com/weixin/Windows/WeChatSetup_x86.exe";
            filename = "WeChatSetup_x86.exe";
            version = "32位(3.9.12)";
        }
        else
        {
            url = "https://dldir1v6.qq.com/weixin/Universal/Windows/WeChatWin.exe";
            filename = "WeChatWin.exe";
            version = "64位(最新版)";
        }

        if (!await ShowConfirmDialogAsync($"安装微信 {version}", $"即将下载并安装微信 {version}。\n\n确定要开始吗？"))
            return;

        UpdateStatus = $"正在安装微信 {version}，请稍候...";
        try
        {
            await Task.Run(async () => { await InstallService.SilentInstallAsync(url, filename); });
            UpdateStatus = $"微信 {version} 安装完成！";
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("微信安装失败", $"无法完成微信 {version} 的安装。\n\n错误详情：{ex.Message}");
        }
    }

    private async Task ShowInstallDialogAsync(string appName, string url, string filename)
    {
        if (!await ShowConfirmDialogAsync($"安装{appName}", $"即将下载并安装 {appName}。\n\n安装文件将保存到桌面，安装完成后自动清理。\n\n确定要开始吗？"))
            return;

        UpdateStatus = $"正在安装{appName}，请稍候...";

        try
        {
            await Task.Run(async () =>
            {
                await InstallService.SilentInstallAsync(url, filename);
            });
            UpdateStatus = $"{appName} 安装完成！";
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync($"{appName} 安装失败", $"无法完成 {appName} 的安装。\n\n错误详情：{ex.Message}");
        }
    }

    private async Task FreezeDisksAsync(bool allDrives)
    {
        string diskNames;
        if (allDrives)
        {
            diskNames = "全部磁盘";
            if (!await ShowConfirmDialogAsync("确认冻结", $"确认要冻结全部磁盘吗？\n\n操作完成后系统将自动重启。"))
                return;
        }
        else
        {
            diskNames = "C盘";
            if (!await ShowConfirmDialogAsync("确认冻结", $"确认要冻结 C 盘吗？\n\n操作完成后系统将自动重启。"))
                return;
        }

        try
        {
            UpdateStatus = $"正在冻结{diskNames}，请稍候...";
            if (allDrives)
                await FreezeService.FreezeAllAsync();
            else
                await FreezeService.FreezeDisksAsync(new[] { "C" });

            await ShowInfoDialogAsync("操作成功", $"已开始冻结{diskNames}。\n系统将自动重启...");
            UpdateStatus = $"{diskNames}冻结成功";
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("冻结失败", ex.Message);
        }
    }

    private async Task ThawDisksAsync()
    {
        if (!await ShowConfirmDialogAsync("确认解冻", "确认要解冻全部磁盘吗？\n\n解冻后可能需要重启才能生效。"))
            return;

        try
        {
            UpdateStatus = "正在解冻所有磁盘，请稍候...";
            await FreezeService.ThawAllAsync();

            var reboot = await ShowConfirmDialogAsync("解冻成功", "电脑已成功解除希沃冰点还原。\n\n是否立即重启电脑？\n（建议重启以确保操作生效）");
            if (reboot)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "shutdown", Arguments = "/r /t 0",
                    UseShellExecute = false, CreateNoWindow = true
                });
            }
            else
            {
                await ShowInfoDialogAsync("提示", "重启后操作生效！请手动重启！");
            }
            UpdateStatus = "磁盘解冻成功";
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("解冻失败", ex.Message);
        }
    }

    private async Task StartSeewoSetupAsync()
    {
        var selected = SeewoSoftwareList.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0)
        {
            await ShowInfoDialogAsync("提示", "请至少选择一个要安装的软件。");
            return;
        }

        DownloadProgress = 0;
        DownloadLog = string.Empty;
        DownloadSpeed = "0 B/s";
        IsDownloading = true;

        try
        {
            for (int i = 0; i < selected.Count; i++)
            {
                var software = selected[i];
                DownloadLog += $"[{i + 1}/{selected.Count}] 正在下载 {software.Name}...\n";

                await DownloadService.DownloadFileAsync(
                    software.Url, software.Filename,
                    SpeedLimit > 0 ? SpeedLimit * 1024 : 0,
                    p => DownloadProgress = (double)i / selected.Count + (p / selected.Count) * 0.99,
                    s => DownloadSpeed = s,
                    System.Threading.CancellationToken.None);

                DownloadLog += $"[{i + 1}/{selected.Count}] {software.Name} 下载完成\n";
            }
            DownloadProgress = 1.0;
            DownloadLog += "\n所有软件下载完成！";
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("下载失败", $"软件下载过程中出现错误。\n\n错误详情：{ex.Message}");
        }
        finally
        {
            IsDownloading = false;
        }
    }

    private async Task RebootToRecoveryAsync()
    {
        if (!await ShowConfirmDialogAsync("即将重启到恢复模式", "此操作将立即重启计算机并进入 Windows 恢复环境。\n\n请确保已保存所有工作。\n\n确定要继续吗？"))
            return;

        if (!await ShowConfirmDialogAsync("二次确认：重启到恢复模式", "再次提醒：重启后将进入恢复模式，所有未保存的数据将丢失。\n\n真的确定要继续吗？"))
            return;

        if (!await ShowConfirmDialogAsync("最终确认：重启到恢复模式", "这是最后一次确认机会！\n\n点击确定后将立即执行重启，无法撤销。\n\n确定要重启到恢复模式吗？"))
            return;

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "shutdown",
            Arguments = "/r /o /t 0",
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }

    private async Task InstallClassIslandAsync()
    {
        var choice = await ShowFourChoiceDialogAsync(
            "选择 ClassIsland 版本",
            "ClassIsland 是一个 Windows 桌面美化小组件工具。\n\n请选择版本（建议有冰点还原的设备选择含运行时版本）：",
            "X64 含运行时（推荐）",
            "X64 便携版",
            "ARM64 含运行时",
            "ARM64 便携版");

        if (choice == 0) return;

        string url, version;
        switch (choice)
        {
            case 1:
                url = "https://get.classisland.tech/d/ClassIsland-Ningbo-S3/classisland/distribution-v2/2.1/2.1.0.1/ClassIsland_app_windows_x64_selfContained_folder.zip";
                version = "X64 含运行时";
                break;
            case 2:
                url = "https://get.classisland.tech/d/ClassIsland-Ningbo-S3/classisland/distribution-v2/2.1/2.1.0.1/ClassIsland_app_windows_x64_full_folder.zip";
                version = "X64 便携版";
                break;
            case 3:
                url = "https://get.classisland.tech/d/ClassIsland-Ningbo-S3/classisland/distribution-v2/2.1/2.1.0.1/ClassIsland_app_windows_arm64_selfContained_folder.zip";
                version = "ARM64 含运行时";
                break;
            case 4:
                url = "https://get.classisland.tech/d/ClassIsland-Ningbo-S3/classisland/distribution-v2/2.1/2.1.0.1/ClassIsland_app_windows_arm64_full_folder.zip";
                version = "ARM64 便携版";
                break;
            default:
                return;
        }

        if (!await ShowConfirmDialogAsync($"安装 ClassIsland ({version})", $"即将下载 ClassIsland {version}。\n\n下载后将自动解压到桌面 ClassIsland 文件夹。\n\n确定要开始吗？"))
            return;

        UpdateStatus = $"正在下载 ClassIsland {version}，请稍候...";

        try
        {
            await Task.Run(async () =>
            {
                var zipPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "ClassIsland.zip");
                var extractDir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "ClassIsland");

                await DownloadService.DownloadFileAsync(url, "ClassIsland.zip", 0,
                    p => DownloadProgress = p,
                    s => DownloadSpeed = s,
                    System.Threading.CancellationToken.None);

                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractDir, true);
                System.IO.File.Delete(zipPath);
            });

            UpdateStatus = $"ClassIsland {version} 安装完成！已解压到桌面 ClassIsland 文件夹。";
            await ShowInfoDialogAsync("安装完成", $"ClassIsland {version} 已下载并解压到桌面 ClassIsland 文件夹。\n\n打开 ClassIsland 文件夹即可使用。");
        }
        catch (Exception ex)
        {
            await ShowErrorDialogAsync("安装失败", $"无法完成 ClassIsland 的下载安装。\n\n错误详情：{ex.Message}");
        }
    }


    private static Button CreateAccentButton(string text)
    {
        var res = Application.Current!.Resources;
        var bg = (res["AccentBrush"] as SolidColorBrush)?.Color ?? Color.Parse("#6C5CE7");
        return new Button
        {
            Content = text,
            Height = 42,
            CornerRadius = new Avalonia.CornerRadius(12),
            FontSize = 14,
            FontFamily = (FontFamily)res["ContentFont"]!,
            Background = new SolidColorBrush(bg),
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
        };
    }

    private static Button CreateGhostButton(string text)
    {
        var res = Application.Current!.Resources;
        return new Button
        {
            Content = text,
            Height = 42,
            CornerRadius = new Avalonia.CornerRadius(12),
            FontSize = 14,
            FontFamily = (FontFamily)res["ContentFont"]!,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
        };
    }

    private static TextBlock CreateDialogTitle(string text)
    {
        var res = Application.Current!.Resources;
        var fg = (res["HeaderForegroundBrush"] as SolidColorBrush)?.Color ?? Color.Parse("#1B1B2F");
        return new TextBlock
        {
            Text = text,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            FontFamily = (FontFamily)res["ContentFont"]!,
            Foreground = new SolidColorBrush(fg),
            Margin = new Thickness(0, 0, 0, 8),
        };
    }

    private static TextBlock CreateDialogMessage(string text)
    {
        var res = Application.Current!.Resources;
        var fg = (res["SubtextForegroundBrush"] as SolidColorBrush)?.Color ?? Color.Parse("#7C7C9A");
        return new TextBlock
        {
            Text = text,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 13,
            FontFamily = (FontFamily)res["ContentFont"]!,
            Foreground = new SolidColorBrush(fg),
            LineHeight = 20,
        };
    }

    private static Window CreateDialog(string title, int width, int height)
    {
        var res = Application.Current!.Resources;
        var bg = (res["DialogBackgroundBrush"] as SolidColorBrush)?.Color ?? Colors.White;

        var dialog = new Window
        {
            Title = title,
            Width = width,
            Height = height,
            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(bg),
            FontFamily = (FontFamily)res["ContentFont"]!,
        };

        PopupIntroAnimationBehavior.SetIsIntroAnimationEnabled(dialog, true);

        return dialog;
    }

    private static StackPanel CreateDialogContent()
    {
        return new StackPanel { Margin = new Thickness(32), Spacing = 4 };
    }

    private static async Task<bool> ShowConfirmDialogAsync(string title, string message)
    {
        var mainWindow = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null) return false;

        var tcs = new TaskCompletionSource<bool>();

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dialog = CreateDialog(title, 400, 280);
            var panel = CreateDialogContent();

            panel.Children.Add(CreateDialogTitle(title));
            panel.Children.Add(CreateDialogMessage(message));

            var spacer = new Border { Height = 20 };
            panel.Children.Add(spacer);

            var cancelBtn = CreateGhostButton("取消");
            cancelBtn.Click += (_, _) => { tcs.TrySetResult(false); dialog.Close(); };

            var confirmBtn = CreateAccentButton("确定");
            confirmBtn.Margin = new Thickness(0, 8, 0, 0);
            confirmBtn.Click += (_, _) => { tcs.TrySetResult(true); dialog.Close(); };

            panel.Children.Add(cancelBtn);
            panel.Children.Add(confirmBtn);

            dialog.Content = panel;
            dialog.Closed += (_, _) => tcs.TrySetResult(false);

            dialog.ShowDialog(mainWindow);
        });

        return await tcs.Task;
    }

    private static async Task ShowInfoDialogAsync(string title, string message)
    {
        var mainWindow = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null) return;

        var tcs = new TaskCompletionSource<bool>();

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dialog = CreateDialog(title, 400, 260);
            var panel = CreateDialogContent();

            panel.Children.Add(CreateDialogTitle(title));
            panel.Children.Add(CreateDialogMessage(message));

            var spacer = new Border { Height = 20 };
            panel.Children.Add(spacer);

            var okBtn = CreateAccentButton("确定");
            okBtn.Click += (_, _) => { tcs.TrySetResult(true); dialog.Close(); };

            panel.Children.Add(okBtn);

            dialog.Content = panel;
            dialog.Closed += (_, _) => tcs.TrySetResult(false);
            dialog.ShowDialog(mainWindow);
        });

        await tcs.Task;
    }

    private static async Task ShowErrorDialogAsync(string title, string message)
    {
        var mainWindow = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null) return;

        var tcs = new TaskCompletionSource<bool>();

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dialog = CreateDialog(title, 440, 320);
            var panel = CreateDialogContent();

            panel.Children.Add(CreateDialogTitle(title));
            panel.Children.Add(CreateDialogMessage(message));

            var spacer = new Border { Height = 20 };
            panel.Children.Add(spacer);

            var okBtn = CreateAccentButton("确定");
            okBtn.Click += (_, _) => { tcs.TrySetResult(true); dialog.Close(); };

            panel.Children.Add(okBtn);

            dialog.Content = panel;
            dialog.Closed += (_, _) => tcs.TrySetResult(false);
            dialog.ShowDialog(mainWindow);
        });

        await tcs.Task;
    }

    private static async Task<int> ShowThreeChoiceDialogAsync(string title, string message, string button1, string button2, string cancelButton)
    {
        var mainWindow = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null) return 0;

        var tcs = new TaskCompletionSource<int>();

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dialog = CreateDialog(title, 400, 340);
            var panel = CreateDialogContent();

            panel.Children.Add(CreateDialogTitle(title));
            panel.Children.Add(CreateDialogMessage(message));

            var spacer = new Border { Height = 20 };
            panel.Children.Add(spacer);

            var btn1Control = CreateAccentButton(button1);
            btn1Control.Click += (_, _) => { tcs.TrySetResult(1); dialog.Close(); };

            var btn2Control = CreateAccentButton(button2);
            btn2Control.Margin = new Thickness(0, 8, 0, 0);
            btn2Control.Click += (_, _) => { tcs.TrySetResult(2); dialog.Close(); };

            var cancelControl = CreateGhostButton(cancelButton);
            cancelControl.Margin = new Thickness(0, 8, 0, 0);
            cancelControl.Click += (_, _) => { tcs.TrySetResult(0); dialog.Close(); };

            panel.Children.Add(btn1Control);
            panel.Children.Add(btn2Control);
            panel.Children.Add(cancelControl);

            dialog.Content = panel;
            dialog.Closed += (_, _) => tcs.TrySetResult(0);
            dialog.ShowDialog(mainWindow);
        });

        return await tcs.Task;
    }

    private static async Task<int> ShowFourChoiceDialogAsync(string title, string message, string button1, string button2, string button3, string cancelButton)
    {
        var mainWindow = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null) return 0;

        var tcs = new TaskCompletionSource<int>();

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dialog = CreateDialog(title, 400, 420);
            var panel = CreateDialogContent();

            panel.Children.Add(CreateDialogTitle(title));
            panel.Children.Add(CreateDialogMessage(message));

            var spacer = new Border { Height = 20 };
            panel.Children.Add(spacer);

            var btn1Control = CreateAccentButton(button1);
            btn1Control.Click += (_, _) => { tcs.TrySetResult(1); dialog.Close(); };

            var btn2Control = CreateAccentButton(button2);
            btn2Control.Margin = new Thickness(0, 8, 0, 0);
            btn2Control.Click += (_, _) => { tcs.TrySetResult(2); dialog.Close(); };

            var btn3Control = CreateAccentButton(button3);
            btn3Control.Margin = new Thickness(0, 8, 0, 0);
            btn3Control.Click += (_, _) => { tcs.TrySetResult(3); dialog.Close(); };

            var cancelControl = CreateGhostButton(cancelButton);
            cancelControl.Margin = new Thickness(0, 8, 0, 0);
            cancelControl.Click += (_, _) => { tcs.TrySetResult(0); dialog.Close(); };

            panel.Children.Add(btn1Control);
            panel.Children.Add(btn2Control);
            panel.Children.Add(btn3Control);
            panel.Children.Add(cancelControl);

            dialog.Content = panel;
            dialog.Closed += (_, _) => tcs.TrySetResult(0);
            dialog.ShowDialog(mainWindow);
        });

        return await tcs.Task;
    }
}

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool>? _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter ?? "") ?? true;
    public void Execute(object? parameter) => _execute(parameter ?? "");
    public event EventHandler? CanExecuteChanged;
}




