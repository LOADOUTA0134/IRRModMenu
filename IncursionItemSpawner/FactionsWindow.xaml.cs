using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
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

            if (factionName == "VLF") level = VLFLevel.Value;
            else if (factionName == "UICS") level = UICSLevel.Value;
            else if (factionName == "IGC") level = IGCLevel.Value;

            ExecuteFactionCommand(factionName, (int)level);
        }

        private void VLFLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int sliderValue = (int)VLFLevel.Value;

            int level = sliderValue;

            if (level > 4)
                level = 4;

            VLFLevelText.Text = "Level " + level.ToString();

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

            if (level == 1) level = 24;
            else if (level == 2) level = 49;
            else if (level == 3) level = 74;
            else if (level == 4) level = 100;
            else level = 0;

            string command = $"setFactionReputation {level} {faction}";

            SendKeys.SendWait("{F10}");
            System.Threading.Thread.Sleep(250);
            SendKeys.SendWait(command);
            SendKeys.SendWait("{ENTER}");
        }


        private void UICSLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int sliderValue = (int)UICSLevel.Value;

            int level = sliderValue;

            if (level > 4)
                level = 4;

            UICSLevelText.Text = "Level " + level.ToString();
        }

        private void IGCLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int sliderValue = (int)IGCLevel.Value;

            int level = sliderValue;

            if (level > 4)
                level = 4;

            IGCLevelText.Text = "Level " + level.ToString();
        }
    }
}