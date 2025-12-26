using Workout.Properties.class_interfaces.Other;

namespace Workout.Properties.Services.Accessories
{
    public class TrainingPlanCreat
    {
        public Dictionary<string, Dictionary<string, TrainingDay>> weeks { get; set; }
        public List<bool> sameWeekQuestion = new List<bool>();
        //private Dictionary<string, TrainingData> trainingDatas=new Dictionary<string, TrainingData>();

        public TrainingPlanCreat()
        {
            //InitializeAsync();
            sameWeekQuestion.Add(true);
            sameWeekQuestion.Add(false);
            sameWeekQuestion.Add(false);
            sameWeekQuestion.Add(false);
            weeks = new Dictionary<string, Dictionary<string, TrainingDay>>();

            // Inicializáljuk a heteket és napokat
            for (int i = 1; i <= 4; i++)
            {
                string weekKey = $"{i}.het";
                weeks[weekKey] = new Dictionary<string, TrainingDay>
            {
                { "H", new TrainingDay() },
                { "K", new TrainingDay() },
                { "SZ", new TrainingDay() },
                { "CS", new TrainingDay() },
                { "P", new TrainingDay() },
                { "SZO", new TrainingDay() },
                { "V", new TrainingDay() }
            };
            }
        }

        /*private async Task InitializeAsync()
        {
            trainingDatas = await AdatbazisHivasok.GetTrainingData();
        }*/

        // Metódus egy adott day adatainak lekéréséhez egy adott héten
        public TrainingDay GetTrainDay(string week, string day)
        {
            if (weeks.ContainsKey(week) && weeks[week].ContainsKey(day))
            {
                return weeks[week][day];
            }
            throw new ArgumentException("Hibás hét vagy nap megadva. olvasáskor (het: " + week + "  Nap: " + day + ")");
        }

        // Metódus egy adott day adatainak beállításához egy adott héten
        public void PutTrainDay(string week, string day, TrainingDay trainDay)
        {
            if (weeks.ContainsKey(week) && weeks[week].ContainsKey(day))
            {
                weeks[week][day] = trainDay;
            }
            else
            {
                throw new ArgumentException("Hibás hét vagy nap megadva. Modositáskor(het: " + week + "  Nap: " + day + ")");
            }
        }

        public void Update(string week, string day, Training traning)
        {
            TrainingDay trainDay = GetTrainDay(week, day);
            for (int i = 0; i < trainDay.trainings.Count; i++)
            {
                if (trainDay.trainings[i].Id == traning.Id)
                {
                    trainDay.trainings[i] = traning;
                    PutTrainDay(week, day, trainDay);
                    return;
                }
            }
            throw new ArgumentException("Nem található a megadott nevű edzés.");
        }

        public void UpdateSummery(string week, string day, string summery)
        {
            TrainingDay trainDay = GetTrainDay(week, day);
            trainDay.summery = summery;
            PutTrainDay(week, day, trainDay);
        }

        public void DeleteTraning(string week, string day, string name)
        {
            TrainingDay trainDay = GetTrainDay(week, day);
            for (int i = 0; i < trainDay.trainings.Count; i++)
            {
                if (trainDay.trainings[i].Id == name)
                {
                    trainDay.trainings.Remove(trainDay.trainings[i]);
                    PutTrainDay(week, day, trainDay);
                    return;
                }
            }
            throw new ArgumentException("Nem található a megadott nevű edzés.");
        }

    }
}
