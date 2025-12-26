using Newtonsoft.Json;
using System.Net.Http.Headers;
using Workout.Properties.Services.Accessories;

namespace Workout.Properties.Services.Other
{
    public class OtherFajlService
    {
        private readonly string urlFajl = ConfigService.OldApiBaseUrl + "FajlService/";

        public async Task<byte[]> DownloadFajl(string email, string fileName)
        {
            if (!await ActiveNetworkChecking.ActiveNetworkCheck())
            {
                return null;
            }

            var httpClient = new HttpClient();
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(email), "email");
            formData.Add(new StringContent(fileName), "fileName");
            formData.Add(new StringContent(ConfigService.SecurityCode), "biztonsagiKod");

            var response = await httpClient.PostAsync(urlFajl + "DownloadTrainerFiles.php", formData);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteFajl(string email, string fileName)
        {
            if (!await ActiveNetworkChecking.ActiveNetworkCheck())
            {
                return false;
            }
            try
            {
                var httpClient = new HttpClient();
                var values = new Dictionary<string, string>
                {
                    { "biztonsagiKod", ConfigService.SecurityCode },
                    { "email", email },
                    { "fileName", fileName }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await httpClient.PostAsync(urlFajl + "DeleteTrenerFajl.php", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

                    if (jsonResponse.ContainsKey("success") && (bool)jsonResponse["success"])
                    {
                        return true;
                    }
                    else if (jsonResponse.ContainsKey("error"))
                    {
                        Console.WriteLine($"Hiba történt: {jsonResponse["error"]}");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine($"HTTP hiba: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Hálózati hiba: {httpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ismeretlen hiba történt: {ex.Message}");
                return false;
            }

            return false;
        }

        public async Task<Dictionary<string, List<string>>> GetFajl()
        {
            if (!await ActiveNetworkChecking.ActiveNetworkCheck())
            {
                return new Dictionary<string, List<string>>();
            }
            var httpClient = new HttpClient();
            var formData = new MultipartFormDataContent
                {
                    { new StringContent(ConfigService.SecurityCode), "biztonsagiKod" }
                };

            try
            {
                var response = await httpClient.PostAsync(urlFajl + "ReadTrenerFiles.php", formData);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to get files from server: " + jsonResponse);
                }

                if (jsonResponse.Contains("error"))
                {
                    Console.WriteLine("Error in server response: " + jsonResponse);
                    return new Dictionary<string, List<string>>();
                }

                var files = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonResponse);

                return files ?? new Dictionary<string, List<string>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return new Dictionary<string, List<string>>();
            }
        }

        public async Task<bool> UploadFile(string fileName, Stream fileStream, string email)
        {
            if (!await ActiveNetworkChecking.ActiveNetworkCheck())
            {
                return false;
            }
            HttpClient client = new HttpClient();

            try
            {
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(ConfigService.SecurityCode), "biztonsagiKod");
                    content.Add(new StringContent(email), "email");
                    content.Add(new StringContent(fileName), "fileName");

                    var fileContent = new ByteArrayContent(fileData);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, "file", fileName);

                    var response = await client.PostAsync(urlFajl + "UploadFile.php", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
                    return result.ContainsKey("success") && (bool)result["success"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during file upload: " + ex.Message);
                return false;
            }
        }
    }
}
