using Prism.Ioc;
using Prism.Regions;
using PrisonersDilemma.Core.Services;
using PrisonersDilemma.WpfUi.ViewModels;
using PrisonersDilemma.WpfUi.Views;
using System.Windows;

namespace PrisonersDilemma.WpfUi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register Services
            containerRegistry.RegisterSingleton<ISimulationService, SimulationService>();

            // Register Views for Navigation
            // Make sure the view model names match exactly if you rely on naming conventions
            // or explicitly provide them if they differ.
            containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
            containerRegistry.RegisterForNavigation<SimulationView, SimulationViewModel>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var regionManager = Container.Resolve<IRegionManager>();
            // The region name in MainWindow.xaml is "ContentRegion"
            // The view name for navigation is typically the class name of the view.
            regionManager.RequestNavigate("ContentRegion", nameof(SettingsView)); 
        }
    }
}
