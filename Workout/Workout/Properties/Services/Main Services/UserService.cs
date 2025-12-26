using Workout.Properties.class_interfaces.Accessories;
using Workout.Properties.class_interfaces.Main;

namespace Workout.Properties.Services.Main
{
    public class UserService
    {
        private readonly ApiClient _api;
        private const string Controller = "user/";

        public UserService(ApiClient api)
        {
            _api = api;
        }

        public async Task<bool> LoadUser(string email)
        {
            var response = await _api.PostAsync<UserResponse>(
                Controller + "get",
                new { email }
            );

            if (response == null || !response.Success || response.Data == null)
                return false;

            UserDatas.UserName = response.Data.name;
            UserDatas.Email = response.Data.email;
            UserDatas.Date = response.Data.date;
            UserDatas.profileData = response.Data.profile_data;
            UserDatas.validData = response.Data.valid_data;

            return true;
        }

        public async Task<ValidData> GetUserValidData(string email)
        {
            var response = await _api.PostAsync<ValidData>(
                Controller + "get/validData",
                new { email }
            );

            return response?.Data ?? new ValidData();
        }

        public async Task<bool> LogoutUser(string email, ProfileData profileData)
        {
            var response = await _api.PutAsync<bool>(
                Controller + "logout",
                new { email, profile_data = profileData }
            );

            return response?.Success ?? false;
        }

        public async Task<bool> UpdateUserValidData(string email, ValidData validData)
        {
            var response = await _api.PutAsync<bool>(
                Controller + "update/validData",
                new { email, valid_data = validData }
            );

            return response?.Success ?? false;
        }

        public async Task<bool> UpdateUserProfileData(string email, ProfileData profile)
        {
            var response = await _api.PutAsync<bool>(
                Controller + "update/profileData",
                new { email, profile_data = profile }
            );

            return response?.Success ?? false;
        }
    }
}
