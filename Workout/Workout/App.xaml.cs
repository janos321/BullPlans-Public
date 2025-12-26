using Microsoft.Maui.Controls;
using Workout.Properties;

namespace Workout
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
