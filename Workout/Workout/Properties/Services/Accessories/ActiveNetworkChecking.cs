namespace Workout.Properties.Services.Accessories
{
    internal class ActiveNetworkChecking
    {
        public static async Task<bool> ActiveNetworkCheck()
        {
            var current = Connectivity.NetworkAccess;

            if (true/*current == NetworkAccess.Internet*/)
            {
                return true;
            }
            else
            {
                await MainPage.menuPage.VanAktivHálózatiKapcsolat();
                return false;
            }
        }
    }
}
