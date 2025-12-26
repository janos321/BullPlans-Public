using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Workout.Properties.class_interfaces.Interface;
using Workout.Properties.class_interfaces.Main;

namespace Workout.Properties.Services.Main
{
    public class MainFajlService
    {
        private readonly string fileName = "AdatokTarolasa.txt";
        private readonly byte[] key = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bájt hosszú kulcs (128 bit AES-hez)
        private readonly byte[] iv = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bájt hosszú IV


        public async Task WriteMainFile()
        {
            string content = SerializeSettings();
            try
            {
                var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var filePath = Path.Combine(folderPath, fileName);

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var sw = new StreamWriter(cs))
                            {
                                await sw.WriteAsync(content);
                            }
                        }

                        var encrypted = ms.ToArray();
                        await File.WriteAllBytesAsync(filePath, encrypted);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("-------------> " + e);
            }
        }

        public async Task<bool> ReadMainFile()
        {
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var filePath = Path.Combine(folderPath, fileName);

            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                var encrypted = await File.ReadAllBytesAsync(filePath);

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var ms = new MemoryStream(encrypted))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        var decryptedContent = await sr.ReadToEndAsync();
                        if (string.IsNullOrEmpty(decryptedContent))
                        {
                            return false;
                        }
                        else
                        {
                            Console.WriteLine("-------------> fajlbol olvasas eredmeny " + decryptedContent);
                            DeserializeSettings(decryptedContent);
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Hiba az adatok dekódolása közben: {e.Message}");
                return false;
            }
        }

        public void DeleteFile()
        {
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static string SerializeSettings() //szöveg kodolása
        {
            AppSettings settings = new AppSettings
            {
                UserName = UserDatas.UserName,
                Email = UserDatas.Email,
                Date = UserDatas.Date,
                allData = UserDatas.profileData,
                validData = UserDatas.validData,
                trainerDatas = UserDatas.trainerDatas
            };

            return JsonConvert.SerializeObject(settings);
        }

        private void DeserializeSettings(string json) // szöveg kikodolás
        {
            AppSettings settings = JsonConvert.DeserializeObject<AppSettings>(json);
            if (settings != null)
            {
                UserDatas.UserName = settings.UserName;
                UserDatas.Email = settings.Email;
                UserDatas.Date = settings.Date;
                UserDatas.profileData = settings.allData;
                UserDatas.validData = settings.validData;
                UserDatas.trainerDatas = settings.trainerDatas;
            }
        }
    }
}
