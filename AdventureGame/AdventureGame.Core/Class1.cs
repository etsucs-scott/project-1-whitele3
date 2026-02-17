using System.Dynamic;

namespace AdventureGame.Core
{
    /// <summary> 
    /// Represents a character in the game that can attack and take damage. 
    /// </summary>
    public interface ICharacter
    {
        int Health { get; }
        void Attack(ICharacter target);
        void TakeDamage(int amount);
    }

    /// <summary> 
    ///  The player character controlled by the user. 
    ///  Tracks health, inventory, and weapon bonuses. 
    ///  </summary>
    public class Player : ICharacter
    {
        //player starting health is 100. can go to 150
        //base damage is 10 plus modifiers
        private const int MaxHealth = 150;
        private const int BaseDamage = 10;
        public int Health { get; private set; } = 100;
        public List<Item> Inventory { get; } = new List<Item>();

        public void Attack(ICharacter target)
        {
            int damage = BaseDamage + GetBestWeaponBonus();
            target.TakeDamage(damage);
        }
        public void TakeDamage(int amount)
        {
            Health = Math.Max(0, Health - amount);
        }
        public void AddItem(Item item)
        {
            Inventory.Add(item);

            if (item is Potion potion)
            {
                Health = Math.Min(MaxHealth, Health + potion.HealAmount);
            }
        }

        private int GetBestWeaponBonus()
        {
            return Inventory
                .OfType<Weapon>()
                .Select(w => w.AttackBonus)
                .DefaultIfEmpty(0)
                .Max();
        }
    }

    /// <summary> 
    ///  A monster encountered in the maze. 
    ///  Has randomized health and deals damage. 
    ///  </summary>
    public class Monster : ICharacter
    {
        private static readonly Random rng = new Random();
        public int Health { get; private set; }
        public int AttackPower { get; }

        public Monster()
        {
            Health = rng.Next(30, 51);
            AttackPower = 10;
        }
        public void Attack(ICharacter target)
        {
            target.TakeDamage(AttackPower);
        }
        public void TakeDamage(int amount)
        {
            Health = Math.Max(0, Health - amount);
        }
    }

    /// <summary> 
    ///  Base class for all items that can appear in the maze. 
    ///  </summary>
    public abstract class Item
    {
        public string Name { get;}
        public string PickupMessage { get; }

        protected Item(string name, string pickupMessage)
        {
            Name = name;
            PickupMessage = pickupMessage;
        }
    }

    /// <summary> 
    ///  A weapon that increases the player's attack damage.  
    ///  </summary>
    public class Weapon : Item
    {
        public int AttackBonus { get; }

        public Weapon(string name, int attackBonus)
            : base(name, $"You picked up a {name}!")
        {
            AttackBonus = attackBonus;
        }
    }

    /// <summary> 
    ///  A potion that heals the player immediately upon pickup. 
    ///  </summary>
    public class Potion : Item
    {
        public int HealAmount { get; } = 20;

        public Potion(string name)
            : base(name, $"You drink the {name} and get healthier!")
        {

        }
    }


    /// <summary> 
    ///  Represents a single tile in the maze. 
    ///  Can contain a wall, item, monster, or the exit. 
    ///  </summary>
    public class Tile
    {
        public bool IsWall { get; set; }
        public Item? Item { get; set; }
        public Monster? Monster { get; set; }
        public bool IsExit { get; set; }
    }


    /// <summary> 
    /// A 2D grid of tiles that forms the maze. 
    /// </summary>
    public class Maze
    {
        public Tile[,] Tiles { get; }
        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);

        public Maze(int width, int height)
        {
            Tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Tiles[x,y] = new Tile();
        }

        public Tile GetTile(int x, int y) => Tiles[x, y];
    }

    public enum Direction
    {
        Up, Down, Right, Left
    }
    public enum GameStatus
    {
        PlayerWon,
        PlayerDead,
        InProgress
    }


    /// <summary> 
    ///  Contains the result of, 
    ///  messages, battle outcomes, and item pickups. 
    ///  </summary>
    public class MoveResult
    {
        public bool Moved { get; set; }
        public string Message {  get; set; } = string.Empty;
        public bool BattleOccured { get; set; }
        public bool ItemPickedUp { get; set; }
        public bool ReachedExit { get; set; }
        public bool PlayerDied { get; set; }
    }


    /// <summary> 
    /// Core game logic: movement, battles, item handling, and maze generation. 
    /// This class contains all rules for the game. 
    /// </summary>
    public class GameEngine
    {
        private readonly Random _rng;
        public Maze Maze { get; }
        public Player Player { get; }
        public GameStatus Status { get; private set; } = GameStatus.InProgress;

        public int PlayerX { get; private set; }
        public int PlayerY { get; private set; }

        public GameEngine(int width = 10, int height = 10, Random? rng = null)
        {
            if (width < 10 || height < 10)
                throw new ArgumentException("Maze must be 10x10 or bigger.");

            _rng = rng ?? new Random();
            Maze = new Maze(width, height);
            Player = new Player();
            PlayerX = 0;
            PlayerY = 0;

            GenerateMaze();
        }

        public MoveResult Move(Direction direction)
        {
            if (Status != GameStatus.InProgress)
            {
                return new MoveResult
                {
                    Moved = false,
                    Message = "Game is over."
                };
            }
            var (newX, newY) = GetTargetPosition(direction);

            if (!IsInsideMaze(newX, newY))
            {
                return new MoveResult
                {
                    Moved = false,
                    Message = "You cannot move outside the maze."
                };
            }

            var targetTile = Maze.GetTile(newX, newY);

            if (targetTile.IsWall)
            {
                return new MoveResult
                {
                    Moved = false,
                    Message = "A wall is blocking your path."
                };
            }

            PlayerX = newX;
            PlayerY = newY;


            if(targetTile.Monster != null)
            {
                var battleMessage = ResolveBattle(targetTile.Monster);
                bool playerDied = Status == GameStatus.PlayerDead;

                if (!playerDied)
                {
                    targetTile.Monster = null;
                }

                return new MoveResult
                {
                    Moved = true,
                    BattleOccured = true,
                    PlayerDied = playerDied,
                    Message = battleMessage
                };
            }

            if (targetTile.Item != null)
            {
                var item = targetTile.Item;
                Player.AddItem(item);
                targetTile.Item = null;

                return new MoveResult
                {
                    Moved = true,
                    ItemPickedUp = true,
                    Message = item.PickupMessage
                };

            }

            if (targetTile.IsExit)
            {
                Status = GameStatus.PlayerWon;
                return new MoveResult
                {
                    Moved = true,
                    ReachedExit = true,
                    Message = "You found the exit!"
                };
            }

            return new MoveResult
            {
                Moved = true,
                Message = "You moved to an empty tile."
            };
        }

        private(int x, int y) GetTargetPosition(Direction direction)
        {
            int x = PlayerX;
            int y = PlayerY;

            switch (direction)
            {
                case Direction.Left: x--; break;
                case Direction.Right: x++; break;
                case Direction.Up: y--; break;
                case Direction.Down: y++; break;
            }
            return (x, y);
        }
        private bool IsInsideMaze(int x, int y)
        {
            return x >= 0 && x < Maze.Width && y >= 0 && y< Maze.Height;
        }

        private string ResolveBattle(Monster monster)
        {
            var log = new List<string>();

            while(Player.Health > 0 && monster.Health > 0)
            {

                Player.Attack(monster);
                log.Add($"You hurt the monster. Monster HP: {monster.Health}");

                if(monster.Health <= 0)
                {
                    log.Add("You defeated the monster! Great job!");
                    break;
                } 

                monster.Attack(Player);
                log.Add($"The monster damages you. Your HP: {Player.Health}");

                if(Player.Health <= 0)
                {
                    log.Add("You have died.");
                    Status = GameStatus.PlayerDead;
                    break;
                }
            }

            return string.Join(" ", log);
        }

        private void GenerateMaze()
        {

            int w = Maze.Width;
            int h = Maze.Height;


            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    Maze.Tiles[x, y].IsWall = false;

            int cx = 0;
            int cy = 0;
            while (cx < w - 1)
            {
                Maze.Tiles[cx,cy].IsWall = false;
                cx++;
            }
            while (cy < h - 1)
            {
                Maze.Tiles[cx,cy].IsWall = false;
                cy++;
            }


            Maze.Tiles[w -1, h -1].IsExit = true;

            for(int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if ((x == 0 && y == 0) || Maze.Tiles[x, y].IsExit)
                        continue;

                    bool onPath = (x <= w - 1 && y == 0) || (x == w - 1 && y <= h - 1);
                    if (onPath)
                        continue;

                    if (_rng.NextDouble() < 0.25)
                    {
                        Maze.Tiles[x,y].IsWall = true;
                    }
                }
            }

            for (int x = 0;x < w; x++)
            {
                for (int y = 0;y < h; y++)
                {
                    var tile = Maze.Tiles[x, y];

                    if (tile.IsWall || tile.IsExit || (x == 0 && y == 0))
                        continue;

                    double roll = _rng.NextDouble();

                    if (roll < 0.10)
                    {
                        tile.Monster = new Monster();
                    }
                    else if (roll < 0.15)
                    {
                        tile.Item = new Potion("Health Potion");
                    }
                    else if (roll < 0.20)
                    {
                        int bonus = _rng.Next(1, 6);
                        tile.Item = new Weapon($"Sword +{bonus}", bonus);
                    }
                }
            }
        }
    }


}
