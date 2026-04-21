using System.Windows;
using Wpf.Ui.Controls;

namespace IncursionItemSpawner
{
    public partial class SafehouseWindow : Window
    {
        public SafehouseWindow()
        {
            InitializeComponent();
        }

        private void Upgrade_Click(object sender, RoutedEventArgs e)
        {
            // Nutze den WPF UI Button Typ
            var button = sender as Wpf.Ui.Controls.Button;
            if (button == null || button.Tag == null) return;

            string upgradeName = button.Tag.ToString();
            int level = 1;

            // WICHTIG: Die Namen im switch müssen exakt mit den Tags im XAML übereinstimmen
            switch (upgradeName.ToLower())
            {
                case "intel": level = (int)IntelSlider.Value; break;
                case "stash": level = (int)StashSlider.Value; break;
                case "generator": level = (int)GeneratorSlider.Value; break;
                case "gunsmith": level = (int)GunsmithSlider.Value; break;
                case "lightning": level = (int)LightningSlider.Value; break;
                case "repair": level = (int)RepairSlider.Value; break; // Tippfehler "tepair" korrigiert
                case "terminal": level = (int)TerminalSlider.Value; break;
                case "structure": level = (int)StructureSlider.Value; break;
                case "shootingrange": level = (int)ShootingRangeSlider.Value; break;
            }

            // Nutze deinen neuen GameCommandRunner für mehr Stabilität (Auto-Start des Spiels etc.)
            string command = $"setHideoutUnitLevel {upgradeName} {level}";
            GameCommandRunner.TrySendCommand(command);
        }
    }
}