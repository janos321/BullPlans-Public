using Workout.Properties.class_interfaces.Main;

namespace Workout.Properties.class_interfaces.Interface
{
    public class AppSettings
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Date { get; set; }
        public ProfileData allData { get; set; }
        public ValidData validData { get; set; }
        public TrainerData trainerDatas { get; set; }
    }
}
