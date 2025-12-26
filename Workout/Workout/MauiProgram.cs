using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Workout.Properties.class_interfaces.Accessories;
using Workout.Properties.Services.Main;
using Workout.Properties.Services.Other;
using Workout.Properties.Services.Other_Services;

namespace Workout
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseSkiaSharp()
                .UseMauiApp<App>()
                .UseMauiCompatibility()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<ApiClient>();

            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<TrainerService>();
            builder.Services.AddSingleton<MainFajlService>();
            builder.Services.AddSingleton<LogInRegService>();
            builder.Services.AddSingleton<MotivationService>();
            builder.Services.AddSingleton<MessagesService>();
            builder.Services.AddSingleton<ProfilePicService>();
            builder.Services.AddSingleton<OfferService>();
            builder.Services.AddSingleton<CustomerService>();
            builder.Services.AddSingleton<OtherFajlService>();
            builder.Services.AddSingleton<TrainingDataService>();

            builder.Services.AddTransient<TrainerPage>();
            builder.Services.AddTransient<MenuPage>();
            builder.Services.AddTransient<Train>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}


