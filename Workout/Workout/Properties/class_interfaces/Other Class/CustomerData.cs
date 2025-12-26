namespace Workout.Properties.class_interfaces.Other
{
    public class CustomerData
    {
        public Dictionary<string, List<string>> questionAndAnswer = new Dictionary<string, List<string>>();
        public bool activeCustomer;
        public Dictionary<DateTime, TrainingDay> trainingDays = new Dictionary<DateTime, TrainingDay>();
        public CustomerData()
        {
            questionAndAnswer = new Dictionary<string, List<string>>();
            activeCustomer = false;
            trainingDays = new Dictionary<DateTime, TrainingDay>();
        }
        public CustomerData(Dictionary<string, List<string>> questionAndAnswer, bool activeCustomer)
        {
            this.questionAndAnswer = questionAndAnswer;
            this.activeCustomer = activeCustomer;
            trainingDays = new Dictionary<DateTime, TrainingDay>();
        }
        //Ha valami nehéz vagy könnyü akkor itt kap értesitést és az edző még tudja modositani az edzéstervet
        //

    }

}
