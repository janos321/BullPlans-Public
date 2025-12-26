using Workout.Properties.class_interfaces.Other;
using Workout.Properties.class_interfaces.Accessories;

namespace Workout.Properties.Services.Other_Services
{
    public class OfferService
    {
        private const string Controller = "offers/";
        private readonly ApiClient _api;

        public OfferService(ApiClient api)
        {
            _api = api;
        }

        public async Task<Dictionary<string, List<Offer>>> GetOffers()
        {
            var response = await _api.PostAsync<Dictionary<string, List<Offer>>>(
                Controller + "get"
            );
            return response?.Data ?? new();
        }

        public async Task<List<Offer>> GetTrainerOffers(string TrainerEmail)
        {
            var response = await _api.PostAsync<List<Offer>>(
                Controller + "get/trainer",
                new { email = TrainerEmail }
            );
            return response?.Data ?? new();
        }

        public async Task<bool> PostTrainerOffers(string TrainerEmail, List<Offer> offers)
        {
            var response = await _api.PostAsync<bool>(
                Controller + "post",
                new { 
                    email = TrainerEmail, 
                    offers = offers 
                }
            );

            return response?.Success ?? false;
        }
    }
}
