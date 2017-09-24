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
        enum StageType { Nothing, Left, Right, Up, Down }
        StageType stage = StageType.Nothing;
        static Random R = new Random();

        public string TeamName
        {
            get
            {
                return "A Team";
            }
        }

        public string ImageFile
        {
            get
            {
                return "1.jpg";
            }
        }

        public BattleCommand GameTick(IEnumerable<GameItem> items, byte remainingSeconds, byte playerId)
        {
            Tank ourTank = items.SingleOrDefault(x => x.OwnerId == playerId && x is Tank) as Tank;
            ourTank.BulletNum = 250;
            ourTank.RocketNum = 250;

            UdpCommand.Action myAction = UdpCommand.Action.NOTHING;
            byte myParameter = 0;

           if (ourTank.Rotation != 0 && ourTank.Rotation != 180 && ourTank.Rotation != 90 && ourTank.Rotation != 270) // bad angle => fix rotation
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
                        case StageType.Right: 
                            myAction = UdpCommand.Action.SETROTATION;
                            myParameter = 0;
                            stage = StageType.Nothing;
                            break;
                        case StageType.Left: 
                            myAction = UdpCommand.Action.SETROTATION;
                            myParameter = 18;
                            stage = StageType.Nothing;
                            break;
                        case StageType.Up: 
                            myAction = UdpCommand.Action.SETROTATION;
                            myParameter = 27;
                            stage = StageType.Nothing;
                            break;
                        case StageType.Down: 
                            myAction = UdpCommand.Action.SETROTATION;
                            myParameter = 9;
                            stage = StageType.Nothing;
                            break;

                        case StageType.Nothing: // we are after a turn, so move forward
                            myAction = UdpCommand.Action.SETSPEED;
                            myParameter = 150;
                            int m = R.Next(0, 2);
                            switch (ourTank.Rotation)
                            {
                                case 0:
                                    stage = m == 0 ? StageType.Up : StageType.Down;
                                    break;
                                case 90:
                                    stage = m == 0 ? StageType.Right : StageType.Left;
                                    break;
                                case 180:
                                    stage = m == 0 ? StageType.Down : StageType.Up;
                                    break;
                                case 270:
                                    stage = m == 0 ? StageType.Left : StageType.Right;
                                    break;
                            }
                            //stage = ourTank.Rotation == 0 ? StageType.Left : StageType.Right;
                            break;
                    }
                }
                else
                {
                    ourTank.Speed = 0; // No effect on the real tank
                    int v = R.Next(0, 20);
                    if (v == 0)
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
            //majd kell
        }

        public void ShutDown()
        {
            //majd kell
        }
    }
}
