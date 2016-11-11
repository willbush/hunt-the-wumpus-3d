using System;
using HuntTheWumpus3d.Infrastructure;
using Microsoft.Xna.Framework;

namespace HuntTheWumpus3d.Entities
{
    public class Wumpus : DeadlyHazard
    {
        private static readonly Logger Logger = Logger.Instance;
        private static readonly Random Rand = new Random();
        private readonly int _initialRoomNumber;

        public Wumpus(int roomNumber) : base(roomNumber)
        {
            _initialRoomNumber = roomNumber;
            EntityColor = Color.Green;
        }

        private bool IsAwake { get; set; }

        /// <summary>
        ///     Updates the state of the wumpus.
        /// </summary>
        public void Update(Map map)
        {
            if (!IsAwake && map.Player.RoomNumber == RoomNumber)
            {
                Logger.Write(Message.WumpusBump);
                IsAwake = true;
            }
            if (!IsAwake && map.Player.CrookedArrowCount < map.Player.MaxArrows)
                IsAwake = true;

            if (IsAwake)
                Move(map);
        }

        /// <summary>
        ///     Moves the wumpus with a 75% chance.
        /// </summary>
        private void Move(Map map)
        {
            if (!WumpusFeelsLikeMoving()) return;

            RoomNumber = map.GetSafeRoomNextTo(RoomNumber);
            if (map.IsCheatMode)
                Logger.Write($"Wumpus moved to {RoomNumber}");
        }

        private static bool WumpusFeelsLikeMoving()
        {
            return Rand.Next(1, 101) > 25; // 75% chance wumpus feels like moving.
        }

        public override void PrintLocation()
        {
            Logger.Write($"Wumpus in room {RoomNumber}");
        }

        public override void PrintHazardWarning()
        {
            Logger.Write(Message.WumpusWarning);
        }

        /// <summary>
        ///     Determine the game end state given the player's current room number.
        /// </summary>
        /// <param name="playerRoomNumber">current player room number</param>
        /// <returns>end state</returns>
        public override EndState DetermineEndState(int playerRoomNumber)
        {
            EndState endState;

            if (IsAwake && playerRoomNumber == RoomNumber)
            {
                IsDiscovered = playerRoomNumber == RoomNumber;
                endState = new EndState(true, $"{Message.WumpusGotYou}\n{Message.LoseMessage}");
            }
            else
            {
                endState = new EndState();
            }
            return endState;
        }

        /// <summary>
        ///     Resets Wumpus to initial state.
        /// </summary>
        public override void Reset()
        {
            RoomNumber = _initialRoomNumber;
            IsAwake = false;
            IsDiscovered = false;
        }
    }
}