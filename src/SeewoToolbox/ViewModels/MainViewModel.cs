using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;
using SeewoToolbox.Models;
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

    public string ThemeIcon => IsDarkTheme ? "☀️" : "🌙";

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

    public MainViewModel()
    {
        ToggleThemeCommand = new RelayCommand(_ => IsDarkTheme = !IsDarkTheme);

        NavigateToSeewoSetupCommand = new RelayCommand(_ =>
        {
            ShowSeewoSetup = true;
            ShowMoreTools = false;
            RaisePropertyChanged(nameof(ShowMainPage));
        });

        NavigateToMoreToolsCommand = new RelayCommand(_ =>
        {
            ShowMoreTools = true;
            ShowSeewoSetup = false;
            RaisePropertyChanged(nameof(ShowMainPage));
        });

        NavigateBackCommand = new RelayCommand(_ =>
        {
            ShowSeewoSetup = false;
            ShowMoreTools = false;
            RaisePropertyChanged(nameof(ShowMainPage));
        });

        CheckUpdateCommand = new RelayCommand(async _ => await CheckForUpdateAsync());
        InstallWeChatCommand = new RelayCommand(async _ => await InstallWeChatAsync());
        InstallQQCommand = new RelayCommand(async _ => await ShowInstallDialogAsync("QQ", "https://dldir1v6.qq.com/qqfile/qq/PCQQ9.7.25/QQ9.7.25.29415.exe", "QQ9.7.25.29415.exe"));
        InstallPCManagerCommand = new RelayCommand(async _ => await ShowInstallDialogAsync("腾讯电脑管家", "https://pm.myapp.com/invc/xfspeed/qqpcmgr/download/QQPCDownload_home_1981.exe", "QQPCDownload_home_1981.exe"));

        FreezeAllDisksCommand = new RelayCommand(async _ => await FreezeDisksAsync(true));
        FreezeCDriveCommand = new RelayCommand(async _ => await FreezeDisksAsync(false));
        ThawDisksCommand = new RelayCommand(async _ => await ThawDisksAsync());
        StartSeewoSetupCommand = new RelayCommand(async _ => await StartSeewoSetupAsync());

        ActivateWindowsCommand = new RelayCommand(async _ => await ShowInstallDialogAsync("Windows激活工具", "https://lz.qaiu.top/parser?url=https://www.ilanzou.com/s/lGZnCdtr", "windows_activator.exe"));

        RebootToRecoveryCommand = new RelayCommand(async _ => await RebootToRecoveryAsync());

        DownloadOriginalWindowsCommand = new RelayCommand(_ =>
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://hellowindows.cn/", UseShellExecute = true });
        });
        OpenAuthorHomepageCommand = new RelayCommand(_ =>
        {
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
    }

    private static void UpdateThemeResources(bool isDark)
    {
        var resources = Application.Current?.Resources;
        if (resources == null) return;

        if (isDark)
        {
            resources["PageBackgroundBrush"] = new SolidColorBrush(Color.Parse("#0F0F1A"));
            resources["CardBackgroundBrush"] = new SolidColorBrush(Color.Parse("#1A1A2E"));
            resources["ButtonHoverBrush"] = new SolidColorBrush(Color.Parse("#2A2A45"));
            resources["HeaderForegroundBrush"] = new SolidColorBrush(Color.Parse("#EAEAF5"));
            resources["SubtextForegroundBrush"] = new SolidColorBrush(Color.Parse("#8888AA"));
            resources["AccentBrush"] = new SolidColorBrush(Color.Parse("#A29BFE"));
            resources["AccentLightBrush"] = new SolidColorBrush(Color.Parse("#6C5CE7"));
            resources["CardBorderBrush"] = new SolidColorBrush(Color.Parse("#2D2D50"));
        }
        else
        {
            resources["PageBackgroundBrush"] = new SolidColorBrush(Color.Parse("#F8F9FE"));
            resources["CardBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
            resources["ButtonHoverBrush"] = new SolidColorBrush(Color.Parse("#EEEDF5"));
            resources["HeaderForegroundBrush"] = new SolidColorBrush(Color.Parse("#1B1B2F"));
            resources["SubtextForegroundBrush"] = new SolidColorBrush(Color.Parse("#7C7C9A"));
            resources["AccentBrush"] = new SolidColorBrush(Color.Parse("#6C5CE7"));
            resources["AccentLightBrush"] = new SolidColorBrush(Color.Parse("#A29BFE"));
            resources["CardBorderBrush"] = new SolidColorBrush(Color.Parse("#E8E6F0"));
        }
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

            var reboot = await ShowConfirmDialogAsync("解冻成功", "电脑已成功解除磁盘保护服务。\n\n是否立即重启电脑？\n（建议重启以确保操作生效）");
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
        if (selected.Count == 0) return;

        DownloadProgress = 0;
        DownloadLog = string.Empty;
        DownloadSpeed = "0 B/s";

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

    // ===== Dialog helpers =====

    private static async Task<bool> ShowConfirmDialogAsync(string title, string message)
    {
        var mainWindow = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null) return false;

        var tcs = new TaskCompletionSource<bool>();

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            var dialog = new Window
            {
                Title = title,
                Width = 420,
                Height = 240,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = false,
            };

            var panel = new StackPanel { Margin = new Thickness(20), Spacing = 16 };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
            });

            var btnPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 16,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var cancelBtn = new Button
            {
                Content = "取消",
                Width = 100,
                Height = 36,
                CornerRadius = new Avalonia.CornerRadius(8),
                FontSize = 14,
            };
            cancelBtn.Click += (_, _) =>
            {
                tcs.TrySetResult(false);
                dialog.Close();
            };

            var confirmBtn = new Button
            {
                Content = "确定",
                Width = 100,
                Height = 36,
                CornerRadius = new Avalonia.CornerRadius(8),
                FontSize = 14,
                Background = new SolidColorBrush(Color.Parse("#6C5CE7")),
                Foreground = new SolidColorBrush(Colors.White),
            };
            confirmBtn.Click += (_, _) =>
            {
                tcs.TrySetResult(true);
                dialog.Close();
            };

            btnPanel.Children.Add(cancelBtn);
            btnPanel.Children.Add(confirmBtn);
            panel.Children.Add(btnPanel);

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
            var dialog = new Window
            {
                Title = title,
                Width = 420,
                Height = 220,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = false,
            };

            var panel = new StackPanel { Margin = new Thickness(20), Spacing = 16 };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
            });

            var okBtn = new Button
            {
                Content = "确定",
                Width = 100,
                Height = 36,
                CornerRadius = new Avalonia.CornerRadius(8),
                FontSize = 14,
                Background = new SolidColorBrush(Color.Parse("#6C5CE7")),
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            };
            okBtn.Click += (_, _) =>
            {
                tcs.TrySetResult(true);
                dialog.Close();
            };

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
            var dialog = new Window
            {
                Title = title,
                Width = 440,
                Height = 260,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = false,
            };

            var panel = new StackPanel { Margin = new Thickness(20), Spacing = 16 };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
            });

            var okBtn = new Button
            {
                Content = "确定",
                Width = 100,
                Height = 36,
                CornerRadius = new Avalonia.CornerRadius(8),
                FontSize = 14,
                Background = new SolidColorBrush(Color.Parse("#6C5CE7")),
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            };
            okBtn.Click += (_, _) =>
            {
                tcs.TrySetResult(true);
                dialog.Close();
            };

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
            var dialog = new Window
            {
                Title = title,
                Width = 440,
                Height = 220,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                CanResize = false,
            };

            var panel = new StackPanel { Margin = new Thickness(20), Spacing = 16 };

            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
            });

            var btnPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 16,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var btn1 = new Button
            {
                Content = button1,
                Width = 120,
                Height = 36,
                CornerRadius = new Avalonia.CornerRadius(8),
                FontSize = 13,
                Background = new SolidColorBrush(Color.Parse("#6C5CE7")),
                Foreground = new SolidColorBrush(Colors.White),
            };
            btn1.Click += (_, _) => { tcs.TrySetResult(1); dialog.Close(); };

            var btn2 = new Button
            {
                Content = button2,
                Width = 120,
                Height = 36,
                CornerRadius = new Avalonia.CornerRadius(8),
                FontSize = 13,
                Background = new SolidColorBrush(Color.Parse("#6C5CE7")),
                Foreground = new SolidColorBrush(Colors.White),
            };
            btn2.Click += (_, _) => { tcs.TrySetResult(2); dialog.Close(); };

            var cancelBtn = new Button
            {
                Content = cancelButton,
                Width = 80,
                Height = 36,
                CornerRadius = new Avalonia.CornerRadius(8),
                FontSize = 13,
            };
            cancelBtn.Click += (_, _) => { tcs.TrySetResult(0); dialog.Close(); };

            btnPanel.Children.Add(btn1);
            btnPanel.Children.Add(btn2);
            btnPanel.Children.Add(cancelBtn);
            panel.Children.Add(btnPanel);

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

