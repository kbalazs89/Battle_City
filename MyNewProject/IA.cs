using BattleCity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNewProject
{
    public class IA : IBattleClient
    {
        public string TeamName => throw new NotImplementedException();

        public string ImageFile => throw new NotImplementedException();

        public BattleCommand GameTick(IEnumerable<GameItem> items, byte remainingSeconds, byte playerId)
        {
            throw new NotImplementedException();
        }

        public void GameWasReset()
        {
            throw new NotImplementedException();
        }

        public void ShutDown()
        {
            throw new NotImplementedException();
        }
    }
}
