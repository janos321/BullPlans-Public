using Workout.Properties.class_interfaces.Accessories;
using Workout.Properties.class_interfaces.Other;

namespace Workout.Properties.Services.Other_Services
{
    public class TrainingDataService
    {
        private const string Controller = "training/";
        private readonly ApiClient _api;

        public TrainingDataService()
        {
            _api = new ApiClient();
        }

        public async Task<Dictionary<string, TrainingData>> GetTrainingData(string email)
        {
            var response = await _api.PostAsync<Dictionary<string, TrainingData>>(
                Controller + "get",
                new { email }
                );

            return response?.Data ?? new();
        }

        public async Task<bool> SaveTrainingData(string email, Dictionary<string, TrainingData> training_data)
        {
            var response = await _api.PutAsync<bool>(
                Controller + "put",
                new { email, training_data }
            );

            return response?.Success ?? false;
        }
    }
}
