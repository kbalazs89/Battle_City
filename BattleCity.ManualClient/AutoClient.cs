using BattleCity.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BattleCity.ManualClient
{
    public class AutoClient : IBattleClient
    {
        enum StageType { Nothing, Left, Right }
        StageType stage = StageType.Nothing;
        static Random R = new Random();


        public AutoClient()
        {
        }

        public string ImageFile
        {
            get
            {
                return "2.jpg";
            }
        }

        public string TeamName
        {
            get
            {
                return "Zs_Auto";
            }
        }


        public BattleCommand GameTick(IEnumerable<GameItem> items, byte remainingSeconds, byte playerId)
        {
            Tank ourTank = items.SingleOrDefault(x => x.OwnerId == playerId && x is Tank) as Tank;

            UdpCommand.Action myAction = UdpCommand.Action.NOTHING;
            byte myParameter = 0;

            if (ourTank.Rotation != 0 && ourTank.Rotation != 180) // bad angle => fix rotation
            {
                stage = StageType.Nothing;
                myAction = UdpCommand.Action.SETROTATION;
            }
            else
            {
                if (ourTank.Speed == 0) // we are not moving
                {
                    switch (stage)
                    {
                        case StageType.Right: // we were moving right in the past, so rotate left
                            myAction = UdpCommand.Action.SETROTATION;
                            myParameter = 18;
                            stage = StageType.Nothing;
                            break;
                        case StageType.Left: // we were moving left in the past, so rotate right
                            myAction = UdpCommand.Action.SETROTATION;
                            myParameter = 0;
                            stage = StageType.Nothing;
                            break;
                        case StageType.Nothing: // we are after a turn, so move forward
                            myAction = UdpCommand.Action.SETSPEED;
                            myParameter = 150;
                            stage = ourTank.Rotation == 0 ? StageType.Right : StageType.Left;
                            break;
                    }
                }
                else
                {
                    ourTank.Speed = 0; // No effect on the real tank
                    if (R.Next(0, 20) == 0)
                    {

                        if (ourTank.BulletNum > 0)
                            myAction = UdpCommand.Action.SHOOTBULLET;
                        else if (ourTank.RocketNum > 0)
                            myAction = UdpCommand.Action.SHOOTROCKET;
                    }
                }
            }


            BattleCommand cmd = new BattleCommand() { Action = myAction, ActionParameter = myParameter };

            return cmd;
        }

        public void GameWasReset()
        {
            // We Do Nothing
        }

        public void ShutDown()
        {
        }
    }
}
