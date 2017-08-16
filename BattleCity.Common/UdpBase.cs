using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCity.Common
{
    public class UdpBase
    {
        public static float Speed;
        public static int NumBytesTotal;
        static int numBytesForSpeed;
        static Stopwatch stw = new Stopwatch();

        public UdpBase()
        {
            stw.Start();
        }

        protected void UpdateSpeed(int numBytes)
        {
            Interlocked.Add(ref NumBytesTotal, numBytes);
            Interlocked.Add(ref numBytesForSpeed, numBytes);
            if (stw.ElapsedMilliseconds > 1000)
            {
                Speed = 1000f * numBytesForSpeed / stw.ElapsedMilliseconds;
                stw.Reset();
                stw.Start();
                numBytesForSpeed = 0;
            }
        }
    }
}
