using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrisonersDilemma.Core.Models;
using PrisonersDilemma.Core.Services;
using PrisonersDilemma.WpfUi.Views; // For nameof(SettingsView)
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading; // For DispatcherTimer

namespace PrisonersDilemma.WpfUi.ViewModels
{
    public class SimulationViewModel : BindableBase, INavigationAware
    {
        private readonly ISimulationService _simulationService; // Changed to ISimulationService
        private readonly IRegionManager _regionManager;

        private int _numberOfPrisoners;
        private int _maxAttempts;
        private SimulationStatus _currentSimulationStatus = SimulationStatus.NotStarted;
        private DispatcherTimer _autoStepTimer;

        private ObservableCollection<BoxViewModel> _boxes;
        public ObservableCollection<BoxViewModel> Boxes
        {
            get { return _boxes; }
            set { SetProperty(ref _boxes, value); }
        }

        private string _simulationResult;
        public string SimulationResult
        {
            get { return _simulationResult; }
            set { SetProperty(ref _simulationResult, value); }
        }

        private bool _isSimulationActive; // Renamed from IsSimulationRunning for clarity
        public bool IsSimulationActive
        {
            get { return _isSimulationActive; }
            set
            {
                SetProperty(ref _isSimulationActive, value);
                NextStepCommand.RaiseCanExecuteChanged();
                ResetSimulationCommand.RaiseCanExecuteChanged(); // Or just always enabled
                ToggleAutoModeCommand.RaiseCanExecuteChanged();
            }
        }

        private string _currentPrisonerInfo;
        public string CurrentPrisonerInfo
        {
            get { return _currentPrisonerInfo; }
            set { SetProperty(ref _currentPrisonerInfo, value); }
        }

        private bool _isAutoModeActive;
        public bool IsAutoModeActive
        {
            get { return _isAutoModeActive; }
            set
            {
                SetProperty(ref _isAutoModeActive, value);
                // Update button text or state if needed, e.g., "Start Auto" / "Stop Auto"
                NextStepCommand.RaiseCanExecuteChanged(); // Manual next step is disabled in auto mode
            }
        }

        private int _autoModeDelayMilliseconds = 500;
        public int AutoModeDelayMilliseconds
        {
            get { return _autoModeDelayMilliseconds; }
            set
            {
                SetProperty(ref _autoModeDelayMilliseconds, value);
                if (_autoStepTimer != null && _autoStepTimer.IsEnabled)
                {
                    _autoStepTimer.Interval = TimeSpan.FromMilliseconds(value);
                }
            }
        }

        public DelegateCommand NextStepCommand { get; private set; }
        public DelegateCommand ResetSimulationCommand { get; private set; }
        public DelegateCommand ToggleAutoModeCommand { get; private set; }


        public SimulationViewModel(ISimulationService simulationService, IRegionManager regionManager) // Changed to ISimulationService
        {
            _simulationService = simulationService;
            _regionManager = regionManager;
            
            Boxes = new ObservableCollection<BoxViewModel>();
            
            NextStepCommand = new DelegateCommand(ExecuteNextStep, CanExecuteNextStep);
            ResetSimulationCommand = new DelegateCommand(ExecuteResetSimulation);
            ToggleAutoModeCommand = new DelegateCommand(ExecuteToggleAutoMode, CanExecuteToggleAutoMode);
        }

        private void InitializeUiForSimulation()
        {
            _simulationService.InitializeSimulation(_numberOfPrisoners, _maxAttempts);
            _currentSimulationStatus = SimulationStatus.NotStarted;
            IsAutoModeActive = false; // Ensure auto mode is off on new simulation
            StopAutoMode(); // Stop timer if it was running

            Boxes.Clear();
            var boxesFromService = _simulationService.GetBoxes();
            if (boxesFromService != null)
            {
                foreach (var coreBox in boxesFromService)
                {
                    Boxes.Add(new BoxViewModel
                    {
                        BoxNumber = coreBox.BoxNumber,
                        PrisonerNumberInside = null, // Initially hidden
                        IsOpened = false,
                        IsCorrectNumber = false,
                        IsCurrentPrisonersPath = false
                    });
                }
            }

            CurrentPrisonerInfo = "Нажмите 'Следующий шаг' или 'Авто' для начала.";
            SimulationResult = string.Empty;
            IsSimulationActive = true; // This will trigger CanExecute on commands
        }

        private void ExecuteNextStep()
        {
            if (_currentSimulationStatus == SimulationStatus.Failed || _currentSimulationStatus == SimulationStatus.AllPrisonersSucceeded)
            {
                return;
            }

            SimulationStepResult result = _simulationService.NextStep();
            _currentSimulationStatus = result.Status;

            // Reset path highlighting for previous prisoner if new turn or success
            if (result.Status == SimulationStatus.NextPrisonerTurn || result.Status == SimulationStatus.FoundNumber)
            {
                // More precise: clear path only for the prisoner who just finished.
                // For now, let's clear all paths if a new prisoner starts or one succeeds.
                // If it's FoundNumber, we'll re-highlight the path for the current successful prisoner.
                if (result.Status == SimulationStatus.NextPrisonerTurn)
                {
                     foreach (var bvm in Boxes) { bvm.IsCurrentPrisonersPath = false; }
                }
            }
            
            BoxViewModel openedBoxVm = null;
            if (result.BoxNumberOpened > 0)
            {
                openedBoxVm = Boxes.FirstOrDefault(b => b.BoxNumber == result.BoxNumberOpened);
                if (openedBoxVm != null)
                {
                    openedBoxVm.IsOpened = true;
                    openedBoxVm.PrisonerNumberInside = result.ValueInBox;
                    openedBoxVm.IsCurrentPrisonersPath = true; 
                    openedBoxVm.IsCorrectNumber = result.FoundNumberInBox;
                }
            }

            UpdateCurrentPrisonerInfo(result);

            switch (result.Status)
            {
                case SimulationStatus.FoundNumber:
                    // CurrentPrisonerInfo updated by UpdateCurrentPrisonerInfo
                    // Optional: Highlight the entire path for this prisoner.
                    // This would require SimulationService to expose the path or re-trace it here.
                    // For now, individual box is marked by IsCurrentPrisonersPath = true
                    break;
                case SimulationStatus.NextPrisonerTurn:
                     foreach (var bvm in Boxes) { bvm.IsCurrentPrisonersPath = false; } // Clear path for new prisoner
                    // CurrentPrisonerInfo updated by UpdateCurrentPrisonerInfo
                    break;
                case SimulationStatus.Failed:
                    SimulationResult = $"Симуляция провалена! Заключенный {result.PrisonerNumber} не нашел свой номер после {result.AttemptsMade} попыток.";
                    if (result.BoxNumberOpened > 0 && openedBoxVm != null) { // If failure on a specific box opening
                        SimulationResult += $" (Последняя открытая коробка: {result.BoxNumberOpened} с числом {result.ValueInBox})";
                    }
                    IsSimulationActive = false;
                    StopAutoMode();
                    break;
                case SimulationStatus.AllPrisonersSucceeded:
                    SimulationResult = "Успех! Все заключенные нашли свои номера.";
                    IsSimulationActive = false;
                    StopAutoMode();
                    break;
                case SimulationStatus.Searching:
                    // CurrentPrisonerInfo updated by UpdateCurrentPrisonerInfo
                    break;
            }
            NextStepCommand.RaiseCanExecuteChanged();
            ToggleAutoModeCommand.RaiseCanExecuteChanged();
        }

        private void UpdateCurrentPrisonerInfo(SimulationStepResult result)
        {
            if (result.Status == SimulationStatus.NextPrisonerTurn)
            {
                 CurrentPrisonerInfo = $"Начинает заключенный {result.PrisonerNumber}. Попытка {result.AttemptsMade +1}/{_maxAttempts}";
            }
            else if (result.Status == SimulationStatus.FoundNumber)
            {
                CurrentPrisonerInfo = $"Заключенный {result.PrisonerNumber} НАШЕЛ свой номер в коробке {result.BoxNumberOpened} за {result.AttemptsMade} попыток!";
            }
            else if (result.Status == SimulationStatus.Searching)
            {
                CurrentPrisonerInfo = $"Ход заключенного {result.PrisonerNumber}. Попытка {result.AttemptsMade}/{_maxAttempts}. Открыта коробка {result.BoxNumberOpened}, внутри {result.ValueInBox}.";
            }
            else if (_currentSimulationStatus == SimulationStatus.NotStarted)
            {
                CurrentPrisonerInfo = "Нажмите 'Следующий шаг' или 'Авто' для начала.";
            }
            // For Failed/AllPrisonersSucceeded, SimulationResult is used.
        }

        private bool CanExecuteNextStep()
        {
            return IsSimulationActive && 
                   _currentSimulationStatus != SimulationStatus.Failed &&
                   _currentSimulationStatus != SimulationStatus.AllPrisonersSucceeded &&
                   !IsAutoModeActive; // Cannot manually step if auto mode is active
        }
        
        private void ExecuteResetSimulation()
        {
            StopAutoMode();
            IsSimulationActive = false; // Stop current simulation activities
            _regionManager.RequestNavigate("ContentRegion", nameof(SettingsView));
        }

        private void ExecuteToggleAutoMode()
        {
            IsAutoModeActive = !IsAutoModeActive;

            if (IsAutoModeActive)
            {
                _autoStepTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(AutoModeDelayMilliseconds) };
                _autoStepTimer.Tick += (s, e) => 
                { 
                    if (CanExecuteNextStepInternal()) // Use internal check that ignores IsAutoModeActive
                    {
                        ExecuteNextStep(); 
                    }
                    else 
                    {
                        StopAutoMode(); 
                    }
                };
                _autoStepTimer.Start();
            }
            else
            {
                StopAutoMode();
            }
            // Update CanExecute for NextStepCommand as it depends on IsAutoModeActive
            NextStepCommand.RaiseCanExecuteChanged();
            ToggleAutoModeCommand.RaiseCanExecuteChanged(); // Reflects current auto mode state
        }

        private bool CanExecuteToggleAutoMode()
        {
             return IsSimulationActive && 
                   _currentSimulationStatus != SimulationStatus.Failed &&
                   _currentSimulationStatus != SimulationStatus.AllPrisonersSucceeded;
        }

        private bool CanExecuteNextStepInternal() // Used by timer, bypasses IsAutoModeActive check
        {
             return IsSimulationActive && 
                   _currentSimulationStatus != SimulationStatus.Failed &&
                   _currentSimulationStatus != SimulationStatus.AllPrisonersSucceeded;
        }

        private void StopAutoMode()
        {
            _autoStepTimer?.Stop();
            _autoStepTimer = null;
            IsAutoModeActive = false; 
            NextStepCommand.RaiseCanExecuteChanged(); // Re-enable manual step if simulation is still active
            ToggleAutoModeCommand.RaiseCanExecuteChanged();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _numberOfPrisoners = navigationContext.Parameters.GetValue<int>("NumberOfPrisoners");
            _maxAttempts = navigationContext.Parameters.GetValue<int>("MaxAttempts");
            InitializeUiForSimulation();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            // Check if parameters are the same to avoid re-initialization if not needed.
            // For simplicity, always re-initialize for now.
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            StopAutoMode(); // Ensure timer is stopped when navigating away
            IsSimulationActive = false; // Mark as inactive
        }
    }
}
