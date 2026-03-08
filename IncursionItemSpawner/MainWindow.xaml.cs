using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Windows.Forms; // Für SendKeys
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using System.Text.Json;
using System.Threading;

namespace IncursionItemSpawner
{
    // WICHTIG: Erbe von UiWindow für das Design
    public partial class MainWindow : Window
    {
        // Wir nennen die Liste MyItemList, damit dein CreateLoadout-Aufruf funktioniert
        public List<Item> MyItemList { get; set; } = new List<Item>();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(System.IntPtr hWnd);

        public MainWindow()
        {
            InitializeComponent();

            // Design-Initialisierung
            ApplicationThemeManager.ApplySystemTheme(updateAccent: true);
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica);

            LoadItems();

            // Kategorien für die ComboBox sammeln
            CategoryBox.Items.Add("All");
            foreach (var cat in MyItemList.Select(i => i.Category).Distinct())
                CategoryBox.Items.Add(cat);

            CategoryBox.SelectedIndex = 0;
            RefreshList();
        }

        void LoadItems()
        {
            try
            {
                // Sicherstellen, dass die Datei da ist
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "items.txt");
                if (!File.Exists(path)) return;

                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    if (parts.Length < 2) continue;

                    MyItemList.Add(new Item
                    {
                        Category = parts[0].Trim(),
                        Name = parts[1].Trim()
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading items: " + ex.Message);
            }
        }

        void RefreshList()
        {
            if (ItemList == null || SearchBox == null || CategoryBox == null)
                return;

            ItemList.Items.Clear();

            string search = SearchBox.Text?.ToLower() ?? "";
            string category = CategoryBox.SelectedItem?.ToString() ?? "All";

            foreach (var item in MyItemList)
            {
                if ((category == "All" || item.Category == category) &&
                    item.Name.ToLower().Contains(search))
                {
                    ItemList.Items.Add(item.Name);
                }
            }
        }

        private void ItemClicked(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ExecuteButton.IsEnabled = ItemList.SelectedItem != null;
        }

        private void SearchChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            RefreshList();
        }

        private void CategoryChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshList();
        }

        private string GetCommandForAction(string itemName, int amount)
        {
            // Suche das Item-Objekt in der Liste, um die Kategorie zu prüfen
            var itemObj = MyItemList.FirstOrDefault(i => i.Name == itemName);
            string command = (itemObj != null && itemObj.Category == "Resource") ? "addresource" : "additem";

            return $"{command} {itemName} {amount}";
        }

        private void ExecuteCommand(object sender, RoutedEventArgs e)
        {
            if (ItemList.SelectedItem == null) return;

            string itemName = ItemList.SelectedItem.ToString();
            if (!int.TryParse(AmountBox.Text, out int amount)) amount = 1;

            var processes = Process.GetProcessesByName("Test_C-Win64-Shipping");
            if (processes.Length == 0)
            {
                System.Windows.MessageBox.Show("Game not running");
                return;
            }

            SetForegroundWindow(processes[0].MainWindowHandle);

            string commandToSend = GetCommandForAction(itemName, amount);

            SendKeys.SendWait("{F10}");
            System.Threading.Thread.Sleep(250);
            SendKeys.SendWait(commandToSend);
            SendKeys.SendWait("{ENTER}");
        }

        private void CreateLoadout(object sender, RoutedEventArgs e)
        {
            // Jetzt wird MyItemList korrekt übergeben
            var loadoutDesigner = new LoadoutWindow(this.MyItemList);

            if (loadoutDesigner.ShowDialog() == true)
            {
                string finalCommand = loadoutDesigner.GeneratedCommand;

                // Sofort-Ausführung für das Loadout
                var processes = Process.GetProcessesByName("Test_C-Win64-Shipping");
                if (processes.Length > 0)
                {
                    SetForegroundWindow(processes[0].MainWindowHandle);
                    SendKeys.SendWait("{F10}");
                    System.Threading.Thread.Sleep(250);
                    SendKeys.SendWait(finalCommand);
                    SendKeys.SendWait("{ENTER}");
                }
            }
        }
    }
}