using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT;

namespace FCharge
{
    public sealed partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            this.InitializeComponent();
        }

        public void ShowOverlay(int duration, Action callback)
        {
            // Initialize the presenter
            OverlappedPresenter presenter = OverlappedPresenter.Create();
            presenter.IsAlwaysOnTop = true;
            presenter.IsMinimizable = false;
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(false, false);

            // Initialize the acrylicController and configurationSource
            DesktopAcrylicController acrylicController = new();
            SystemBackdropConfiguration configurationSource = new();

            configurationSource.Theme = SystemBackdropTheme.Light;
            acrylicController.Kind = DesktopAcrylicKind.Thin;
            acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            acrylicController.SetSystemBackdropConfiguration(configurationSource);

            // Initialize the overlayWindow and overlayTimer
            AppWindow overlayWindow = this.AppWindow;
            DispatcherTimer overlayTimer = new();
            overlayTimer.Interval = TimeSpan.FromSeconds(duration);
            overlayTimer.Tick += (sender, e) =>
            {
                overlayWindow.Hide();
                overlayTimer.Stop();
                callback();
            };
            overlayTimer.Start();
            overlayWindow.SetPresenter(presenter);
            overlayWindow.Show();
            presenter.Maximize();
        }
    }
}
