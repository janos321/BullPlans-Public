using Workout.Properties.class_interfaces.Accessories;
using Workout.Properties.class_interfaces.Other;

namespace Workout.Properties.Services.Other_Services
{
    public class CustomerService
    {

        private const string Controller = "customer/";
        private readonly ApiClient _api;

        public CustomerService(ApiClient api)
        {
            _api = api;
        }

        public async Task<Dictionary<string, CustomerData>> GetCustomers(string trainerEmail)
        {
            var response = await _api.PostAsync<Dictionary<string, CustomerData>>(
                Controller + "get",
                new { email = trainerEmail }
            );
            return response?.Data ?? new();
        }

        public async Task<bool> PutCustomers(string trainer_email, string customer_email, CustomerData customer_data)
        {
            var response = await _api.PutAsync<bool>(
                Controller + "put",
                new { trainer_email, customer_email, customer_data }
            );

            return response?.Success ?? false;
        }
    }
}
