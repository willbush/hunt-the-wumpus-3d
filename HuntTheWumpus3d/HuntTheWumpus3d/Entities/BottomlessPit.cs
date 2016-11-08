using System;
using HuntTheWumpus3d.Infrastructure;

namespace HuntTheWumpus3d.Entities
{
    public class BottomlessPit : DeadlyHazard
    {
        private static readonly Logger Logger = Logger.Instance;

        public BottomlessPit(int roomNumber) : base(roomNumber)
        {
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
            return playerRoomNumber == RoomNumber
                ? new EndState(true, $"{Message.FellInPit}\n{Message.LoseMessage}")
                : new EndState();
        }
    }
}