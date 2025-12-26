using Workout.Properties.class_interfaces.Accessories;

namespace Workout.Properties.Services.Other
{
    public class MotivationService
    {
        private const string Controller = "motivation/";
        private readonly ApiClient _api;

        public MotivationService(ApiClient api)
        {
            _api = api;
        }

        public async Task<string> GetMotivation(string lang)
        {
            var response = await _api.PostAsync<string>(
                Controller + "get",
                new { lang }
            );
            return response?.Data ?? "";
        }

        public async Task<bool> PostMotivation(Dictionary<string, string> translations)
        {
            var response = await _api.PostAsync<bool>(
                Controller + "post",
                new { translations }
            );

            return response?.Success ?? false;
        }
    }
}
