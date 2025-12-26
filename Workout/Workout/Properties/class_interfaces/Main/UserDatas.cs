using Newtonsoft.Json;
using Workout.Properties.class_interfaces.Interface;
using Workout.Properties.class_interfaces.Other;
namespace Workout.Properties.class_interfaces.Main
{
    class UserDatas : AppSettings
    {
        public static string UserName { get; set; } = "FirstName";
        public static string Email { get; set; } = "Email";
        public static string Date { get; set; } = "Date";
        public static ProfileData profileData { get; set; } = new ProfileData(false, "English", "0", new DateTime());
        public static ValidData validData { get; set; } = new ValidData();
        public static TrainerData trainerDatas { get; set; } = new TrainerData();
        


        public static void PrintFelhasznaloiAdatok()
        {
            Console.WriteLine("Felhasználói adatok:");
            Console.WriteLine($"Felhasználónév: {UserName}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine($"Dátum: {Date}");

            Console.WriteLine("\nAllData:");
            Console.WriteLine($"Nyelv: {profileData.Language}");
            Console.WriteLine($"Szint: {profileData.Level}");
            Console.WriteLine($"Napi Jutalom: {profileData.dailyReward}");

            Console.WriteLine("\nÉrvényes Adatok:");
            Console.WriteLine($"Eligible Main Terv: {validData.eligibleMainTerv}");

            Console.WriteLine("\nTraining Napok:");
            foreach (var nap in validData.trainingDays)
            {
                Console.WriteLine($"Dátum: {nap.Key}, Edzés Nap Összefoglaló: {nap.Value.summery}");
                foreach (var training in nap.Value.trainings)
                {
                    Console.WriteLine($"    Gyakorlati Idő: {training.ExerciseTime}");
                    Console.WriteLine($"    Mennyiség: {training.Quantity}");
                    Console.WriteLine($"    Súly: {training.Weight}");
                    Console.WriteLine($"    Végső Pihenő Idő: {training.FinalRestTime}");
                }
            }

            Console.WriteLine("\nAdatok:");
            foreach (var adat in validData.datas)
            {
                Console.WriteLine($"Kulcs: {adat.Key}, Értékek: {string.Join(", ", adat.Value)}");
            }
        }

    }

    //Ez csak jelenlegi megoldás
    class Storage
    {
        public static List<Offer> trainerOffer=new List<Offer>();

        public static Dictionary<string, CustomerData> costumers = new Dictionary<string, CustomerData>();

        public static Dictionary<string, TrainingData> trainingDatas = new Dictionary<string, TrainingData>();
    }

    public class RegisterPayload
    {
        public string name { get; set; }
        public string email { get; set; }
        public string date { get; set; }
        public string password { get; set; }
        public ProfileData profile_data { get; set; }
        public object valid_data { get; set; }
    }

    public class UserResponse
    {
        public string name { get; set; }
        public string email { get; set; }
        public string date { get; set; }
        public ProfileData profile_data { get; set; }
        public ValidData valid_data { get; set; }
    }
}
