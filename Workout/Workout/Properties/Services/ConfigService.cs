using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Workout.Properties.Services
{
    public static class ConfigService
    {
        public static string ApiBaseUrl { get; private set; }
        public static string OldApiBaseUrl { get; private set; }
        public static string SecurityCode { get; private set; }

        static ConfigService()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Workout.Platforms.appsettings.json");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            ApiBaseUrl = data["ApiBaseUrl"];
            OldApiBaseUrl = data["OldApiBaseUrl"];
            SecurityCode = data["SecurityCode"];
        }
    }
}
