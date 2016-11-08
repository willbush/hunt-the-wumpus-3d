using HuntTheWumpus3d.Infrastructure;

namespace HuntTheWumpus3d.Entities
{
    public abstract class DeadlyHazard : Hazard
    {
        protected DeadlyHazard(int roomNumber) : base(roomNumber)
        {
        }

        public abstract EndState DetermineEndState(int playerRoomNumber);
    }
}