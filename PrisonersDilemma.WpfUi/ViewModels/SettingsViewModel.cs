using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrisonersDilemma.WpfUi.Views; // Required for nameof(SimulationView)

namespace PrisonersDilemma.WpfUi.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        private int _numberOfPrisoners = 100;
        public int NumberOfPrisoners
        {
            get { return _numberOfPrisoners; }
            set 
            { 
                SetProperty(ref _numberOfPrisoners, value);
                StartSimulationCommand.RaiseCanExecuteChanged();
            }
        }

        private int _maxAttempts = 50;
        public int MaxAttempts
        {
            get { return _maxAttempts; }
            set 
            { 
                SetProperty(ref _maxAttempts, value);
                StartSimulationCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand StartSimulationCommand { get; private set; }

        public SettingsViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            StartSimulationCommand = new DelegateCommand(ExecuteStartSimulation, CanExecuteStartSimulation);
        }

        private void ExecuteStartSimulation()
        {
            var parameters = new NavigationParameters
            {
                { "NumberOfPrisoners", NumberOfPrisoners },
                { "MaxAttempts", MaxAttempts }
            };
            // Assuming "ContentRegion" is the name of the region in MainWindow
            _regionManager.RequestNavigate("ContentRegion", nameof(SimulationView), parameters);
        }

        private bool CanExecuteStartSimulation()
        {
            return NumberOfPrisoners > 0 && MaxAttempts > 0;
        }
    }
}
