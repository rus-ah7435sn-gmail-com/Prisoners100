namespace PrisonersDilemma.Core.Models
{
    public enum SimulationStatus
    {
        NotStarted,         // Симуляция еще не началась
        Searching,          // Заключенный ищет свой номер
        FoundNumber,        // Заключенный нашел свой номер
        NextPrisonerTurn,   // Переход хода к следующему заключенному
        Failed,             // Симуляция провалена (кто-то не нашел номер или все попытки исчерпаны)
        AllPrisonersSucceeded // Все заключенные успешно нашли свои номера
    }
}
