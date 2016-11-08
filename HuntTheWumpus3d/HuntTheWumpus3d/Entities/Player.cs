using System;
using System.Collections.Generic;
using System.Linq;
using HuntTheWumpus3d.Infrastructure;

namespace HuntTheWumpus3d.Entities
{
    public class Player : GameEntity
    {
        private const int MaxNumberOfArrows = 5;
        private static readonly Logger Logger = Logger.Instance;
        private readonly int _initialRoomNum;

        public Player(int roomNumber) : base(roomNumber)
        {
            _initialRoomNum = roomNumber;
        }

        public int MaxArrows { get; } = MaxNumberOfArrows;
        public int CrookedArrowCount { get; private set; } = MaxNumberOfArrows;

        /// <summary>
        ///     Requests where the player wants to move to, validates the input, and moves the player.
        /// </summary>
        public void Move()
        {
            Logger.Write("Where to? ");
            string response = Console.ReadLine();

            int adjacentRoom;
            while (!int.TryParse(response, out adjacentRoom) || !Map.IsAdjacent(RoomNumber, adjacentRoom))
            {
                Logger.Write("Not Possible - Where to? ");
                response = Console.ReadLine();
            }
            RoomNumber = adjacentRoom;
        }

        internal void Move(int roomNumber)
        {
            RoomNumber = roomNumber;
        }

        /// <summary>
        ///     Requests the player how many and what rooms the arrow should traverse.
        ///     The arrow traverses the traversable rooms or randomly selected adjacent ones and the
        ///     arrows traversal path is checked for a hit to determine end game state.
        /// </summary>
        /// <param name="wumpusRoomNumber">current wumpus room number</param>
        /// <returns>game end state result</returns>
        public EndState ShootArrow(int wumpusRoomNumber)
        {
            EndState endState;
            int numOfRooms = GetNumRoomsToTraverse();

            if (numOfRooms > 0)
            {
                CrookedArrowCount = CrookedArrowCount - 1;
                endState = ShootArrow(GetRoomsToTraverse(numOfRooms), wumpusRoomNumber);
            }
            else
            {
                Logger.Write("OK, suit yourself...");
                endState = new EndState();
            }
            return endState;
        }

        //Requests from the player how many rooms they want the arrow they're shooting to traverse.
        private static int GetNumRoomsToTraverse()
        {
            const int lowerBound = 0;
            const int upperBound = 5;
            int numOfRooms;
            string response;

            do
            {
                Logger.Write(Message.NumOfRoomsToShootPrompt);
                response = Console.ReadLine();
            } while (!int.TryParse(response, out numOfRooms) || numOfRooms < lowerBound || numOfRooms > upperBound);

            return numOfRooms;
        }

        // Traverses the given rooms or randomly selected adjacent rooms if the given rooms are not traversable.
        // Checks if the arrow hit the player, wumpus, or was a miss, and game state is set accordingly.
        private EndState ShootArrow(IReadOnlyCollection<int> roomsToTraverse, int wumpusRoomNum)
        {
            var endstate = Traverse(roomsToTraverse).Select(r => HitTarget(r, wumpusRoomNum))
                .FirstOrDefault(e => e.IsGameOver);

            if (endstate != null) return endstate;

            Logger.Write(Message.Missed);
            return CrookedArrowCount == 0
                ? new EndState(true, $"{Message.OutOfArrows}\n{Message.LoseMessage}")
                : new EndState();
        }

        private EndState HitTarget(int currentRoom, int wumpusRoomNum)
        {
            Logger.Write(currentRoom.ToString());
            EndState endState;
            if (RoomNumber == currentRoom)
            {
                endState = new EndState(true, $"{Message.ArrowGotYou}\n{Message.LoseMessage}");
            }
            else if (wumpusRoomNum == currentRoom)
            {
                endState = new EndState(true, Message.WinMessage);
            }
            else
            {
                endState = new EndState();
            }
            return endState;
        }

        // Attempts to traverse the requested rooms to traverse, but as soon as one
        // requested room is not adjacent to the current room, it starts traversing rooms randomly.
        private IEnumerable<int> Traverse(IReadOnlyCollection<int> roomsToTraverse)
        {
            int currentRoom = RoomNumber;

            ICollection<int> traversedRooms = roomsToTraverse.TakeWhile(
                nextRoom =>
                {
                    var adjacentRooms = Map.Rooms[currentRoom];
                    if (!adjacentRooms.Contains(nextRoom)) return false;
                    currentRoom = nextRoom;
                    return true;
                }).ToList();

            int numLeftToTraverse = roomsToTraverse.Count - traversedRooms.Count;
            RandomlyTraverse(traversedRooms, currentRoom, numLeftToTraverse);
            return traversedRooms;
        }

        // Adds to the given list of traversed rooms a randomly selected next adjacent room where
        // said selected room is not the previously traversed room (preventing U-turns).
        private static void RandomlyTraverse(
            ICollection<int> traversedRooms,
            int currentRoom,
            int numberToTraverse)
        {
            int previousRoom;

            // if no traversed rooms, randomly select an adjacent next room and set the previous to the room the player is in.
            if (!traversedRooms.Any())
            {
                var rooms = Map.Rooms[currentRoom];
                int firstRoom = rooms.ElementAt(new Random().Next(rooms.Count));

                previousRoom = currentRoom;
                currentRoom = firstRoom;
            }
            else
            {
                previousRoom = currentRoom;
            }

            // while we need more rooms, get a randomly selected adjacent room that is not the previously traversed room.
            for (var traversed = 0; traversed < numberToTraverse; ++traversed)
            {
                var rooms = Map.Rooms[currentRoom].Where(r => r != previousRoom).ToArray();
                int nextRoom = rooms.ElementAt(new Random().Next(rooms.Length));

                traversedRooms.Add(currentRoom);
                previousRoom = currentRoom;
                currentRoom = nextRoom;
            }
        }

        // Requests the player to give the list of rooms they want the arrow to traverse.
        private static List<int> GetRoomsToTraverse(int numOfRooms)
        {
            var rooms = new List<int>();
            var count = 1;

            while (count <= numOfRooms)
            {
                Logger.Write(Message.RoomNumPrompt);
                int roomNumber;
                if (!int.TryParse(Console.ReadLine(), out roomNumber) || roomNumber < 0 || roomNumber > Map.NumOfRooms)
                {
                    Logger.Write("Bad number - try again:");
                    continue;
                }
                if (IsTooCrooked(roomNumber, rooms))
                {
                    Logger.Write(Message.TooCrooked);
                }
                else
                {
                    rooms.Add(roomNumber);
                    count = count + 1;
                }
            }
            return rooms;
        }

        // A requested room number is too crooked for an arrow to go into when:
        // The requested room is the same as the previously requested room
        // (essentially asking the arrow to stay in the same room).
        // The requested room is the same as request before last.
        // (essentially asking for the arrow to make a U-turn).
        private static bool IsTooCrooked(int roomNumber, IReadOnlyList<int> rooms)
        {
            return (rooms.Count > 0 && rooms.Last() == roomNumber) ||
                   (rooms.Count > 1 && rooms[rooms.Count - 2] == roomNumber);
        }

        public override void PrintLocation()
        {
            Logger.Write($"You are in room {RoomNumber}");
            Map.PrintAdjacentRoomNumbers(RoomNumber);
        }

        /// <summary>
        ///     Resets player to initial state.
        /// </summary>
        public void Reset()
        {
            RoomNumber = _initialRoomNum;
            CrookedArrowCount = MaxArrows;
        }
    }
}