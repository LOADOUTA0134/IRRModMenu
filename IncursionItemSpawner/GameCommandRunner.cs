using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace IncursionItemSpawner
{
    internal static class GameCommandRunner
    {
        private const string GameProcessName = "Test_C-Win64-Shipping";
        private const string SettingsFileName = "settings.json";

        private static readonly string SettingsDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IncursionItemSpawner");

        private static readonly string SettingsPath = Path.Combine(SettingsDir, SettingsFileName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private sealed class Settings
        {
            public string GameExePath { get; set; }
        }

        internal static bool TrySendCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return false;

            if (!TryEnsureGameRunning(out var process))
                return false;

            if (!TryFocusMainWindow(process, timeoutMs: 30_000))
            {
                System.Windows.MessageBox.Show("Game window not ready yet.");
                return false;
            }

            SendKeys.SendWait("{F10}");
            Thread.Sleep(250);
            SendKeys.SendWait(command);
            SendKeys.SendWait("{ENTER}");
            return true;
        }

        private static bool TryEnsureGameRunning(out Process process)
        {
            process = GetGameProcess();
            if (process != null)
                return true;

            var exePath = LoadSettings()?.GameExePath;
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Select game executable",
                    Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*",
                    FileName = GameProcessName + ".exe"
                };

                if (dialog.ShowDialog() != true)
                    return false;

                exePath = dialog.FileName;
                SaveSettings(new Settings { GameExePath = exePath });
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to start game: " + ex.Message);
                return false;
            }

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 60_000)
            {
                process = GetGameProcess();
                if (process != null)
                    return true;

                Thread.Sleep(500);
            }

            System.Windows.MessageBox.Show("Game did not start in time.");
            return false;
        }

        private static bool TryFocusMainWindow(Process process, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    process.Refresh();
                    if (process.HasExited)
                        return false;

                    if (process.MainWindowHandle != IntPtr.Zero)
                        return SetForegroundWindow(process.MainWindowHandle);
                }
                catch
                {
                    return false;
                }

                Thread.Sleep(200);
            }

            return false;
        }

        private static Process GetGameProcess()
        {
            try
            {
                return Process.GetProcessesByName(GameProcessName)
                    .OrderByDescending(p => p.MainWindowHandle != IntPtr.Zero)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static Settings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return null;

                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsPath));
            }
            catch
            {
                return null;
            }
        }

        private static void SaveSettings(Settings settings)
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // Ignored: not critical for core functionality.
            }
        }
    }
}
