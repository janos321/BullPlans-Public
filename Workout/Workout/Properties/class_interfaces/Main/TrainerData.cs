using Workout.Properties.class_interfaces.Other;

namespace Workout.Properties.class_interfaces.Main
{
    public class TrainerData
    {
    }

    public class TrainerResponse
    {
        public string name { get; set; }
        public string email { get; set; }
        public string date { get; set; }
        public ProfileData profile_data { get; set; }
        public TrainerData valid_data { get; set; }
    }
}
