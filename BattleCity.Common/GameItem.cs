using BattleCity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BattleCity.Common
{
    public class GameItem
    {
        public const int BUFFERSIZE = 16;
        public const int ITEMSIZE = 35;

        static int itemIdGenerator = 0;

        public static int ItemsToConsole(IEnumerable<GameItem> allItems)
        {
            int maxY = -1;
            foreach (var akt in allItems)
            {
                if (akt.ItemId > 0)
                {
                    int y = akt.Y.MyRound();
                    if (y > maxY) maxY = y;
                    Console.SetCursorPosition(akt.X.MyRound(), y);
                    Console.Write((char)akt.ItemChar);
                }
            }
            return maxY;
        }

        // Rotation: 0 .. 350
        public const int LUTmax = 350;
        public static Vector[] VectorLUT = new Vector[LUTmax + 1];

        static GameItem()
        {
            for (int i = 0; i <= LUTmax; i += 10)
            {
                VectorLUT[i] = new Vector(1, 0).RotateRadians(Math.PI * i / 180);
            }
        }

        private GameItem()
        {
            ItemRect = new Rect(0, 0, ITEMSIZE, ITEMSIZE);
        }

        public GameItem Clone()
        {
            return this.MemberwiseClone() as GameItem;
        }

        public GameItem(char newchar, ItemTypes type, byte newowner, double newx, double newy)
            : this()
        {
            Interlocked.Increment(ref itemIdGenerator);
            ItemId = itemIdGenerator;
            ItemType = type;
            OwnerId = newowner;
            X = newx;
            Y = newy;
            ItemChar = newchar;
        }

        /// <summary>
        /// Unique identifier of the GameItem
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Item type (deprecated)
        /// </summary>
        public ItemTypes ItemType { get; set; }

        /// <summary>
        /// Owner identifier
        /// </summary>
        public byte OwnerId { get; set; }

        /// <summary>
        /// Character representation of the game item
        /// </summary>
        public char ItemChar { get; set; }

        /// <summary>
        /// X coordinate
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Rotation, in degrees. 0 = facing right
        /// </summary>
        public short Rotation { get; set; }

        /// <summary>
        /// Speed, in units/timespan Can be [-100..100]
        /// </summary>
        public sbyte Speed { get; set; }

        /// <summary>
        /// Number of bullets remaining
        /// </summary>
        public byte BulletNum { get; set; }

        /// <summary>
        /// Number of rockets remainig
        /// </summary>
        public byte RocketNum { get; set; }

        /// <summary>
        /// Byte buffer for UDP communication
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        ///  Rotation in radians
        /// </summary>
        public double RotationRad
        {
            get { return Math.PI * Rotation / 180; }
        }

        /// <summary>
        /// The rectangle of the item - stored version, if the item won't move
        /// </summary>
        public Rect ItemRect { get; set; }

        protected bool isFixedPosition;

        /// <summary>
        /// The rectangle of the item - universal version
        /// </summary>
        public Rect RealRect
        {
            get
            {
                if (isFixedPosition)
                {
                    return ItemRect;
                }
                else
                {
                    return new Rect(ItemRect.Left + X * ITEMSIZE, ItemRect.Top + Y * ITEMSIZE,
                        ItemRect.Width, ItemRect.Height);
                }
            }
        }

        static Dictionary<char, Brush> Brushes = new Dictionary<char, Brush>();
        static Dictionary<char, string> OverrideBrushes = new Dictionary<char, string>();

        public static void OverrideBrush(byte playerId, string file)
        {
            char c = (char)('a' + playerId - 1);
            if (OverrideBrushes.ContainsKey(c))
            {
                OverrideBrushes.Remove(c);
            }

            if (file != null)
            {
                OverrideBrushes.Add(c, file);
            }

            if (Brushes.ContainsKey(c))
            {
                Brushes.Remove(c);
            }
        }

        public static void AddBrush(char itemChar, string file)
        {
            string fname = @"..\..\..\Images\";

            if (OverrideBrushes.ContainsKey(itemChar))
            {
                fname += OverrideBrushes[itemChar];
            }
            else
            {
                fname += file;
            }

            ImageBrush ib = new ImageBrush(new BitmapImage(new Uri(fname, UriKind.Relative)));
            if (!Brushes.ContainsKey(itemChar))
            {
                Brushes.Add(itemChar, ib);
            }
            else
            {
                Brushes[itemChar] = ib;
            }
        }

        /// <summary>
        /// The brush of the game item
        /// </summary>
        public Brush ItemBrush 
        {
            get
            {
                if (!Brushes.ContainsKey(ItemChar))
                {
                    string file = String.Empty;
                    switch (ItemChar)
                    {
                        case 'V': file = "weakwall_V.png"; break;
                        case 'W': file = "wall_W.png"; break;
                        case 'O': file = "crate_O.png"; break;
                        case 'B': file = "bullet_B.png"; break;
                        case 'R': file = "rocket_R.png"; break;
                        default: file = "tank.png"; break;
                    }
                    AddBrush(ItemChar, file);
                }
                return Brushes[ItemChar];
            }
        }

        public bool CollidesWith(GameItem other)
        {
            return this.RealRect.IntersectsWith(other.RealRect);
        }

        public void ConvertToBuffer()
        {
            if (Buffer == null)
            {
                Buffer = new byte[GameItem.BUFFERSIZE];
                Buffer[0] = (byte)(ItemId >> 24);
                Buffer[1] = (byte)(ItemId >> 16);
                Buffer[2] = (byte)(ItemId >> 8);
                Buffer[3] = (byte)ItemId;

                Buffer[4] = (byte)ItemType;
                Buffer[5] = (byte)ItemChar;
            }

            Buffer[6] = OwnerId;
            Buffer[7] = (byte)X;
            Buffer[8] = (byte)((int)(X * 100) % 100);
            Buffer[9] = (byte)Y;
            Buffer[10] = (byte)((int)(Y * 100) % 100);
            Buffer[11] = (byte)(Rotation >> 8);
            Buffer[12] = (byte)Rotation;
            Buffer[13] = (byte)(Speed + 100);
            Buffer[14] = BulletNum;
            Buffer[15] = RocketNum;
        }

        public GameItem(byte[] buffer)
            : this()
        {
            this.ItemId = buffer[3] + (buffer[2] << 8) + (buffer[1] << 16) + (buffer[0] << 24);
            this.ItemType = (ItemTypes)buffer[4];
            this.ItemChar = (char)buffer[5];
            this.OwnerId = buffer[6];
            this.X = buffer[7] + (double)buffer[8] / 100;
            this.Y = buffer[9] + (double)buffer[10] / 100;
            this.Rotation = (short)(buffer[12] + (buffer[11] << 8));
            this.Speed = (sbyte)(buffer[13] - 100);
            this.BulletNum = buffer[14];
            this.RocketNum = buffer[15];
        }

        public void MoveItem()
        {
            if (Speed != 0)
            {
                Vector rot = VectorLUT[Rotation];
                X += rot.X * C.MAP_MAXSPEED * Speed / 100;
                Y += rot.Y * C.MAP_MAXSPEED * Speed / 100;

                if (this is Tank)
                {
                    (this as Tank).Distance += Math.Abs(rot.Length * C.MAP_MAXSPEED * Speed / 100);
                }
            }
        }
    }

    public enum ItemTypes
    {
        WALL = 1,
        WEAKWALL = 2,
        TANK = 3,
        BULLET = 4,
        ROCKET = 5,
        AMMOCRATE = 6
    }

    public class Wall : GameItem
    {
        public Wall(double newx, double newy) :
            base('W', ItemTypes.WALL, 0, newx, newy)
        {
            ItemRect = RealRect;
            isFixedPosition = true;
        }
    }
    public class WeakWall : GameItem
    {
        public WeakWall(double newx, double newy) :
            base('V', ItemTypes.WEAKWALL, 0, newx, newy)
        {
            ItemRect = RealRect;
            isFixedPosition = true;
        }
    }

    public class Tank : GameItem
    {
        static Random R = new Random();

        /// <summary>
        /// Number of times the tank was shot
        /// </summary>
        public int TankWasShot { get; set; }

        /// <summary>
        /// Number of achieved points
        /// </summary>
        public int TankScore { get; set; }

        /// <summary>
        /// Distance covered
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Last good location - X coordinate
        /// </summary>
        public double LastGoodX { get; set; }

        /// <summary>
        /// Last good location - Y coordinate
        /// </summary>
        public double LastGoodY { get; set; }

        public Tank(char c, double newx, double newy, byte owner) :
            base(c, ItemTypes.TANK, owner, newx, newy)
        {
            ItemRect = new Rect(7, 7, ITEMSIZE - 15, ITEMSIZE - 15);

            BulletNum = C.TANK_STARTAMMO;
            RocketNum = C.TANK_STARTAMMO;

            LastGoodX = newx;
            LastGoodY = newy;

            Rotation = (short)(R.Next(0, 36) * 10);
        }
    }

    public class Bullet : GameItem
    {
        public Bullet(double newx, double newy, byte owner, short rot) :
            base('B', ItemTypes.BULLET, owner, newx, newy)
        {
            ItemRect = new Rect(10, 10, ITEMSIZE - 20, ITEMSIZE - 20);

            Speed = C.GetClient(owner).BulletSpeed;
            Rotation = rot;
        }
    }
    public class Rocket : GameItem
    {
        public Rocket(double newx, double newy, byte owner, short rot) :
            base('R', ItemTypes.ROCKET, owner, newx, newy)
        {
            ItemRect = new Rect(10, 10, ITEMSIZE - 20, ITEMSIZE - 20);

            Speed = C.GetClient(owner).RocketSpeed;
            Rotation = rot;
        }
    }

    public class AmmoCrate : GameItem
    {
        public AmmoCrate(double newx, double newy) :
            base('O', ItemTypes.AMMOCRATE, 0, newx, newy)
        {
            ItemRect = new Rect(5, 5, ITEMSIZE - 10, ITEMSIZE - 10);
            ItemRect = RealRect;
            isFixedPosition = true;
        }
    }

}
