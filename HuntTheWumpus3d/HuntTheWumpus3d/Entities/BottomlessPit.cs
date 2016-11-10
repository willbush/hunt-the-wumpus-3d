using HuntTheWumpus3d.Infrastructure;
using Microsoft.Xna.Framework;

namespace HuntTheWumpus3d.Entities
{
    public class BottomlessPit : DeadlyHazard
    {
        private static readonly Logger Logger = Logger.Instance;

        public BottomlessPit(int roomNumber) : base(roomNumber)
        {
            EntityColor = Color.Black;
        }

        public override void PrintLocation()
        {
            Logger.Write($"Bottomless pit in room {RoomNumber}");
        }

        public override void PrintHazardWarning()
        {
            Logger.Write(Message.PitWarning);
        }

        public override EndState DetermineEndState(int playerRoomNumber)
        {
            EndState endState;

            if (playerRoomNumber == RoomNumber)
            {
                endState = new EndState(true, $"{Message.FellInPit}\n{Message.LoseMessage}");
                IsDiscovered = true;
            }
            else
            {
                endState = new EndState();
            }
            return endState;
        }
    }
}