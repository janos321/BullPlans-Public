using SkiaSharp;
using Workout.Properties.class_interfaces.Main;
using Workout.Properties.Services.Other;

namespace Workout.Properties.Services.Accessories
{
    public class uploadResponse
    {
        public ImageSource? image {  get; set; }
        public string error { get; set; }

        public uploadResponse(ImageSource? image, string error)
        {
            this.image = image;
            this.error = error;
        }
    }
    public class ProfilePicGetUpload
    {
        private readonly ProfilePicService _profileService;

        public ProfilePicGetUpload(ProfilePicService profileService)
        {
            _profileService = profileService;
        }

        public async Task<ImageSource?> LoadProfileImage()
        {
            // Ellenőrizzük, hogy van-e elmentett kép elérési út
            if (Preferences.ContainsKey("ProfileImagePath"))
            {
                var savedImagePath = Preferences.Get("ProfileImagePath", string.Empty);
                if (!string.IsNullOrEmpty(savedImagePath) && File.Exists(savedImagePath))
                {
                    return ImageSource.FromStream(() =>
                    {
                        return File.OpenRead(savedImagePath);
                    });
                }
                else
                {
                    return await adatbazisbolKepKiolvasas();
                }
            }
            else
            {
                return await adatbazisbolKepKiolvasas();
            }
        }

        private async Task<ImageSource?> adatbazisbolKepKiolvasas()
        {
            var result = await _profileService.DownloadProfilePic(UserDatas.Email);


            if (result.Value.Image is not StreamImageSource streamImageSource)
                return null;

            await using var imageStream = await streamImageSource.Stream(CancellationToken.None);
            if (imageStream == null)
                return null;

            var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;      

            await SaveImageLocallyAsync(memoryStream, result.Value.FileName);

            return ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
        }

        public async Task<uploadResponse?> PickAndUploadPhotoAsync()
        {
            var result = await PickPhotoAsync();

            if (result != null)
            {
                if (IsImageFile(result))
                {
                    var stream = await result.OpenReadAsync();

                    if (IsFileSizeValid(stream))
                    {
                        var compressedStream = CompressImage(stream);

                        var memoryStream = await CopyToMemoryStreamAsync(compressedStream);
                        ImageSource Image = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));

                        await SaveImageLocallyAsync(memoryStream, result.FileName);
                        if(!await UploadImageToDatabaseAsync(memoryStream))
                        {
                            return new uploadResponse(null, "hiba4");
                        }

                        return new uploadResponse(Image, "");
                    }
                    else
                    {
                        return new uploadResponse(null, "tooBig");
                    }
                }
                else
                {
                    return new uploadResponse(null, "jpgPng");
                }
            }
            return new uploadResponse(null, "hiba3");
        }

        private async Task<FileResult> PickPhotoAsync()
        {
            try
            {
                return await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle ="Valasz kepet" //Nyelvbeallitas["valaszKepet"]
                });
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool IsImageFile(FileResult file)
        {
            return file.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   file.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsFileSizeValid(Stream stream)
        {
            return stream.Length <= 1 * 1024 * 1024;
        }

        private async Task<MemoryStream> CopyToMemoryStreamAsync(Stream input)
        {
            var memoryStream = new MemoryStream();
            await input.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private async Task SaveImageLocallyAsync(MemoryStream memoryStream, string fileName)
        {
            KepTorles();

            var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write))
            {
                memoryStream.WriteTo(fileStream);
            }

            Preferences.Set("ProfileImagePath", localPath);
        }

        public void KepTorles()
        {
            // Ellenőrizzük, hogy van-e elmentett kép elérési út a Preferences-ben
            if (Preferences.ContainsKey("ProfileImagePath"))
            {
                var oldImagePath = Preferences.Get("ProfileImagePath", string.Empty);
                if (!string.IsNullOrEmpty(oldImagePath) && File.Exists(oldImagePath))
                {
                    try
                    {
                        File.Delete(oldImagePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Nem sikerült törölni a fájlt: {oldImagePath}, Hiba: {ex.Message}");
                    }
                }
                Preferences.Remove("ProfileImagePath");
            }
        }


        private async Task<bool> UploadImageToDatabaseAsync(MemoryStream memoryStream)
        {
            memoryStream.Position = 0;
            return await _profileService.UploadProfilePic(UserDatas.Email, memoryStream);
        }

        public Stream CompressImage(Stream input)
        {
            input.Position = 0;
            using (var original = SKBitmap.Decode(input))
            {
                using (var image = SKImage.FromBitmap(original))
                {
                    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 90))
                    {
                        var stream = new MemoryStream();
                        data.SaveTo(stream);
                        stream.Position = 0;
                        return stream;
                    }
                }
            }
        }
    }
}
