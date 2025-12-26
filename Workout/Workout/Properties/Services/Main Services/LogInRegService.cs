using Newtonsoft.Json;
using Workout.Properties.class_interfaces.Accessories;
using Workout.Properties.class_interfaces.Main;

namespace Workout.Properties.Services.Main
{
    public class LogInRegService
    {
        private const string Controller = "loginreg/";
        private readonly ApiClient _api;

        public LogInRegService(ApiClient api)
        {
            _api = api;
        }
        public async Task<string?> CheckEmail(string email)
        {
            var response = await _api.PostAsync<string>(
                Controller + "email/check",
                new { email }
            );

            return response?.Data;
        }

        public async Task<bool> UpdatePassword(string email, string new_password)
        {
            var response = await _api.PostAsync<bool>(
                Controller + "password/update",
                new { email, new_password }
            );

            return response?.Success ?? false;
        }

        public async Task<bool> RegisterUser(RegisterPayload user)
        {
            var response = await _api.PostAsync<bool>(
                Controller + "user/register",
                user
            );

            return response?.Success ?? false;
        }

        public async Task<bool> RegisterTrainer(RegisterPayload trainer)
        {
            var json = JsonConvert.SerializeObject(trainer);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            var response = await _api.PostAsync<bool>(
                Controller + "trainer/register",
                dict!
            );

            return response?.Success ?? false;
        }

        /// <summary>
        /// LOGIN – backend eldönti: admin | trainer | user | already_logged_in | null
        /// </summary>
        public async Task<string?> GetRole(string email, string password)
        {
            var response = await _api.PostAsync<string>(
                Controller + "login",
                new { email, password }
            );

            if (response == null || !response.Success)
                return null;

            return response.Data;
        }
    }
}
