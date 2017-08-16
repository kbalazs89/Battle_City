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
    public class ManualClient : IBattleClient
    {
        UdpCommand.Action myAction = UdpCommand.Action.NOTHING;
        byte myParameter = 0;

        byte speed = 100;
        byte rotation = 0;

        int idx = 0;
        Window w;
        ListBox lb;
        TaskScheduler GUIScheduler;

        public ManualClient()
        {
            GUIScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            w = new Window();
            
            w.Top = 0;
            w.Left = 1050;
            w.Width = 300;
            w.Height = 500;
            w.Title = "Zs Manual Client";

            lb = new ListBox();

            w.Content = lb;
            w.Show();
            w.KeyDown += W_KeyDown;
        }

        private void W_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    speed = 100;
                    myParameter = speed;
                    myAction = UdpCommand.Action.SETSPEED;
                    break;
                case Key.W:
                    speed += 10;
                    if (speed > 200) speed = 200;
                    myParameter = speed;
                    myAction = UdpCommand.Action.SETSPEED;
                    break;
                case Key.S:
                    speed -= 10;
                    if (speed > 200) speed = 0;
                    myParameter = speed;
                    myAction = UdpCommand.Action.SETSPEED;
                    break;
                case Key.Right:
                    rotation = 0;
                    myParameter = rotation;
                    myAction = UdpCommand.Action.SETROTATION;
                    break;
                case Key.Left:
                    rotation = 18;
                    myParameter = rotation;
                    myAction = UdpCommand.Action.SETROTATION;
                    break;
                case Key.Up:
                    rotation = 27;
                    myParameter = rotation;
                    myAction = UdpCommand.Action.SETROTATION;
                    break;
                case Key.Down:
                    rotation = 9;
                    myParameter = rotation;
                    myAction = UdpCommand.Action.SETROTATION;
                    break;
                case Key.A:
                    rotation--;
                    if (rotation > 35) rotation = 0;
                    myParameter = rotation;
                    myAction = UdpCommand.Action.SETROTATION;
                    break;
                case Key.D:
                    rotation++;
                    if (rotation > 35) rotation = 35;
                    myParameter = rotation;
                    myAction = UdpCommand.Action.SETROTATION;
                    break;
                case Key.F:
                    myParameter = 0;
                    myAction = UdpCommand.Action.SHOOTBULLET;
                    break;
                case Key.R:
                    myParameter = 0;
                    myAction = UdpCommand.Action.SHOOTROCKET;
                    break;
            }
        }

        public string ImageFile
        {
            get
            {
                return "anon.png";
            }
        }

        public string TeamName
        {
            get
            {
                return "Zs_Manual";
            }
        }

        public BattleCommand GameTick(IEnumerable<GameItem> items, byte remainingSeconds, byte playerId)
        {
            BattleCommand cmd = new BattleCommand() { Action=myAction, ActionParameter=myParameter };
            myAction = UdpCommand.Action.NOTHING;
            myParameter = 0;

            Tank ourTank = items.SingleOrDefault(x => x.OwnerId == playerId && x is Tank) as Tank;

            idx++;
            new Task(() =>
            {
                w.Title = "LOOP "+idx+" AT "+ourTank.X+";"+ourTank.Y;
                string msg = "ACTION " + cmd.Action + " PARAM " + cmd.ActionParameter;
                if (!msg.Contains("NOTHING")) lb.Items.Insert(0, msg);
            }).Start(GUIScheduler);

            return cmd;
        }

        public void GameWasReset()
        {
            // We Do Nothing
        }

        public void ShutDown()
        {
            w.Close();
        }
    }
}
