using Prism.Mvvm;

namespace PrisonersDilemma.WpfUi.ViewModels
{
    public class BoxViewModel : BindableBase
    {
        private int _boxNumber;
        public int BoxNumber
        {
            get { return _boxNumber; }
            set { SetProperty(ref _boxNumber, value); }
        }

        private int? _prisonerNumberInside;
        public int? PrisonerNumberInside
        {
            get { return _prisonerNumberInside; }
            // Make sure this is updated only when the box is opened or for initial setup.
            // UI should ideally hide this if IsOpened is false.
            set { SetProperty(ref _prisonerNumberInside, value); }
        }

        private bool _isOpened;
        public bool IsOpened
        {
            get { return _isOpened; }
            set { SetProperty(ref _isOpened, value); }
        }

        private bool _isCorrectNumber;
        public bool IsCorrectNumber
        {
            get { return _isCorrectNumber; }
            set { SetProperty(ref _isCorrectNumber, value); }
        }

        private bool _isCurrentPrisonersPath;
        public bool IsCurrentPrisonersPath
        {
            get { return _isCurrentPrisonersPath; }
            set { SetProperty(ref _isCurrentPrisonersPath, value); }
        }

        public BoxViewModel()
        {
            // PrisonerNumberInside is nullable, so it's null by default.
            // No specific initialization needed here for it to be initially hidden if IsOpened is false.
        }
    }
}
