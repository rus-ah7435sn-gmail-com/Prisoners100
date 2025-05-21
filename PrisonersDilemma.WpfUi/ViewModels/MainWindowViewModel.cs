using Prism.Mvvm;

namespace PrisonersDilemma.WpfUi.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Загадка 100 заключенных";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
