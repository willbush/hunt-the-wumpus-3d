namespace HuntTheWumpus3d.Entities
{
    public abstract class Hazard : GameEntity
    {
        protected Hazard(int roomNumber) : base(roomNumber)
        {
        }

        public bool IsDiscovered { get; set; }

        public abstract void PrintHazardWarning();

        public override void Reset()
        {
            IsDiscovered = false;
        }
    }
}