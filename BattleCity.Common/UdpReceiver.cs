using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCity.Common
{
    public class UdpReceiver : UdpBase
    {
        public bool IsRunning { get; set; }
        public event EventHandler<UdpCommand> CommandReceived;
        public int AvailableBytes
        {
            get
            {
                return sock.Available;
            }
        }

        Socket sock;
        IPEndPoint endPoint;
        Thread recvThread;

        public UdpReceiver(int receiverPort)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.ReceiveBufferSize = 2048000;
            endPoint = new IPEndPoint(IPAddress.Any, receiverPort);
            sock.Bind(endPoint);
            
            recvThread = new Thread(ReceiveData);
            recvThread.Start();

            IsRunning = true;
        }

        public void Close()
        {
            IsRunning = false;
            sock.Close();
        }

        private void ReceiveData()
        {
            while (IsRunning)
            {
                if (sock.Available >= GameItem.BUFFERSIZE)
                {
                    int numPackets = sock.Available / GameItem.BUFFERSIZE;
                    for (int i = 0; i < numPackets; i++)
                    {
                        UdpCommand packet = new UdpCommand();

                        int numOfBytes = 0;
                        try
                        {
                            numOfBytes = sock.ReceiveFrom(packet.Buffer, GameItem.BUFFERSIZE, SocketFlags.None, ref packet.EndPoint);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText("error.log",
                                "------------\r\n" +
                                DateTime.Now + "\r\n" +
                                ex.ToString() + "\r\n\r\n\r\n");
                        }
                        if (CommandReceived != null && numOfBytes==GameItem.BUFFERSIZE)
                        {
                            CommandReceived(this, packet);
                        }

                        UpdateSpeed(numOfBytes);
                    }
                }
                System.Threading.Thread.Sleep(C.UDP_RECVDELAY);
            }
        }

    }
}
