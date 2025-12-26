using Newtonsoft.Json;

namespace Workout.Properties.class_interfaces.Accessories
{
    public class ApiResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }

        public override string ToString()
        {
            return $"Success: {Success}, Message: {Message}, Data: {JsonConvert.SerializeObject(Data)}";
        }
    }
}
