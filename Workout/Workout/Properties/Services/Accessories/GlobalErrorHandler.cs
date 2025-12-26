using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workout.Properties.Services.Accessories
{
    public static class GlobalErrorHandler
    {
        public static void Show(string message)
        {
            // Alkalmazás típusától függően:
            // WinForms: MessageBox.Show(message);
            // MAUI / WPF: App.Current.MainPage.DisplayAlert("Hiba", message, "OK");
            Console.WriteLine($"[HIBA] {message}");
        }

        public static void Log(string message)
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkoutErrorLog.txt");
            File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            Console.WriteLine(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
        }
    }

    public static class GlobalSuccesHandler
    {
        public static void Show(string message)
        {
            // Alkalmazás típusától függően:
            // WinForms: MessageBox.Show(message);
            // MAUI / WPF: App.Current.MainPage.DisplayAlert("Hiba", message, "OK");
            Console.WriteLine($"[OK] {message}");
        }

        public static void Log(string message)
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkoutErrorLog.txt");
            File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            Console.WriteLine(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
        }
    }
}
