using BattleCity.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BattleCity.ServerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel VM;
        BCServer server;
        bool isRunning;

        private void LoadDLLs(string dir)
        {
            string[] plugins = Directory.GetFiles(dir, "*.dll");

            foreach (string akt in plugins)
            {
                string path = System.IO.Path.GetFullPath(akt);
                Assembly assembly = Assembly.LoadFile(path);
                foreach (Type aktType in assembly.GetTypes())
                {
                    if (aktType.GetInterface("IBattleClient") != null)
                    {
                        IBattleClient instance = Activator.CreateInstance(aktType) as IBattleClient;

                        GameClient client = new GameClient(instance.TeamName, instance.ImageFile, 0, true, true, true, true, true);
                        client.DLL = instance;
                        C.Players.Add(client);
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            server = new BCServer();

            server.Map.ResetMap += Map_ResetMap;

            VM = new ViewModel();
            DataContext = VM;

            DispatcherTimer vmTimer = new DispatcherTimer();
            vmTimer.Interval = TimeSpan.FromMilliseconds(C.SERVER_UIDELAY);
            vmTimer.Tick += vmTimer_Tick;
            vmTimer.Start();

            DispatcherTimer resultsTimer = new DispatcherTimer();
            resultsTimer.Interval = TimeSpan.FromSeconds(C.SERVER_AUTOSAVE);
            resultsTimer.Tick += resultsTimer_Tick;
            resultsTimer.Start();


            C.Players = new List<GameClient>();
            LoadDLLs(C.PLUGIN_DIR);
            LoadDLLs(Environment.CurrentDirectory);

            isRunning = true;
            vmTimer_Tick(null, null);
            isRunning = false;


        }

        private void Map_ResetMap(object sender, EventArgs e)
        {
            vmTimer_Tick(null, null);
            isRunning = false;
        }

        void resultsTimer_Tick(object sender, EventArgs e)
        {
            string fname = "result_auto_"+DateTime.Now.ToString("s").Replace(":", "") + ".tsv";
            server.Map.SaveToFile(fname);
        }

        void vmTimer_Tick(object sender, EventArgs e)
        {
            VM.GameItems = server.Map.GetItemList();
            VM.GameStatus = String.Format("{3} CLIENTS; {4} ITEMS; {5} SECONDS LEFT | SPEED {0} bytes, {1} bytes/sec, {2} bytes/sec/client", UdpBase.NumBytesTotal, UdpBase.Speed, 0, 0, server.Map.Items.Count, server.Map.RemainingTime);
            if (isRunning)
            {
                VM.Clients = null;
                VM.Clients = C.GetClients().OrderByDescending(x => x);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            server.Stop();
        }

        private void RunGame_Click(object sender, RoutedEventArgs e)
        {
            var clients = C.GetClients();
            foreach (var akt in ClientsLB.SelectedItems.Cast<GameClient>())
            {
                byte nextPlayerId = server.Map.PlayerIds.
                                    Except(clients.Where(x => x.Tank != null).Select(x => x.Tank.OwnerId)).
                                    OrderBy(x => Guid.NewGuid()).
                                    FirstOrDefault();
                Tank selectedTank = server.Map.GetTank(nextPlayerId);
                GameItem.OverrideBrush(nextPlayerId, akt.DLL.ImageFile);

                akt.Tank = selectedTank;
            }

            isRunning = true;
        }
    }
}
