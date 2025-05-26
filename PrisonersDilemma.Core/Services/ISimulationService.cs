using System.Collections.Generic;
using PrisonersDilemma.Core.Models; // Для SimulationStepResult и IBox (если GetBoxes будет возвращать IReadOnlyList<IBox>)

namespace PrisonersDilemma.Core.Services
{
    public interface ISimulationService
    {
        void InitializeSimulation(int numberOfPrisoners, int maxAttemptsPerPrisoner);
        SimulationStepResult NextStep();
        IReadOnlyList<IBox> GetBoxes(); // Изменяем Box на IBox
    }
}
