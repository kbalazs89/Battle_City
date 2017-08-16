using BattleCity.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BattleCity.ServerGUI
{
    class ViewModel : Bindable
    {
        List<GameItem> gameItems;

        public List<GameItem> GameItems
        {
            get { return gameItems; }
            set { SetProperty(ref gameItems, value); }
        }

        string gameStatus;

        public string GameStatus
        {
            get { return gameStatus; }
            set { SetProperty(ref gameStatus, value); }
        }

        IEnumerable<GameClient> clients;

        public IEnumerable<GameClient> Clients
        {
            get { return clients; }
            set { SetProperty(ref clients, value); }
        }
    }
}
