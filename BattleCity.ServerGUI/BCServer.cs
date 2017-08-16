using BattleCity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BattleCity.ServerGUI
{

    class BCServer
    {
        public GameMap Map { get; private set; }
        object lockObj = new object();

        public BCServer()
        {
            Map = new GameMap(C.MAP_FILE, Map_SendMap, Map_ResetMap);
        }

        void Map_ResetMap(object sender, EventArgs e)
        {
            var clients = C.GetClients();
            if (clients == null) return;

            foreach (var akt in clients)
            {
                if (akt.Tank != null)
                {
                    GameItem.OverrideBrush(akt.Tank.OwnerId, null);
                }
                akt.Tank = null;
                akt.DLL.GameWasReset();
            }
        }

        void Map_SendMap(object sender, SendMapEventArgs e)
        {
            var clients = C.GetClients();
            if (clients == null) return;

            List<GameItem> mapItems = new List<GameItem>();
            foreach (var akt in e.Items)
            {
                mapItems.Add(akt.Clone());
            }

            foreach (var akt in clients)
            {
                if (!akt.IsBusy && akt.Tank!=null)
                {
                    akt.IsBusy = true;
                    Task.Factory.StartNew(() =>
                    {
                        var action = akt.DLL.GameTick(mapItems, Map.RemainingTime, akt.Tank.OwnerId);
                        Map.ProcessCommand(action.Action, akt.Tank.OwnerId, action.ActionParameter);
                        akt.IsBusy = false;
                    });
                }
            }
        }

        public void Stop()
        {
            Map.IsRunning = false;
            foreach (var akt in C.GetClients())
            {
                akt.DLL.ShutDown();
            }
        }
    }
}
