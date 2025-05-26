using Prism.Ioc;
using Prism.Unity;
using PrisonersDilemma.Core;
using PrisonersDilemma.Wpf.ViewModels;
using PrisonersDilemma.Wpf.Views;
using System.Windows;
using Prism.Mvvm; // Для ViewModelLocationProvider

namespace PrisonersDilemma.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        // Этот метод должен возвращать главное окно приложения.
        // Prism автоматически создаст его и свяжет с ViewModel, если настроено.
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Регистрация сервисов из Core
        containerRegistry.RegisterSingleton<IRoom, Room>();
        
        // Регистрируем ISimulation как transient, чтобы каждый раз создавался новый экземпляр
        // при запросе из MainViewModel, т.к. параметры симуляции (кол-во заключенных, попыток) могут меняться.
        // MainViewModel будет отвечать за передачу этих параметров в конструктор Simulation.
        // Unity может автоматически разрешать ISimulation, если его конструктор имеет параметры,
        // которые также зарегистрированы в контейнере (например, IRoom).
        // Если параметры ISimulation (numberOfPrisoners, maxAttempts) не могут быть разрешены контейнером,
        // MainViewModel придется создавать Simulation вручную или использовать фабрику.
        // В данном случае, MainViewModel будет создавать Simulation вручную.
        // Поэтому явная регистрация ISimulation здесь не обязательна, если он не будет разрешаться через контейнер с параметрами.
        // containerRegistry.Register<ISimulation, Simulation>(); // Оставим пока закомментированным

        // Регистрация ViewModel для View.
        // ViewModelLocationProvider используется Prism для автоматического связывания View и ViewModel.
        // Если ViewModel называется отлично от конвенции (ViewNameViewModel), нужно настроить это.
        // См. ConfigureViewModelLocator ниже.
    }

    protected override void ConfigureViewModelLocator()
    {
        base.ConfigureViewModelLocator();
        // Связываем MainWindow (из Views) с MainViewModel (из ViewModels)
        ViewModelLocationProvider.Register<MainWindow, MainViewModel>();
    }
}
