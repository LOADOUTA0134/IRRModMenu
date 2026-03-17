using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace IncursionItemSpawner
{
    public partial class LoadoutWindow : System.Windows.Window
    {
        private List<Item> _items;
        public string GeneratedCommand { get; private set; }

        public LoadoutWindow(List<Item> allItems)
        {
            InitializeComponent();
            _items = allItems;
            PopulateComboBoxes();
            RefreshLoadoutList_Click(null, null);
        }

        private void PopulateComboBoxes()
        {
            HelmetBox.ItemsSource = _items.Where(i => i.Category.Contains("Helmet")).Select(i => i.Name).ToList();
            RigBox.ItemsSource = _items.Where(i => i.Category.Contains("Rig") || i.Category.Contains("Plate Carrier")).Select(i => i.Name).ToList();
            BackpackBox.ItemsSource = _items.Where(i => i.Category.Contains("Backpack")).Select(i => i.Name).ToList();
            WeaponBox.ItemsSource = _items.Where(i => i.Category.Contains("Weapon")).Select(i => i.Name).ToList();

            MuzzleBox.ItemsSource = _items.Where(i => i.Category.Contains("Muzzle")).Select(i => i.Name).ToList();
            GripBox.ItemsSource = _items.Where(i => i.Category.Contains("Foregrip")).Select(i => i.Name).ToList();
            OpticBox.ItemsSource = _items.Where(i => i.Category.Contains("Optic")).Select(i => i.Name).ToList();
            MagazineBox.ItemsSource = _items.Where(i => i.Category.Contains("Magazine")).Select(i => i.Name).ToList();
        }

        
        private void RefreshLoadoutList_Click(object sender, RoutedEventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            
            var files = Directory.GetFiles(path, "*.json")
                .Select(f => Path.GetFileName(f))
                .Where(f => !f.EndsWith(".deps.json") && !f.EndsWith(".runtimeconfig.json"))
                .Select(f => Path.GetFileNameWithoutExtension(f)) 
                .ToList();

            LoadoutListBox.ItemsSource = files;
        }

        private void LoadoutListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadoutListBox.SelectedItem == null) return;
            string path = LoadoutListBox.SelectedItem.ToString() + ".json";

            var loadout = JsonSerializer.Deserialize<Loadout>(File.ReadAllText(path));

            LoadoutNameBox.Text = loadout.Name;
            HelmetBox.SelectedItem = loadout.Helmet;
            RigBox.SelectedItem = loadout.Rig;
            BackpackBox.SelectedItem = loadout.Backpack;
            WeaponBox.SelectedItem = loadout.Weapon;
            MuzzleBox.SelectedItem = loadout.Muzzle;
            GripBox.SelectedItem = loadout.Grip;
            OpticBox.SelectedItem = loadout.Optic;
            MagazineBox.SelectedItem = loadout.Magazine;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LoadoutNameBox.Text)) return;
            var loadout = new Loadout
            {
                Name = LoadoutNameBox.Text,
                Helmet = HelmetBox.SelectedItem?.ToString(),
                Rig = RigBox.SelectedItem?.ToString(),
                Backpack = BackpackBox.SelectedItem?.ToString(),
                Weapon = WeaponBox.SelectedItem?.ToString(),
                Muzzle = MuzzleBox.SelectedItem?.ToString(),
                Grip = GripBox.SelectedItem?.ToString(),
                Optic = OpticBox.SelectedItem?.ToString(),
                Magazine = MagazineBox.SelectedItem?.ToString()
            };

            File.WriteAllText(loadout.Name + ".json", JsonSerializer.Serialize(loadout, new JsonSerializerOptions { WriteIndented = true }));
            RefreshLoadoutList_Click(null, null);
            System.Windows.MessageBox.Show("Loadout saved!");
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            var selectedIds = new List<string>();
            ComboBox[] boxes = { HelmetBox, RigBox, BackpackBox, WeaponBox, MuzzleBox, GripBox, OpticBox, MagazineBox };

            foreach (var box in boxes)
            {
                if (box.SelectedItem != null)
                {
                    string itemName = box.SelectedItem.ToString();
                    selectedIds.Add($"{itemName} 1");
                }
            }

            if (selectedIds.Count == 0)
            {
                System.Windows.MessageBox.Show("Select at least one item");
                return;
            }

            GeneratedCommand = "additem " + string.Join(" | additem ", selectedIds);

            this.DialogResult = true;
            this.Close();
        }
        private void DeleteLoadout_Click(object sender, RoutedEventArgs e)
        {
            if (LoadoutListBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please select a loadout to delete.");
                return;
            }

            string loadoutName = LoadoutListBox.SelectedItem.ToString();
            string path = loadoutName + ".json";

            var result = System.Windows.MessageBox.Show($"Are you sure you want to delete '{loadoutName}'?",
                                                        "Confirm Delete",
                                                        MessageBoxButton.YesNo,
                                                        MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);

                        LoadoutNameBox.Clear();
                        RefreshLoadoutList_Click(null, null);
                        System.Windows.MessageBox.Show("Loadout deleted!");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error deleting file: " + ex.Message);
                }
            }
        }
    }
}
