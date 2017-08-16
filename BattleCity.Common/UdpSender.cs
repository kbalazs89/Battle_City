using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCity.Common
{
    public class UdpSender : UdpBase
    {
        public IPEndPoint EndPoint { get; private set; }

        byte[] pkgBuffer = new byte[GameItem.BUFFERSIZE];
        Socket sock;

        public byte PlayerId { get; private set; }

        public UdpSender(string clientIp, int clientPort, byte newplayer)
        {
            PlayerId = newplayer;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SendBufferSize = 1024000;
            EndPoint = new IPEndPoint(IPAddress.Parse(clientIp), clientPort);
            if (EndPoint.Address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new Exception("ipv6?");
            }

            for (int i = 0; i < 4; i++) pkgBuffer[i] = 255;
            pkgBuffer[4] = 0;
            pkgBuffer[5] = PlayerId;
        }

        public void SendState(List<GameItem> items, byte remainingSeconds)
        {
            pkgBuffer[6] = remainingSeconds;
            SendArray(pkgBuffer);
            int cnt = items.Count;
            for (int i = 0; i < cnt; i++)
            {
                if (items[i].Buffer != null)
                {
                    SendArray(items[i].Buffer);
                }
            }
        }

        public void SendArray(byte[] buffer)
        {
            UpdateSpeed(buffer.Length);
            sock.SendTo(buffer, buffer.Length, SocketFlags.None, EndPoint);
        }


    }
}
