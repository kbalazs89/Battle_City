// TODO comm elcsúszás a szerveren?
// TODO comm elcsúszás esetén 4x0xFF -ek keresése
// TODO időnként nem indul el a kliens?

// TODO random kliens

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BattleCity.Common
{
    public static class C
    {
        public static List<GameClient> Players;

        public const bool MAP_ALLOWANON = false;
        public const string MAP_FILE = @"..\..\..\Maps\_open.txt";
        public const string PLUGIN_DIR = @"..\..\..\Plugins\";
        //public const string PLUGIN_DIR = @"n:\anon\programming\GeekDay7_BattleCity_DLL\CODE\BattleCity.ManualClient\bin\Debug\";

        public const byte MAP_GAMETIME = 180;
        public const int MAP_PLUSAMMOTIME = 10;
        public const int TANK_STARTAMMO = 3;
        public const int TANK_AMMOINCRATE = 3;

        public const double SPEED_NORMAL = 1;
        public const double SPEED_BONUS = 1.25;
        public const int BULLETREGEN_NORMAL = 60; //60;
        public const int BULLETREGEN_BONUS = 40;
        public const sbyte BULLETSPEED_NORMAL = 105;
        public const sbyte BULLETSPEED_BONUS = 125;
        public const int ROCKETREGEN_NORMAL = 60; //60;
        public const int ROCKETREGEN_BONUS = 40;
        public const sbyte ROCKETSPEED_NORMAL = 70;
        public const sbyte ROCKETSPEED_BONUS = 90;

        public const int MAP_SIZE = 20;
        public const int MAP_SPINDELAY = 50;
        public const double MAP_MAXSPEED = 0.2;

        public const int UDP_RECVDELAY = 20;
        public const int SERVER_UIDELAY = 200;
        public const int SERVER_AUTOSAVE = 30;
        public const int SERVER_UDPPORT = 11111;
        public const string SERVER_IP = "10.4.11.24";
        public const int CLIENT_UIDELAY = 50;

        static object lockObj = new object();
        static void LockAction(Action a)
        {
            lock (lockObj)
            {
                a();
            }
        }
        public static IEnumerable<GameClient> GetClients()
        {
            IEnumerable<GameClient> clients = null;
            LockAction(() => { clients = Players?.ToList(); });
            return clients;
        }
        public static GameClient GetClient(int playerId)
        {
            GameClient client = null;
            LockAction(() =>
            {
                client = Players.SingleOrDefault(x => x.Tank != null && x.Tank.OwnerId == playerId);
            });
            return client;
        }

    }
}
