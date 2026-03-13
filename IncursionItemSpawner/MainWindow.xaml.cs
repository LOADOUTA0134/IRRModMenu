using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace IncursionItemSpawner
{
    public partial class MainWindow : Window
    {
        public List<Item> MyItemList { get; set; } = new List<Item>();

        public MainWindow()
        {
            InitializeComponent();

            ApplicationThemeManager.ApplySystemTheme(updateAccent: true);
            ApplicationThemeManager.Apply(ApplicationTheme.Dark, WindowBackdropType.Mica);

            // ZUERST laden, damit MyItemList nicht leer ist
            LoadItems();
        }

        void LoadItems()
        {
            try
            {
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

        private void OpenItemSpawner_Click(object sender, RoutedEventArgs e)
        {
            // Auch hier die Liste übergeben, sonst ist sie dort ebenfalls leer
            var itemWindow = new ItemCheatWindow(this.MyItemList);
            itemWindow.Show();
        }

        private void OpenLoadout_Click(object sender, RoutedEventArgs e)
        {
            // Jetzt enthält MyItemList die Daten aus der Textdatei
            var loadoutWindow = new LoadoutWindow(this.MyItemList);
            loadoutWindow.Show();
        }

        private void OpenFactions_Click(object sender, RoutedEventArgs e)
        {
            new FactionsWindow().Show();
        }
    }
}