using HuntTheWumpus3d.Infrastructure;

namespace HuntTheWumpus3d.Entities
{
    public class SuperBats : Hazard
    {
        private static readonly Logger Logger = Logger.Instance;

        public SuperBats(int roomNumber) : base(roomNumber)
        {
        }

        public override void PrintLocation()
        {
            Logger.Write($"SuperBats in room {RoomNumber}");
        }

        public override void PrintHazardWarning()
        {
            Logger.Write(Message.BatWarning);
        }

        /// <summary>
        ///     Moves player to a random location on the map if they enter the
        ///     same room as a super bat.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>true if the bat snatched the player into another room</returns>
        public bool TrySnatch(Player player)
        {
            if (player.RoomNumber != RoomNumber) return false;

            Logger.Write(Message.BatSnatch);
            player.Move(Map.GetAnyRandomRoomNumber());
            return true;
        }
    }
}