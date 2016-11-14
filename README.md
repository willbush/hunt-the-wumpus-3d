<!-- markdown-toc start - Don't edit this section. Run M-x markdown-toc-generate-toc again -->

**Table of Contents**

- [Hunt the Wumpus 3D](#hunt-the-wumpus-3d)
    - [Introduction](#introduction)
    - [Controls](#controls)
        - [Keyboard Inputs](#keyboard-inputs)
        - [Mouse Inputs](#mouse-inputs)
    - [How to Play](#how-to-play)
        - [Game Rules](#game-rules)
        - [Moving](#moving)
        - [Hazards](#hazards)
        - [Warnings](#warnings)
        - [Shooting](#shooting)
        - [Winning the game:](#winning-the-game)
    - [Building From Source](#building-from-source)

<!-- markdown-toc end -->

# Hunt the Wumpus 3D

## Introduction

This is a 3D implementation of Gregory Yob's [Hunt the Wumpus](https://en.wikipedia.org/wiki/Hunt_the_Wumpus) text based game in 1972. This game was implemented using [monogame](http://www.monogame.net/).

## Controls

### Keyboard Inputs

- For each turn in the game the player can perform the following actions:
  - Shoot, Move, or Quit (Press S, M, or Q)
    - Simply pressing the button will trigger the action. There's no need to press enter.
  - When shooting the player is prompted to enter a space separated list of rooms.
    - Entering text is much like a console, blinking cursor, the ability to backspace, and needing to press (Enter) to submit your list of rooms.
    
  - Pressing the (Right Ctrl) button will toggle cheat mode on and off.
    - Cheat mode displays all the harzards on the map and prints their locations.

### Mouse Inputs

- The dodecahedron map normally rotates around. Actually, the camera rotates around the map. **Right clicking** the mouse will toggle this rotation on and off.

- Holding down the **left mouse button** allows you to control the rotation of the map. This honestly isn't implement very well, but it's better than nothing. If you **right click while holdling the left mouse button** you can freeze the map's position to where you want.

- Mousing over a room (sphere) display's it's room number next to your cursor.

## How to Play

The game has 20 rooms which are represented as spheres in a 3 dimensional dodecahedron. The default color of a room with nothing in the room is **white**.

You as the player:
- You are represented as an oscillating color between **blue** and whatever else is in the room.
- have a bow with 5 crooked arrows
- when you shoot a crooked arrow, the rooms it traverses oscillate between **red** and whatever else is in the room.
- You gain information about the map's layout via clues from adjacent rooms.

The game has 3 types hazards which are assigned to random room numbers:
- 2 super bats
  - Super bats are represented as **purple** spheres
  - They Can pick up and transport the player to another random room
  - The Wumpus can go into this room, but won't be picked up because it's too heavy!
- 2 bottomless pits
  - Bottomless pits are represented as **black** spheres
  - The player falls to their death if one of these rooms is entered.
  - In the original game the Wumpus can go in these rooms because it has "suckers" on its feet. However, I the way I implemented it has the Wumpus avoiding going into rooms with Bottomless pits.
- 1 Wumpus
  - The Wumpus is represented as a **green** sphere
  - The Wumups is the monster you have to slay to win the game. It sleeps until you wake it up by shooting an arrow somewhere on the map or enter the room the Wumpus is in and bump it.

### Game Rules

The game is turn based. On each turn, the game first moves the Wumpus (if awake), then checks neighboring rooms to the player for hazards, and displays a warning message if any exist. The player is then asked for their action, which can be move, shoot, or quit (press M, S, or Q).

### Moving

For moving, the game displays a list of the room numbers of connected rooms, then asks for the room number of the destination room. If the entered room is not connected to the current room, the game displays, "Not possible" and re-asks for the destination room.
On a move, the game moves the player to the new room, then checks for whether the player has hit a hazard in that room. 

### Hazards

A superbat attack causes the player to be randomly placed in a new room. When this happens, the game displays "Zap--Super Bat snatch! Elsewhereville for you!" This is the equivalent to a normal move, in that hazard checks are performed after determining the new room.

A bottomless pit kills the player, and ends the game. When this happens, the game displays, "YYYIIIIEEEE . . . fell in a pit"

The Wumpus begins the game asleep. If the player shoots an arrow anywhere in the map, the Wumpus wakes up. For every turn after the Wumpus is awoken (including the turn that woke him up), the Wumpus has a 75% chance of moving to another room.

The first time the player enters the room containing the Wumpus, the Wumpus wakes up, and the game displays, "... Ooops! Bumped a Wumpus." Once you bump the Wumpus the player will die if the Wumpus decides to not leave the room of the player with a 25% chance.

### Warnings

- If superbats are in an adjoining room, display the warning message, “Bats nearby”.
- If a bottomless pit is in an adjoining room, display the warning message, "I feel a draft".
- If the Wumpus is in an adjoining room, display the warning message, "I smell a Wumpus".
Shooting

### Shooting

If the player chooses to shoot, the game then asks for up to five rooms the arrow should visit. The game asks the player to enters a space separated list of rooms, but does not check if each room correctly follows another. The game does, however, check to ensure the arrow does not go from room A to B and back to A again. If the player attempts to enter an A-B-A path, the game replies, "Arrows aren't that crooked - try another room" and the player must re-enter the list of rooms. If  the entered list of rooms are not correctly connected from one room to the next starting with the player's room, then it's path is determined at randomly (while still preventing a "too crooked" path).

If the arrow enters the room containing the Wumpus, the game displays, "Aha! You got the Wumpus!" (and the player wins the game). If the arrow enters the room containing the player, the game displays, "Ouch! Arrow got you!" If the arrow goes through its entire path and does not hit the Wumpus (or the player), display "Missed!"
The player only has 5 arrows, and loses one each time an arrow is shot. The game ends when the player runs out of arrows.

### Winning the game:

When the player wins the game, display, "Hee hee hee - the Wumpus'll getcha next time!!"

Once the game is won or lost the player is given the option to re-play the game using the same randomly chosen locations for the Wumpus, bats, and pits, or to have new random locations chosen for them.

## Building From Source

### Windows

1. Install [visual studio 2015](https://www.visualstudio.com/downloads/). This should also install the required required [.NET 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=48130)

2. install [monogame](http://www.monogame.net/downloads/) 3.5

