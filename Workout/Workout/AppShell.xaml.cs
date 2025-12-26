namespace Workout
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        public void SetMainMenuPage(ContentPage newPage, string routeName, string title)
        {
            // Eltávolítja a jelenlegi oldalt és a hozzá tartozó útvonalat, ha létezik
            var existingPage = this.Items
                .OfType<ShellContent>()
                .FirstOrDefault(x => x.Route == routeName);

            if (existingPage != null)
            {
                this.Items.Remove(existingPage);
                Routing.UnRegisterRoute(routeName);
            }

            // Új oldal hozzáadása
            var newShellContent = new ShellContent
            {
                Title = string.IsNullOrEmpty(title) ? null : title,
                ContentTemplate = new DataTemplate(() => newPage),
                Route = routeName
            };

            this.Items.Add(newShellContent);
            Routing.RegisterRoute(routeName, newPage.GetType());
        }


    }
}
