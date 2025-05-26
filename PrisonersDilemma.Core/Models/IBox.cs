namespace PrisonersDilemma.Core.Models
{
    public interface IBox
    {
        int BoxNumber { get; } // Предполагаем, что номер коробки не меняется после создания
        int PrisonerNumberInside { get; } // Номер заключенного внутри тоже не меняется после инициализации
    }
}
