using Prism.Mvvm;

namespace PrisonersDilemma.Wpf.ViewModels;

/// <summary>
/// ViewModel для представления одной коробки в UI.
/// </summary>
public class BoxViewModel : BindableBase
{
    private int _boxNumber;
    /// <summary>
    /// Номер коробки.
    /// </summary>
    public int BoxNumber
    {
        get => _boxNumber;
        set => SetProperty(ref _boxNumber, value);
    }

    private int? _prisonerNumberInside;
    /// <summary>
    /// Номер заключенного внутри коробки. Null, если неизвестно или скрыто.
    /// </summary>
    public int? PrisonerNumberInside
    {
        get => _prisonerNumberInside;
        set => SetProperty(ref _prisonerNumberInside, value);
    }

    private bool _isOpened;
    /// <summary>
    /// Открыта ли коробка. (Для будущей детальной визуализации)
    /// </summary>
    public bool IsOpened
    {
        get => _isOpened;
        set => SetProperty(ref _isOpened, value);
    }

    private bool _isBeingChecked;
    /// <summary>
    /// Проверяется ли коробка в данный момент заключенным. (Для будущей детальной визуализации)
    /// </summary>
    public bool IsBeingChecked
    {
        get => _isBeingChecked;
        set => SetProperty(ref _isBeingChecked, value);
    }

    private bool _containsMatchingNumber;
    /// <summary>
    /// Содержит ли коробка номерок, который ищет текущий заключенный. (Для будущей детальной визуализации)
    /// </summary>
    public bool ContainsMatchingNumber
    {
        get => _containsMatchingNumber;
        set => SetProperty(ref _containsMatchingNumber, value);
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="boxNumber">Номер коробки.</param>
    public BoxViewModel(int boxNumber)
    {
        BoxNumber = boxNumber;
        PrisonerNumberInside = null; 
        IsOpened = false;
        IsBeingChecked = false;
        ContainsMatchingNumber = false;
    }
}
