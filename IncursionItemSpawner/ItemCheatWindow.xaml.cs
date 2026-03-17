using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms; // Für SendKeys
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using System.Text.Json;
using System.Threading;


namespace IncursionItemSpawner
{
    /// <summary>
    /// Interaction logic for ItemCheatWindow.xaml
    /// </summary>
    public partial class ItemCheatWindow : Window
    {
        public List<Item> MyItemList { get; set; } = new List<Item>();
        public ItemCheatWindow(List<Item> myItemList)
        {
            InitializeComponent();

            LoadItems();

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

            string commandToSend = GetCommandForAction(itemName, amount);
            GameCommandRunner.TrySendCommand(commandToSend);
        }

        private void CreateLoadout(object sender, RoutedEventArgs e)
        {
            var loadoutDesigner = new LoadoutWindow(this.MyItemList);

            if (loadoutDesigner.ShowDialog() == true)
            {
                string finalCommand = loadoutDesigner.GeneratedCommand;
                GameCommandRunner.TrySendCommand(finalCommand);
            }
        }
    }
}
