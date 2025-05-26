using System;
using System.Collections.Generic;
using System.Linq;
using PrisonersDilemma.Core.Models; // SimulationStatus and SimulationStepResult are here

namespace PrisonersDilemma.Core.Services
{
    public class SimulationService : ISimulationService
    {
        private List<Box> _boxes = new List<Box>(); // Initialized
        private Random _random = new Random();
        private int _numberOfPrisoners;
        private int _maxAttempts;
        private int _currentPrisonerNumber;
        private int _attemptsMadeByCurrentPrisoner;
        private int _lastOpenedBoxNumberValue; // Renamed to avoid conflict with a potential property
        private bool _currentPrisonerFoundHisNumber;
        private List<int> _prisonersWhoFoundTheirNumber = new List<int>(); // Initialized


        public void InitializeSimulation(int numberOfPrisoners, int maxAttemptsPerPrisoner)
        {
            _numberOfPrisoners = numberOfPrisoners;
            _maxAttempts = maxAttemptsPerPrisoner;

            _boxes = new List<Box>(numberOfPrisoners);
            List<int> prisonerNumbersToPlace = new List<int>(numberOfPrisoners);

            for (int i = 1; i <= numberOfPrisoners; i++)
            {
                _boxes.Add(new Box { BoxNumber = i });
                prisonerNumbersToPlace.Add(i);
            }

            Shuffle(prisonerNumbersToPlace);

            for (int i = 0; i < numberOfPrisoners; i++)
            {
                _boxes[i].PrisonerNumberInside = prisonerNumbersToPlace[i];
            }

            // Reset simulation state
            _currentPrisonerNumber = 1;
            _attemptsMadeByCurrentPrisoner = 0;
            _lastOpenedBoxNumberValue = 0; // Indicates nothing opened yet by current prisoner
            _currentPrisonerFoundHisNumber = false;
            _prisonersWhoFoundTheirNumber = new List<int>();
        }

        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public SimulationStepResult NextStep()
        {
            // Check for overall simulation end conditions first
            if (_prisonersWhoFoundTheirNumber.Count == _numberOfPrisoners)
            {
                return new SimulationStepResult { Status = SimulationStatus.AllPrisonersSucceeded };
            }
            // This condition means all prisoners have had their turns, but not all succeeded.
            if (_currentPrisonerNumber > _numberOfPrisoners) 
            {
                return new SimulationStepResult { Status = SimulationStatus.Failed };
            }

            // Logic for current prisoner's turn
            if (_currentPrisonerFoundHisNumber || _attemptsMadeByCurrentPrisoner >= _maxAttempts)
            {
                if (!_currentPrisonerFoundHisNumber && _attemptsMadeByCurrentPrisoner >= _maxAttempts)
                {
                    // Current prisoner failed to find their number within attempts
                    return new SimulationStepResult 
                    { 
                        Status = SimulationStatus.Failed, 
                        PrisonerNumber = _currentPrisonerNumber,
                        BoxNumberOpened = _lastOpenedBoxNumberValue, // This was the last box they opened
                        AttemptsMade = _attemptsMadeByCurrentPrisoner,
                        FoundNumberInBox = false 
                    };
                }

                // Transition to the next prisoner
                _currentPrisonerNumber++;
                _attemptsMadeByCurrentPrisoner = 0;
                _currentPrisonerFoundHisNumber = false;
                _lastOpenedBoxNumberValue = 0;

                // Check if all prisoners have completed their turns after incrementing
                if (_currentPrisonerNumber > _numberOfPrisoners)
                {
                    // If all found their numbers, it's a success (this should have been caught by the first check, but as a safeguard)
                    if (_prisonersWhoFoundTheirNumber.Count == _numberOfPrisoners)
                    {
                        return new SimulationStepResult { Status = SimulationStatus.AllPrisonersSucceeded };
                    }
                    else // Otherwise, it's a failure
                    {
                        return new SimulationStepResult { Status = SimulationStatus.Failed };
                    }
                }
                
                // Announce next prisoner's turn
                return new SimulationStepResult 
                { 
                    Status = SimulationStatus.NextPrisonerTurn, 
                    PrisonerNumber = _currentPrisonerNumber,
                    AttemptsMade = _attemptsMadeByCurrentPrisoner
                };
            }

            // Execute an attempt for the current prisoner
            _attemptsMadeByCurrentPrisoner++;
            int boxToOpenNumber;

            if (_attemptsMadeByCurrentPrisoner == 1)
            {
                boxToOpenNumber = _currentPrisonerNumber; // First attempt: open own box number
            }
            else
            {
                // Subsequent attempts: open box number found in the previously opened box
                // Ensure _lastOpenedBoxNumberValue is valid (e.g., > 0)
                if (_lastOpenedBoxNumberValue <= 0 || _lastOpenedBoxNumberValue > _numberOfPrisoners)
                {
                    // This case indicates an error in logic or state.
                    // For robustness, could treat as failure or throw exception.
                    // For now, let's assume it leads to failure for this prisoner.
                    return new SimulationStepResult { Status = SimulationStatus.Failed, PrisonerNumber = _currentPrisonerNumber };
                }
                boxToOpenNumber = _lastOpenedBoxNumberValue;
            }
            
            Box openedBox = _boxes.FirstOrDefault(b => b.BoxNumber == boxToOpenNumber);
            if (openedBox == null)
            {
                // Should not happen if box numbers are managed correctly.
                return new SimulationStepResult { Status = SimulationStatus.Failed, PrisonerNumber = _currentPrisonerNumber, AttemptsMade = _attemptsMadeByCurrentPrisoner };
            }

            _lastOpenedBoxNumberValue = openedBox!.PrisonerNumberInside; // Store for next step, asserted non-null
            bool foundNumberInThisBox = openedBox!.PrisonerNumberInside == _currentPrisonerNumber; // asserted non-null

            if (foundNumberInThisBox)
            {
                _currentPrisonerFoundHisNumber = true;
                if (!_prisonersWhoFoundTheirNumber.Contains(_currentPrisonerNumber))
                {
                    _prisonersWhoFoundTheirNumber.Add(_currentPrisonerNumber);
                }

                // Check if this was the last prisoner and all succeeded
                if (_prisonersWhoFoundTheirNumber.Count == _numberOfPrisoners)
                {
                     return new SimulationStepResult 
                    { 
                        Status = SimulationStatus.AllPrisonersSucceeded, // Changed from FoundNumber
                        PrisonerNumber = _currentPrisonerNumber, 
                        BoxNumberOpened = openedBox!.BoxNumber, // asserted non-null
                        ValueInBox = openedBox!.PrisonerNumberInside, // asserted non-null
                        AttemptsMade = _attemptsMadeByCurrentPrisoner, 
                        FoundNumberInBox = true 
                    };
                }

                return new SimulationStepResult 
                { 
                    Status = SimulationStatus.FoundNumber, 
                    PrisonerNumber = _currentPrisonerNumber, 
                    BoxNumberOpened = openedBox!.BoxNumber,  // asserted non-null
                    ValueInBox = openedBox!.PrisonerNumberInside, // asserted non-null
                    AttemptsMade = _attemptsMadeByCurrentPrisoner, 
                    FoundNumberInBox = true 
                };
            }
            else
            {
                // If attempts are maxed out and number not found, this prisoner (and simulation) fails
                if (_attemptsMadeByCurrentPrisoner >= _maxAttempts)
                {
                    return new SimulationStepResult 
                    { 
                        Status = SimulationStatus.Failed, 
                        PrisonerNumber = _currentPrisonerNumber, 
                        BoxNumberOpened = openedBox!.BoxNumber, // asserted non-null
                        ValueInBox = openedBox!.PrisonerNumberInside, // asserted non-null
                        AttemptsMade = _attemptsMadeByCurrentPrisoner, 
                        FoundNumberInBox = false 
                    };
                }

                return new SimulationStepResult 
                { 
                    Status = SimulationStatus.Searching, 
                    PrisonerNumber = _currentPrisonerNumber, 
                    BoxNumberOpened = openedBox!.BoxNumber, // asserted non-null
                    ValueInBox = openedBox!.PrisonerNumberInside, // asserted non-null
                    AttemptsMade = _attemptsMadeByCurrentPrisoner, 
                    FoundNumberInBox = false 
                };
            }
        }

        public IReadOnlyList<IBox> GetBoxes() // Тип изменен на IReadOnlyList<IBox>
        {
            // Это должно работать благодаря ковариантности.
            // ReadOnlyCollection<Box> (возвращаемый AsReadOnly()) реализует IReadOnlyList<Box>,
            // и IReadOnlyList<Box> ковариантен к IReadOnlyList<IBox>.
            return _boxes.AsReadOnly(); // Removed ?. as _boxes is now initialized
        }

        // Method for testing purposes
        internal void SetBoxesForTesting(List<Box> boxes, int numberOfPrisoners, int maxAttempts)
        {
            _boxes = boxes;
            _numberOfPrisoners = numberOfPrisoners;
            _maxAttempts = maxAttempts;
            
            // Reset simulation state
            _currentPrisonerNumber = 1;
            _attemptsMadeByCurrentPrisoner = 0;
            _lastOpenedBoxNumberValue = 0; 
            _currentPrisonerFoundHisNumber = false;
            _prisonersWhoFoundTheirNumber = new List<int>();
        }
    }
}
