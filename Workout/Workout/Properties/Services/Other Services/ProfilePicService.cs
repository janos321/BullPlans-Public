using Microsoft.Maui.Controls;
using System.Net.Http.Headers;
using Workout.Properties.class_interfaces.Accessories;
using static System.Net.Mime.MediaTypeNames;

namespace Workout.Properties.Services.Other
{
    public class ProfilePicService
    {
        private readonly ApiClient _api;
        private const string Controller = "profile/";

        public ProfilePicService(ApiClient api)
        {
            _api = api;
        }

        public async Task<bool> UploadProfilePic(string email, Stream fileStream)
        {
            var content = new MultipartFormDataContent
                {
                    { new StringContent(email), "email" }
                };

            var bytes = new StreamContent(fileStream);
            bytes.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            content.Add(bytes, "file", "upload.jpg");

            var resp = await _api.UploadMultipartAsync<ApiResponse<bool>>(Controller + "upload", content);

            return resp?.Success ?? false;
        }


        public async Task<(ImageSource Image, string FileName)?> DownloadProfilePic(string email)
        {
            var result = await _api.DownloadFileAsync(
                Controller + "download",
                new { email }
            );

            if (result == null || result.Bytes.Length == 0)
                return null;

            var imageSource = ImageSource.FromStream(
                () => new MemoryStream(result.Bytes)
            );

            return (imageSource, result.FileName);
        }
    }
}
