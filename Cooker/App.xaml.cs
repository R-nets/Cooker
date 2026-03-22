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
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(
            new NavigationPage(
                new HomePage(notificationService)
            )
        );
    }
}