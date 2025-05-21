namespace PrisonersDilemma.Core.Models
{
    public class SimulationStepResult
    {
        public SimulationStatus Status { get; set; }
        public int PrisonerNumber { get; set; } // Чей ход или кто нашел/не нашел
        public int BoxNumberOpened { get; set; } // Какую коробку открыли
        public int? ValueInBox { get; set; } // Что нашли в коробке
        public int AttemptsMade { get; set; } // Попыток сделано текущим заключенным
        public bool FoundNumberInBox { get; set; } // Найден ли номер именно в этой открытой коробке
    }
}
