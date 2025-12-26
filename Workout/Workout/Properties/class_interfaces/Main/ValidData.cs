using Workout.Properties.class_interfaces.Other;

namespace Workout.Properties.class_interfaces.Main
{
    public class ValidData // idejönnek majd azok a hitelesitett/megvett datas, mint pl az edzés, étrend, meg ilyenek, meg pl az is hogy mikortolo mikorig vette meg az adott csomagot meg ilyneke, meg hogy van-e aktiv kérvényezett csomagja
    {
        public bool eligibleMainTerv = false;
        public Dictionary<DateTime, TrainingDay> trainingDays = new();
        public Dictionary<string, List<string>> datas = new();

        public string trainerEmailAddress = "";
        public ValidData()
        {
            trainingDays = new Dictionary<DateTime, TrainingDay>();
            eligibleMainTerv = false;
            datas = new Dictionary<string, List<string>>();
            trainerEmailAddress = "";
        }

        /*public void TraningAd(DateTime dateTime, TrainingDay train)
        {
            trainingDays[dateTime] = train;
        }*/
    }
}
