namespace HuntTheWumpus3d.Entities
{
    public abstract class Hazard : GameEntity
    {
        protected Hazard(int roomNumber) : base(roomNumber)
        {
        }

        public abstract void PrintHazardWarning();
    }
}