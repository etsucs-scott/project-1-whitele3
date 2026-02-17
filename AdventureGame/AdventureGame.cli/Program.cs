using AdventureGame.Core;



namespace AdventureGame.cli
{
    /// <summary>  
    /// Handles input, output, and rendering the maze. 
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
         
            var engine = new GameEngine(15,15);
            string lastMessage = "Welcome to the Adventure Game!";

            while (engine.Status == GameStatus.InProgress)
            {
                Console.Clear();

                Console.WriteLine(lastMessage);
                Console.WriteLine();

                DrawMaze(engine);

                Console.WriteLine();
                Console.WriteLine($"HP: {engine.Player.Health}");
                
                ConsoleKeyInfo key = Console.ReadKey(true);

                Direction? dir =key.Key switch
                {
                    ConsoleKey.W or ConsoleKey.UpArrow => Direction.Up,
                    ConsoleKey.S or ConsoleKey.DownArrow => Direction.Down,
                    ConsoleKey.A or ConsoleKey.LeftArrow => Direction.Left,
                    ConsoleKey.D or ConsoleKey.RightArrow => Direction.Right,
                    _ => null
                };

                if (dir == null)
                {
                    lastMessage = "Use WASD or arrow keys to move.";
                    continue;
                }

                var result = engine.Move(dir.Value);
                lastMessage = result.Message;
            }

            Console.Clear();
            Console.WriteLine(engine.Status == GameStatus.PlayerWon
                ? " You have escaped the maze!"
                : "You have died. Game over.");
        }

        /// <summary>  
        /// # = wall, . = empty, @ = player, M = monster, /// W = weapon, P = potion, E = exit. 
        /// </summary>
        static void DrawMaze(GameEngine engine)
        {
            for (int y = 0; y < engine.Maze.Height; y++)
            {
                for (int x = 0; x < engine.Maze.Width; x++)
                {
                    if (engine.PlayerX == x && engine.PlayerY == y)
                    {
                        Console.Write("@ ");
                        continue;
                    }

                    var tile = engine.Maze.Tiles[x, y];

                    if (tile.IsWall)
                        Console.Write("# ");
                    else if (tile.IsExit)
                        Console.Write("E ");
                    else if (tile.Monster != null)
                        Console.Write("M ");
                    else if (tile.Item is Weapon)
                        Console.Write("W ");
                    else if (tile.Item is Potion)
                        Console.Write("P ");
                    else
                        Console.Write(". ");
                }
                Console.WriteLine();
            }
        }
    }
}
