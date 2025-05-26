namespace PrisonersDilemma.Core.Models
{
    public class Box : IBox
    {
        public int BoxNumber { get; set; } // Оставляем set для инициализации
        public int PrisonerNumberInside { get; set; } // Оставляем set для инициализации
    }
}
