using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCity.Common
{
    public class GameClient : Bindable, IComparable<GameClient>
    {
        public IBattleClient DLL { get; set; }
        public bool IsBusy { get; set; } 

        public GameClient(string newname, string newimg, byte newid, bool bonusSpeed, bool bonusBulletRegen, bool bonusBulletSpeed, bool bonusRocketRegen, bool bonusRocketSpeed)
        {
            IsBusy = false;
            DLL = null;

            TeamName = newname;
            ImageFile = newimg;
            PlayerId = newid;
            SpeedMulti = bonusSpeed ? C.SPEED_BONUS : C.SPEED_NORMAL;
            BulletRegenerationSeconds = bonusBulletRegen ? C.BULLETREGEN_BONUS : C.BULLETREGEN_NORMAL;
            BulletSpeed = bonusBulletSpeed ? C.BULLETSPEED_BONUS : C.BULLETSPEED_NORMAL;
            RocketRegenerationSeconds = bonusRocketRegen ? C.ROCKETREGEN_BONUS : C.ROCKETREGEN_NORMAL;
            RocketSpeed = bonusRocketSpeed ? C.ROCKETSPEED_BONUS : C.ROCKETSPEED_NORMAL;
        }

        public string TeamName { get; set; }
        public string ImageFile { get; set; }
        public byte PlayerId { get; set; }

        public double SpeedMulti { get; set; }
        public int BulletRegenerationSeconds { get; set; }
        public sbyte BulletSpeed { get; set; }
        public int RocketRegenerationSeconds { get; set; }
        public sbyte RocketSpeed { get; set; }

        public string IP { get; set; }
        public int Port { get; set; }
        public Tank Tank { get; set; }

        public int CompareTo(GameClient other)
        {
            int res;

            if (this.Tank == null && other.Tank == null) return 0;
            if (this.Tank == null && other.Tank != null) return -1;
            if (this.Tank != null && other.Tank == null) return 1;

            res = this.Tank.TankScore.CompareTo(other.Tank.TankScore);
            if (res != 0) return res;
            res = other.Tank.TankWasShot.CompareTo(this.Tank.TankWasShot);
            if (res != 0) return res;
            return this.Tank.Distance.CompareTo(other.Tank.Distance);
        }
    }
}
