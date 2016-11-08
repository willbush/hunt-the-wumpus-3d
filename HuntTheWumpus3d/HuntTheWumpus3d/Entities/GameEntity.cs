namespace HuntTheWumpus3d.Entities
{
    public abstract class GameEntity
    {
        protected GameEntity(int roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public int RoomNumber { get; protected set; }

        public abstract void PrintLocation();
    }
}