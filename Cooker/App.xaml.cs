using Cooker.Pages;
using Cooker.Services;

namespace Cooker;

public partial class App : Application
{
    public readonly INotificationService notificationService;

    public App(INotificationService service)
    {
        InitializeComponent();
        notificationService = service;
        ApplySavedTheme();
    }

    void ApplySavedTheme()
    {
        int theme = Preferences.Default.Get("theme", 2);

        UserAppTheme = theme switch
        {
            0 => AppTheme.Light,
            1 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();

        var window = new Window(shell);

        if (window.Page != null)
        {
            ApplyBackgroundToPage(window.Page);
        }

        return window;
    }

    public static void ApplyBackgroundToPage(Page page)
    {
        if (page is not ContentPage contentPage)
            return;

        string path = Preferences.Default.Get("background_image", string.Empty);

        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            contentPage.BackgroundImageSource = path;
        }
        else
        {
            contentPage.BackgroundImageSource = null;
        }
    }
}