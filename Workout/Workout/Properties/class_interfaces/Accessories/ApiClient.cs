using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using Workout.Properties.Services;
using Workout.Properties.Services.Accessories;
using static Workout.Properties.class_interfaces.Other_Class.ProfilePic;

namespace Workout.Properties.class_interfaces.Accessories
{
    public class ApiClient
    {
        private readonly HttpClient _client;

        public ApiClient()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(ConfigService.ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // PUT=update POST=Add
        public async Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object? payload = null)
        {
            return await RequestAsync<T>(HttpMethod.Post, endpoint, payload);
        }

        public Task<ApiResponse<T>?> PostAsync<T>(string endpoint)
            => PostAsync<T>(endpoint, new { });

        public Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object payload)
            => RequestAsync<T>(HttpMethod.Put, endpoint, payload);

        public Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint, object? payload = null)
            => RequestAsync<T>(HttpMethod.Delete, endpoint, payload);

        private async Task<ApiResponse<T>> RequestAsync<T>(
            HttpMethod method,
            string endpoint,
            object? payload = null)
        {
            try
            {
                // ---- PAYLOAD ----
                var baseDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(payload ?? new { })
                ) ?? new Dictionary<string, object>();

                if (!baseDict.ContainsKey("biztonsagiKod"))
                    baseDict["biztonsagiKod"] = ConfigService.SecurityCode;

                var json = JsonConvert.SerializeObject(baseDict);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ---- HTTP ----
                var request = new HttpRequestMessage(method, endpoint)
                {
                    Content = content
                };

                var response = await _client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                GlobalErrorHandler.Log(
                    $"[{method}] {endpoint}\nPayload: {json}\nResponse: {body}"
                );

                // ---- TECHNIKAI HIBA: nem JSON ----
                if (string.IsNullOrWhiteSpace(body) || !body.TrimStart().StartsWith("{"))
                {
                    GlobalErrorHandler.Show(
                        $"HTTP {(int)response.StatusCode}: A szerver nem JSON választ adott"
                    );

                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = "Technikai hiba: érvénytelen válasz",
                        Data = default
                    };
                }

                var intermediate = JsonConvert.DeserializeObject<ApiResponse<JToken>>(body);
                if (intermediate == null)
                {
                    GlobalErrorHandler.Show("Technikai hiba: nem értelmezhető API válasz.");

                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = "Technikai hiba: hibás válasz",
                        Data = default
                    };
                }

                // ---- DATA KONVERZIÓ ----
                T? data = default;
                if (intermediate.Data != null)
                {
                    try
                    {
                        data = intermediate.Data.ToObject<T>();
                    }
                    catch (Exception ex)
                    {
                        GlobalErrorHandler.Show("JSON konvertálási hiba: " + ex.Message);

                        return new ApiResponse<T>
                        {
                            Success = false,
                            Message = "Technikai hiba: adatkonverzió",
                            Data = default
                        };
                    }
                }

                // ---- ÜZLETI VÁLASZ ----
                return new ApiResponse<T>
                {
                    Success = intermediate.Success,
                    Message = intermediate.Message,
                    Data = data
                };
            }
            catch (TaskCanceledException)
            {
                GlobalErrorHandler.Show("Időtúllépés: a szerver nem válaszolt.");

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Időtúllépés",
                    Data = default
                };
            }
            catch (HttpRequestException ex)
            {
                GlobalErrorHandler.Show("Hálózati hiba: " + ex.Message);

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Hálózati hiba",
                    Data = default
                };
            }
            catch (Exception ex)
            {
                GlobalErrorHandler.Show("Ismeretlen hiba: " + ex.Message);

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Ismeretlen technikai hiba",
                    Data = default
                };
            }
        }



        public async Task<T?> UploadMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                if (!content.Any(p =>
                    p.Headers.ContentDisposition?.Name?.Trim('"') == "biztonsagiKod"))
                {
                    content.Add(new StringContent(ConfigService.SecurityCode), "biztonsagiKod");
                }

                var response = await _client.PostAsync(endpoint, content);
                string body = await response.Content.ReadAsStringAsync();

                GlobalErrorHandler.Log($"[MULTIPART] {endpoint}\nResponse: {body}");

                if (!response.IsSuccessStatusCode)
                {
                    GlobalErrorHandler.Show($"Upload failed: {(int)response.StatusCode}");
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(body);
            }
            catch (Exception ex)
            {
                GlobalErrorHandler.Show("Upload error: " + ex.Message);
                return default;
            }
        }
        public async Task<FileDownloadResult?> DownloadFileAsync(string endpoint, object payload)
        {
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(payload))
                    ?? new Dictionary<string, object>();

                if (!dict.ContainsKey("biztonsagiKod"))
                    dict["biztonsagiKod"] = ConfigService.SecurityCode;

                var json = JsonConvert.SerializeObject(dict);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    GlobalErrorHandler.Show($"File download failed: {response.StatusCode}");
                    return null;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();

                response.Headers.TryGetValues("X-File-Name", out var names);
                var fileName = names?.FirstOrDefault() ?? "profile.png";

                return new FileDownloadResult
                {
                    Bytes = bytes,
                    FileName = fileName
                };
            }
            catch (Exception ex)
            {
                GlobalErrorHandler.Show("Download error: " + ex.Message);
                return null;
            }
        }

    }
}
