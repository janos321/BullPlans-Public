namespace Workout.Properties.class_interfaces.Other
{
    public class TrainingDay
    {
        public List<Training> trainings = new List<Training>();
        public string summery = "";
        public int finish = 0;
        public int hardLevel = 2;//1=könnyü , 2= közepes , 3=nehéz

        public TrainingDay()
        {
            trainings = new List<Training>();
            summery = "";
            finish = 0;
            hardLevel = 2;
        }
    }

    public class Training
    {
        private string id;
        private string exerciseTime;
        private string quantity;
        private string weight;
        private string finalRestTime;


        public Training(string id, string exerciseTime, string quantity, string weight, string finalRestTime)
        {
            this.id = id;
            this.exerciseTime = exerciseTime;
            this.quantity = quantity;
            this.weight = weight;
            this.finalRestTime = finalRestTime;
        }

        public string Id => id;

        // A exerciseTime, quantity, weight, és finalRestTime típusának konvertálása int-té, hiba esetén 5-ös értékkel
        public int ExerciseTime => ParseOrDefault(exerciseTime);
        public int Quantity => ParseOrDefault(quantity);
        public int Weight => ParseOrDefault(weight);
        public int FinalRestTime => ParseOrDefault(finalRestTime);

        private int ParseOrDefault(string input)
        {
            bool success = int.TryParse(input, out int result);
            return success ? result : 5;
        }

    }
}
