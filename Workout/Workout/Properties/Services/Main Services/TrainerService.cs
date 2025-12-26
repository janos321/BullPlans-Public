using Newtonsoft.Json;
using Workout.Properties.class_interfaces.Accessories;
using Workout.Properties.class_interfaces.Main;
using Workout.Properties.Services.Accessories;

namespace Workout.Properties.Services.Main
{
    public class TrainerService
    {
        private const string Controller = "trainer/";
        private readonly ApiClient _api;

        public TrainerService(ApiClient api)
        {
            _api = api;
        }
        public async Task<bool> LogoutTrainer(string email, ProfileData profileData)
        {
            var response = await _api.PutAsync<bool>(
                Controller + "logout",
                new { email, profile_data = profileData }
            );

            return response?.Success ?? false;
        }

        public async Task<bool> LoadTrainer(string email)
        {
            var response = await _api.PostAsync<TrainerResponse>(
                Controller + "get",
                new { email }
            );

            if (response == null || !response.Success || response.Data == null)
                return false;

            UserDatas.UserName = response.Data.name;
            UserDatas.Email = response.Data.email;
            UserDatas.Date = response.Data.date;
            UserDatas.profileData = response.Data.profile_data;
            UserDatas.trainerDatas = response.Data.valid_data;

            return true;
        }

        public async Task<bool> UpdateTrainerProfileData(string email, ProfileData profile)
        {
            var response = await _api.PutAsync<bool>(
                Controller + "update/profileData",
                new { email, profile_data = profile }
            );

            return response?.Success ?? false;
        }
    }
}
