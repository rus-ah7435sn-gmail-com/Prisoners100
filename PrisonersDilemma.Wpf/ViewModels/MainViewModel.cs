using Prism.Commands;
using Prism.Mvvm;
using PrisonersDilemma.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; // Для Dispatcher

namespace PrisonersDilemma.Wpf.ViewModels;

public class MainViewModel : BindableBase
{
    private readonly IRoom _room; 
    private ISimulation _simulation;

    private int _numberOfPrisoners = 100;
    public int NumberOfPrisoners
    {
        get => _numberOfPrisoners;
        set => SetProperty(ref _numberOfPrisoners, value, () => RunSimulationCommand.RaiseCanExecuteChanged());
    }

    private int _maxAttemptsPerPrisoner = 50;
    public int MaxAttemptsPerPrisoner
    {
        get => _maxAttemptsPerPrisoner;
        set => SetProperty(ref _maxAttemptsPerPrisoner, value, () => RunSimulationCommand.RaiseCanExecuteChanged());
    }

    private string _simulationResult = "Симуляция еще не запускалась.";
    public string SimulationResult
    {
        get => _simulationResult;
        set => SetProperty(ref _simulationResult, value);
    }

    private string _simulationStatus = "Готов к запуску.";
    public string SimulationStatus
    {
        get => _simulationStatus;
        set => SetProperty(ref _simulationStatus, value);
    }

    public ObservableCollection<string> SimulationLogMessages { get; } = new ObservableCollection<string>();
    public ObservableCollection<BoxViewModel> BoxesToDisplay { get; } = new ObservableCollection<BoxViewModel>();

    public DelegateCommand RunSimulationCommand { get; }

    private bool _isSimulationRunning;
    public bool IsSimulationRunning
    {
        get => _isSimulationRunning;
        set
        {
            SetProperty(ref _isSimulationRunning, value);
            RunSimulationCommand.RaiseCanExecuteChanged(); 
        }
    }
    
    // Конструктор MainViewModel
    // IRoom будет внедрен через DI благодаря регистрации в App.xaml.cs
    public MainViewModel(IRoom room) 
    {
        _room = room ?? throw new ArgumentNullException(nameof(room));
        
        RunSimulationCommand = new DelegateCommand(async () => await ExecuteRunSimulationCommand(), CanExecuteRunSimulationCommand);
        
        // Начальная инициализация коробок (пустых)
        InitializeBoxesForDisplay(NumberOfPrisoners); 
    }
    
    private bool CanExecuteRunSimulationCommand()
    {
        return !IsSimulationRunning && NumberOfPrisoners > 0 && MaxAttemptsPerPrisoner > 0 && MaxAttemptsPerPrisoner <= NumberOfPrisoners;
    }

    private void InitializeBoxesForDisplay(int count)
    {
        // Убедимся, что обновление происходит в UI потоке
        Application.Current.Dispatcher.Invoke(() =>
        {
            BoxesToDisplay.Clear();
            for (int i = 1; i <= count; i++)
            {
                BoxesToDisplay.Add(new BoxViewModel(i));
            }
        });
    }

    private async Task ExecuteRunSimulationCommand()
    {
        IsSimulationRunning = true;
        SimulationLogMessages.Clear();
        SimulationResult = string.Empty;
        SimulationStatus = "Инициализация симуляции...";
        LogMessageOnUiThread("Симуляция начинается...");

        // Инициализируем или обновляем коробки для отображения
        InitializeBoxesForDisplay(NumberOfPrisoners);

        // Создаем экземпляр Simulation. 
        // Параметры (NumberOfPrisoners, MaxAttemptsPerPrisoner) берутся из свойств ViewModel.
        _simulation = new Simulation(NumberOfPrisoners, MaxAttemptsPerPrisoner, _room);
        
        _simulation.SimulationLog += OnSimulationLog; 

        await Task.Run(async () => 
        {
            try
            {
                LogMessageOnUiThread("Подготовка комнаты и перемешивание коробок...");
                _room.SetupBoxes(NumberOfPrisoners); // Это должно быть вызвано перед тем, как заключенные начнут поиск
                
                // Обновляем BoxViewModel данными из _room.Boxes (номера внутри)
                // Это не часть логики симуляции, а подготовка UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var boxVm in BoxesToDisplay)
                    {
                        var coreBox = _room.Boxes.FirstOrDefault(b => b.BoxNumber == boxVm.BoxNumber);
                        if (coreBox != null)
                        {
                            // PrisonerNumberInside будет отображаться в ToolTip или если IsOpened = true
                            boxVm.PrisonerNumberInside = coreBox.PrisonerNumberInside; 
                        }
                        // Сбрасываем визуальные состояния перед каждым запуском
                        boxVm.IsOpened = false; 
                        boxVm.IsBeingChecked = false;
                        boxVm.ContainsMatchingNumber = false;
                    }
                    LogMessageOnUiThread("Коробки подготовлены для визуализации.");
                });

                // Запуск основной логики симуляции
                LogMessageOnUiThread("Запуск логики симуляции...");
                _simulation.Run(); 

                // Обновление UI после завершения симуляции
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SimulationResult = _simulation.IsSuccessful ? "Успех! Все заключенные нашли свои номера." : "Неудача. Не все заключенные нашли свои номера.";
                    SimulationStatus = "Симуляция завершена.";
                    LogMessageOnUiThread($"Результат: {SimulationResult}");
                    LogMessageOnUiThread("Симуляция завершена.");
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SimulationResult = "Ошибка во время симуляции.";
                    SimulationStatus = $"Ошибка: {ex.Message}";
                    LogMessageOnUiThread($"Критическая ошибка: {ex.ToString()}");
                });
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsSimulationRunning = false;
                });
                if (_simulation != null)
                {
                    _simulation.SimulationLog -= OnSimulationLog; 
                }
            }
        });
    }

    private void OnSimulationLog(string message)
    {
        LogMessageOnUiThread(message);
    }

    private void LogMessageOnUiThread(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            SimulationLogMessages.Add(message);
            // Можно также обновлять SimulationStatus на основе определенных логов, если это необходимо
            // Например: if (message.Contains("начинает поиск")) SimulationStatus = message;
        });
    }
}
