using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workout.Properties.Services.Main;

namespace Workout.Properties.class_interfaces.Main
{
    public class ProfileData //minden használt felhasználói adat
    {
        private bool trainer = false;
        private string language;
        private string level;
        public DateTime dailyReward;

        public ProfileData(bool trainer, string language, string level, DateTime dailyReward)
        {
            this.trainer = trainer;
            this.language = language;
            this.level = level;
            this.dailyReward = dailyReward;
        }

        public bool Trainer => trainer;
        public string Language => language;
        public int Level => ParseOrDefault(level);

        public void SetLanguage(string language)
        {
            this.language = language;
        }

        public void SetLevel(int level)
        {
            this.level = level.ToString();
        }

        public bool DailyRewardCheck()
        {
            // Ellenőrizzük, hogy a dailyReward dátum megegyezik-e a mai nappal
            return dailyReward.Date < DateTime.Today;
        }

        public async void setDailyReward()
        {
            dailyReward = DateTime.Now;
            await new MainFajlService().WriteMainFile();
        }

        private int ParseOrDefault(string input)
        {
            bool success = int.TryParse(input, out int result);
            return success ? result : 5;
        }
    }
}
