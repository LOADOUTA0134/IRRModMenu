using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Wpf.Ui.Controls;

namespace IncursionItemSpawner
{
    public partial class FactionsWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public FactionsWindow()
        {
            InitializeComponent();
        }

        private void SetFaction_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Wpf.Ui.Controls.Button;
            string factionName = button.Tag.ToString();
            double level = 1;

            // Welches Level wurde gewählt?
            if (factionName == "VLF") level = VLFLevel.Value;
            else if (factionName == "UCIS") level = UCISLevel.Value;
            else if (factionName == "IGC") level = IGCLevel.Value;

            ExecuteFactionCommand(factionName, (int)level);
        }

        private void ExecuteFactionCommand(string faction, int level)
        {
            var processes = Process.GetProcessesByName("Test_C-Win64-Shipping");
            if (processes.Length == 0)
            {
                System.Windows.MessageBox.Show("Game not running!");
                return;
            }

            SetForegroundWindow(processes[0].MainWindowHandle);

            string command = $"setfactionlevel {faction} {level}";

            SendKeys.SendWait("{F10}");
            System.Threading.Thread.Sleep(250);
            SendKeys.SendWait(command);
            SendKeys.SendWait("{ENTER}");
        }
    }
}