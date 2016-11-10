using Microsoft.Xna.Framework;

namespace HuntTheWumpus3d.Entities
{
    public abstract class GameEntity
    {
        protected GameEntity(int roomNumber)
        {
            RoomNumber = roomNumber;
        }

        public int RoomNumber { get; protected set; }
        public Color EntityColor { get; protected set; } = Color.Black;

        public abstract void PrintLocation();
        public abstract void Reset();
    }
}