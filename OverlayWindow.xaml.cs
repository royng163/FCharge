using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT;

namespace FCharge
{
    public sealed partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            this.InitializeComponent();
            this.AppWindow.SetPresenter(FullScreenPresenter.Create());

            DesktopAcrylicController acrylicController = new();
            acrylicController.Kind = DesktopAcrylicKind.Thin;
            acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        }
    }
}
