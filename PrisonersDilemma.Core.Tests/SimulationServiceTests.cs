using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrisonersDilemma.Core.Models;
using PrisonersDilemma.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace PrisonersDilemma.Core.Tests
{
    [TestClass]
    public class SimulationServiceTests
    {
        [TestMethod]
        public void InitializeSimulation_CreatesCorrectNumberOfBoxesAndShuffledContents()
        {
            var service = new SimulationService();
            int numberOfPrisoners = 10;
            service.InitializeSimulation(numberOfPrisoners, 5);

            var boxes = service.GetBoxes();

            Assert.IsNotNull(boxes, "Boxes should not be null after initialization.");
            Assert.AreEqual(numberOfPrisoners, boxes.Count, "Incorrect number of boxes created.");

            var boxNumbers = boxes.Select(b => b.BoxNumber).ToList();
            var prisonerNumbersInside = boxes.Select(b => b.PrisonerNumberInside).ToList();

            // Check if all box numbers from 1 to numberOfPrisoners are present
            for (int i = 1; i <= numberOfPrisoners; i++)
            {
                Assert.IsTrue(boxNumbers.Contains(i), $"Box number {i} is missing.");
            }
            Assert.AreEqual(numberOfPrisoners, boxNumbers.Distinct().Count(), "Box numbers are not unique.");

            // Check if all prisoner numbers from 1 to numberOfPrisoners are present inside boxes
            for (int i = 1; i <= numberOfPrisoners; i++)
            {
                Assert.IsTrue(prisonerNumbersInside.Contains(i), $"Prisoner number {i} is missing from inside the boxes.");
            }
            Assert.AreEqual(numberOfPrisoners, prisonerNumbersInside.Distinct().Count(), "Prisoner numbers inside boxes are not unique.");
        }

        [TestMethod]
        public void NextStep_PrisonerFindsNumberInFirstBox()
        {
            var service = new SimulationService();
            var boxes = new List<Box> { new Box { BoxNumber = 1, PrisonerNumberInside = 1 } };
            service.SetBoxesForTesting(boxes, numberOfPrisoners: 1, maxAttempts: 1);

            // Step 1: Prisoner 1 opens Box 1 and finds their number
            var result = service.NextStep();
            Assert.AreEqual(SimulationStatus.FoundNumber, result.Status);
            Assert.AreEqual(1, result.PrisonerNumber);
            Assert.AreEqual(1, result.BoxNumberOpened);
            Assert.AreEqual(1, result.ValueInBox);
            Assert.AreEqual(1, result.AttemptsMade);
            Assert.IsTrue(result.FoundNumberInBox);

            // Step 2: Simulation should succeed as all (1) prisoners found their number
            result = service.NextStep();
            Assert.AreEqual(SimulationStatus.AllPrisonersSucceeded, result.Status);
        }

        [TestMethod]
        public void NextStep_PrisonerFindsNumberByFollowingChain()
        {
            var service = new SimulationService();
            // Box1 -> 2 (Prisoner 1 opens Box 1, finds 2)
            // Box2 -> 1 (Prisoner 1 then opens Box 2, finds 1 - success)
            var boxes = new List<Box>
            {
                new Box { BoxNumber = 1, PrisonerNumberInside = 2 },
                new Box { BoxNumber = 2, PrisonerNumberInside = 1 }
            };
            service.SetBoxesForTesting(boxes, numberOfPrisoners: 1, maxAttempts: 2); // Prisoner 1, 2 attempts

            // Step 1: Prisoner 1 opens Box 1 (their number), finds 2
            var result = service.NextStep();
            Assert.AreEqual(SimulationStatus.Searching, result.Status);
            Assert.AreEqual(1, result.PrisonerNumber);
            Assert.AreEqual(1, result.BoxNumberOpened);
            Assert.AreEqual(2, result.ValueInBox);
            Assert.AreEqual(1, result.AttemptsMade);
            Assert.IsFalse(result.FoundNumberInBox);

            // Step 2: Prisoner 1 opens Box 2 (value from previous box), finds 1
            result = service.NextStep();
            Assert.AreEqual(SimulationStatus.FoundNumber, result.Status);
            Assert.AreEqual(1, result.PrisonerNumber);
            Assert.AreEqual(2, result.BoxNumberOpened);
            Assert.AreEqual(1, result.ValueInBox);
            Assert.AreEqual(2, result.AttemptsMade);
            Assert.IsTrue(result.FoundNumberInBox);

            // Step 3: Simulation should succeed
            result = service.NextStep();
            Assert.AreEqual(SimulationStatus.AllPrisonersSucceeded, result.Status);
        }

        [TestMethod]
        public void NextStep_PrisonerFailsToFindNumber()
        {
            var service = new SimulationService();
            // Prisoner 1, max 1 attempt.
            // Box1 -> 2 (Prisoner 1 opens Box 1, finds 2. Attempt used, number not found)
            var boxes = new List<Box> { new Box { BoxNumber = 1, PrisonerNumberInside = 2 } };
            service.SetBoxesForTesting(boxes, numberOfPrisoners: 1, maxAttempts: 1);

            // Step 1: Prisoner 1 opens Box 1, finds 2.
            var result = service.NextStep();
            Assert.AreEqual(SimulationStatus.Searching, result.Status, "Status should be Searching after first attempt.");
            Assert.AreEqual(1, result.PrisonerNumber);
            Assert.AreEqual(1, result.BoxNumberOpened);
            Assert.AreEqual(2, result.ValueInBox);
            Assert.AreEqual(1, result.AttemptsMade);
            Assert.IsFalse(result.FoundNumberInBox);

            // Step 2: Attempts exhausted, prisoner failed.
            result = service.NextStep();
            Assert.AreEqual(SimulationStatus.Failed, result.Status, "Status should be Failed as attempts are exhausted.");
            Assert.AreEqual(1, result.PrisonerNumber, "PrisonerNumber should still be the one who failed.");
            Assert.AreEqual(1, result.AttemptsMade, "AttemptsMade should reflect the max attempts.");
        }

        [TestMethod]
        public void NextStep_AllPrisonersSucceed()
        {
            var service = new SimulationService();
            var boxes = new List<Box>
            {
                new Box { BoxNumber = 1, PrisonerNumberInside = 1 },
                new Box { BoxNumber = 2, PrisonerNumberInside = 2 }
            };
            service.SetBoxesForTesting(boxes, numberOfPrisoners: 2, maxAttempts: 1);

            // Prisoner 1
            var result = service.NextStep(); // P1 opens Box 1, finds 1
            Assert.AreEqual(SimulationStatus.FoundNumber, result.Status);
            Assert.AreEqual(1, result.PrisonerNumber);

            result = service.NextStep(); // Transition to P2
            Assert.AreEqual(SimulationStatus.NextPrisonerTurn, result.Status);
            Assert.AreEqual(2, result.PrisonerNumber);

            // Prisoner 2
            result = service.NextStep(); // P2 opens Box 2, finds 2
            Assert.AreEqual(SimulationStatus.FoundNumber, result.Status);
            Assert.AreEqual(2, result.PrisonerNumber);

            result = service.NextStep(); // Simulation succeeds
            Assert.AreEqual(SimulationStatus.AllPrisonersSucceeded, result.Status);
        }

        [TestMethod]
        public void NextStep_SimulationFailsIfOnePrisonerFails()
        {
            var service = new SimulationService();
            // P1 finds. P2 searches for 2, Box2 has 1. Max 1 attempt for P2.
            var boxes = new List<Box>
            {
                new Box { BoxNumber = 1, PrisonerNumberInside = 1 },
                new Box { BoxNumber = 2, PrisonerNumberInside = 1 } // P2 will open Box2, find 1, fail.
            };
            service.SetBoxesForTesting(boxes, numberOfPrisoners: 2, maxAttempts: 1);

            // Prisoner 1
            var result = service.NextStep(); // P1 opens Box 1, finds 1
            Assert.AreEqual(SimulationStatus.FoundNumber, result.Status);
            Assert.AreEqual(1, result.PrisonerNumber);

            result = service.NextStep(); // Transition to P2
            Assert.AreEqual(SimulationStatus.NextPrisonerTurn, result.Status);
            Assert.AreEqual(2, result.PrisonerNumber);

            // Prisoner 2
            result = service.NextStep(); // P2 opens Box 2 (their number), finds 1. Attempt 1.
            Assert.AreEqual(SimulationStatus.Searching, result.Status);
            Assert.AreEqual(2, result.PrisonerNumber);
            Assert.AreEqual(2, result.BoxNumberOpened);
            Assert.AreEqual(1, result.ValueInBox);
            Assert.AreEqual(1, result.AttemptsMade);

            // Prisoner 2 has no more attempts.
            result = service.NextStep();
            Assert.AreEqual(SimulationStatus.Failed, result.Status);
            Assert.AreEqual(2, result.PrisonerNumber); // Prisoner 2 is the one who failed
        }
    }
}
