using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BattleCity.Common
{
    public class UdpCommand
    {
        public enum Action
        {
            NOTHING = 0,
            SETSPEED = 1,
            SETROTATION = 2,
            SHOOTBULLET = 3,
            SHOOTROCKET = 4
        }

        public EndPoint EndPoint;
        public byte[] Buffer;

        public UdpCommand()
        {
            EndPoint = new IPEndPoint(IPAddress.Any, 1111);
            Buffer = new byte[GameItem.BUFFERSIZE];
        }
    }
}
