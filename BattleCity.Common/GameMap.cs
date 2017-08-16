
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BattleCity.Common
{
    public class SendMapEventArgs : EventArgs
    {
        public List<GameItem> Items { get; set; }
    }

    public class GameMap
    {
        public List<GameItem> Items { get; private set; }
        public bool IsRunning { get; set; }
        public List<byte> PlayerIds { get; private set; }
        public byte RemainingTime { get; private set; }

        public event EventHandler<SendMapEventArgs> SendMap;
        public event EventHandler ResetMap;

        static Random R = new Random();
        Thread spinThread;
        object lockObj = new object();
        string mapFileName;
        string[] mapFileLines;
        List<Wall> hardWalls;
        List<Tank> tanks;
        int round = 0;

        long plusAmmoTimer = 0;

        public void Reset()
        {
            if (IsRunning)
            {
                string fname = "result_final_" + DateTime.Now.ToString("s").Replace(":", "") + ".tsv";
                SaveToFile(fname);
            }
            if (ResetMap != null)
            {
                ResetMap(this, EventArgs.Empty);
            }

            IsRunning = false;
            Thread.Sleep(200);
            PlayerIds = new List<byte>();
            Items = new List<GameItem>();
            hardWalls = new List<Wall>();
            tanks = new List<Tank>();
            RemainingTime = C.MAP_GAMETIME;

            mapFileLines = File.ReadAllLines(mapFileName);
            for (int x = 0; x < C.MAP_SIZE; x++)
            {
                for (int y = 0; y < C.MAP_SIZE; y++)
                {
                    char c = mapFileLines[y][x];
                    switch (c)
                    {
                        case 'W':
                            Wall w = new Wall(x, y);
                            Items.Add(w);
                            hardWalls.Add(w);
                            break;
                        case 'V':
                            Items.Add(new WeakWall(x, y));
                            break;
                        case 'O':
                            Items.Add(new AmmoCrate(x, y));
                            break;
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                        case 'g':
                        case 'h':
                        case 'i':
                        case 'j':
                        case 'k':
                        case 'l':
                        case 'm':
                        case 'n':
                        case 'o':
                        case 'p':
                        case 'q':
                        case 'r':
                            byte playerId = (byte)(c - 'a' + 1);
                            PlayerIds.Add(playerId);
                            Tank t = new Tank(c, x, y, playerId);
                            Items.Add(t);
                            tanks.Add(t);
                            break;
                    }
                }
            }

            if (spinThread != null && spinThread.IsAlive)
            {
                spinThread.Abort();
            }

            round++;
            IsRunning = true;
            spinThread = new Thread(Spin);
            spinThread.Start();
        }

        public void SaveToFile(string fname)
        {
            string format = "{0}\t{1}\t{2}\t{3}\r\n";
            string content = String.Format("ROUND {0}\r\n", round);
            content += String.Format(format, "TEAM", "SCORE", "DEATHS", "DISTANCE");

            var clients = C.GetClients().OrderByDescending(x => x);
            foreach (var akt in clients)
            {
                if (akt.Tank == null)
                {
                    content += String.Format(format, akt.TeamName, null, null, null);
                }
                else
                {
                    content += String.Format(format, akt.TeamName, akt.Tank.TankScore, akt.Tank.TankWasShot, akt.Tank.Distance);
                }
            }
            File.WriteAllText(fname, content);
        }

        public Tank GetTank(int ownerId)
        {
            return tanks.SingleOrDefault(x => x.OwnerId == ownerId);
            /*
            Tank t = null;
            LockAction(() =>
            {
                t = Items.SingleOrDefault(x => x.ItemType == ItemTypes.TANK && x.OwnerId == ownerId) as Tank;
            });
            return t;
            */
        }

        public GameMap(string fname, EventHandler<SendMapEventArgs> sendMapHandler, EventHandler resetMapHandler)
        {
            SendMap += sendMapHandler;
            if (resetMapHandler != null)
            {
                ResetMap += resetMapHandler;
            }

            mapFileName = fname;
            Reset();

            System.Timers.Timer secTimer = new System.Timers.Timer();
            secTimer.Interval = 1000;
            secTimer.Elapsed += secTimer_Elapsed;
            secTimer.Start();
        }

        void secTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            plusAmmoTimer++;

            foreach (var akt in tanks)
            {
                GameClient c = C.GetClient(akt.OwnerId);
                int bulletNeeded = c==null ? C.BULLETREGEN_NORMAL : c.BulletRegenerationSeconds;
                int rocketNeeded = c==null ? C.ROCKETREGEN_NORMAL : c.RocketRegenerationSeconds;

                LockAction(() =>
                {
                    if (plusAmmoTimer % bulletNeeded == 0 && akt.BulletNum < 200)
                    {
                        akt.BulletNum++;
                    }
                    if (plusAmmoTimer % rocketNeeded == 0 && akt.RocketNum < 200)
                    {
                        akt.RocketNum++; ;
                    }
                });
            }

            if (plusAmmoTimer % C.MAP_PLUSAMMOTIME == 0)
            {
                int x, y;
                do
                {
                    x = R.Next(0, C.MAP_SIZE);
                    y = R.Next(0, C.MAP_SIZE);
                } while (mapFileLines[y][x] != ' ');

                LockAction(() =>
                {
                    Items.Add(new AmmoCrate(x, y));
                });
            }

            RemainingTime--;
            if (RemainingTime == 0)
            {
                Reset();
            }
        }

        public void ProcessCommand(UdpCommand.Action command, byte ownerId, byte param)
        {

            Tank t = GetTank(ownerId);
            if (t == null) return;
            switch (command)
            {
                case UdpCommand.Action.SETSPEED:
                    if (param <= 200) t.Speed = (sbyte)(C.GetClient(ownerId).SpeedMulti * (param - 100));
                    break;
                case UdpCommand.Action.SETROTATION:
                    if (param <= 35) t.Rotation = (short)(param * 10);
                    break;
                case UdpCommand.Action.SHOOTBULLET:
                    LockAction(() =>
                    {
                        if (t.BulletNum > 0)
                        {
                            Items.Add(new Bullet(t.X, t.Y, ownerId, t.Rotation));
                            t.BulletNum--;
                        }
                    });
                    break;
                case UdpCommand.Action.SHOOTROCKET:
                    LockAction(() =>
                    {
                        if (t.RocketNum > 0)
                        {
                            Items.Add(new Rocket(t.X, t.Y, ownerId, t.Rotation));
                            t.RocketNum--;
                        }
                    });
                    break;
            }
        }

        private void Spin()
        {
            List<AmmoCrate> ammoCrates = new List<AmmoCrate>();
            List<GameItem> bullets = new List<GameItem>();
            List<WeakWall> weakWalls = new List<WeakWall>();
            List<GameItem> collideableItems = new List<GameItem>();

            while (IsRunning)
            {
                bullets.Clear();
                weakWalls.Clear();
                collideableItems.Clear();
                ammoCrates.Clear();

                List<GameItem> myItems = null;
                LockAction(() =>
                {
                    myItems = Items.ToList();
                });

                foreach (GameItem item in myItems)
                {
                    if (item is Bullet || item is Rocket) bullets.Add(item);
                    if (item is WeakWall) weakWalls.Add(item as WeakWall);
                    if (item is AmmoCrate) ammoCrates.Add(item as AmmoCrate);

                    collideableItems.AddRange(weakWalls);
                    collideableItems.AddRange(tanks);
                    collideableItems.AddRange(hardWalls);
                }

                foreach (GameItem bullet in bullets)
                {
                    bullet.MoveItem();
                    bool collided = false;

                    foreach (Tank currentTank in tanks)
                    {
                        if (bullet.CollidesWith(currentTank) && bullet.OwnerId != currentTank.OwnerId)
                        {
                            collided = true;

                            currentTank.TankWasShot++;
                            currentTank.Speed = 0;

                            int x, y;
                            do
                            {
                                x = R.Next(0, C.MAP_SIZE);
                                y = R.Next(0, C.MAP_SIZE);
                                currentTank.X = x;
                                currentTank.Y = y;
                                currentTank.Rotation = (short)(R.Next(0, 36) * 10);
                            } while (mapFileLines[y][x] != ' ' || 
                                tanks.Exists(akt => akt != currentTank && akt.CollidesWith(currentTank)));

                            int score = bullet is Rocket ? 2 : 1;
                            tanks.Single(akt => akt.OwnerId == bullet.OwnerId).TankScore += score;
                        }
                    }

                    foreach (WeakWall wall in weakWalls)
                    {
                        if (!collided && bullet.CollidesWith(wall))
                        {
                            collided = true;
                            myItems.Remove(wall);
                            LockAction(() =>
                            {
                                Items.Remove(wall);
                            });
                        }
                    }

                    foreach (Wall wall in hardWalls)
                    {
                        if (!collided && bullet.CollidesWith(wall)) collided = true;
                    }

                    if (collided)
                    {
                        myItems.Remove(bullet);
                        LockAction(() =>
                        {
                            Items.Remove(bullet);
                        });
                    }
                }

                foreach (Tank tank in tanks)
                {
                    if (tank.Speed != 0)
                    {
                        bool collided = false;
                        foreach (GameItem item in collideableItems)
                        {
                            if (tank.CollidesWith(item) && tank.OwnerId != item.OwnerId)
                            {
                                collided = true;
                            }
                        }
                        if (collided)
                        {
                            tank.Speed = 0;
                            tank.X = tank.LastGoodX;
                            tank.Y = tank.LastGoodY;
                        }
                        else
                        {
                            tank.LastGoodX = tank.X;
                            tank.LastGoodY = tank.Y;
                        }
                    }

                    foreach (AmmoCrate crate in ammoCrates)
                    {
                        if (tank.CollidesWith(crate))
                        {
                            LockAction(() =>
                            {
                                if (tank.BulletNum<200) tank.BulletNum += C.TANK_AMMOINCRATE;
                                if (tank.RocketNum<200) tank.RocketNum += C.TANK_AMMOINCRATE;
                                Items.Remove(crate);
                            });
                        }
                    }
                }

                foreach (Tank tank in tanks)
                {
                    tank.MoveItem();
                }

                foreach (GameItem item in myItems)
                {
                    item.ConvertToBuffer();
                }

                if (SendMap != null)
                {
                    SendMap(this, new SendMapEventArgs() { Items = myItems });
                }
                Thread.Sleep(C.MAP_SPINDELAY);
            }
        }

        public List<GameItem> GetItemList()
        {
            List<GameItem> items = null;
            LockAction(() =>
            {
                items = Items.ToList();
            });
            return items;
        }

        void LockAction(Action a)
        {
            lock (lockObj)
            {
                a();
            }
        }
    }
}
