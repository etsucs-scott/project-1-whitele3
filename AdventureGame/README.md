How to build & run:
1.
git clone the repository
cd AdventureGame.cli

2.
Open .sln file -> select build solution

3.
Press F5 in VS with the console project as the startup project.




Controls:
WASD or Arrow Keys.

Maze Display Format:
# = wall
. = empty space
@ = player
M = monster
W = weapon
P = potion
E = exit


Movement:
Moves one tile at a time.
Moving into a wall or off the grid prints error message.

Items:
Weapons are added to inventory; only highest bonus applies.
Potions heal 20 hp up to 150 max hp.


Monsters:
Entering a monster tile initiates a battle.
-Player attacks first
- Monster counterattacks
-Repeat until one reaches 0hp

Defeated monsters disappear from maze.



Win Condition:
Reach the exit (E)

Lose Condition:
If player dies.


UML:

