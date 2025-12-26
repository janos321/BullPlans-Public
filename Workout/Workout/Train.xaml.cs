using Microsoft.IdentityModel.Tokens;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Diagnostics;
using System.Timers;
using Workout.Properties.class_interfaces.Main;
using Workout.Properties.class_interfaces.Other;
using Workout.Properties.Services.Accessories;
using Workout.Properties.Services.Main;
using Workout.Properties.Services.Other_Services;

namespace Workout;

public partial class Train : ContentPage
{
    #region változok és kezdetiLépés
    private List<Training> tranings;
    private string osszefoglalo = "";
    private TrainingDay JelenlegiEdzesNap = new TrainingDay();
    public Dictionary<string, string> Nyelvbeallitas;
    private List<ConfettiPiece> confettiPieces;
    private System.Timers.Timer animationTimer;


    //TODO: Nincs megvalositva, most az egész training nem müködik, át kell majd alakitani, de ez a trainingDatas ugy müködik hogy lekéri az edzőjének a gyakorlatjait
    // (a string ilyen egyedi azonosito, még nem találtam ki mi legyen, jelenleg nincs is megvalositva, ott kell kitalálni meg megcsinalni, ahol a trainer létrehozza a trainingeket (Trainer leg baloldali a navigácionál)
    // de kell gondolni arra hogy praktikusan tudja kiválasztani a trainer az edzés össze állitgatosnál, ezt meg kellen majd beszélni/kitalálni hogyan lehetne megcsinalni legjobban)
    public Dictionary<string, TrainingData> trainingDatas = new Dictionary<string, TrainingData>();
    private readonly MainFajlService _mainFajlService;
    private readonly TrainingDataService _trainingDataService;
    public Train(MainFajlService mainFajlService, TrainingDataService trainingDataService)
    {
        _mainFajlService = mainFajlService;
        _trainingDataService = trainingDataService;

        InitializeComponent();

        GetDatas();

        canvasView.PaintSurface += OnCanvasViewPaintSurface;
        confettiPieces = new List<ConfettiPiece>();
        canvasViewKonfetti.PaintSurface += OnCanvasViewPaintSurface2;
    }

    private async void GetDatas()
    {
        if (!UserDatas.validData.trainerEmailAddress.IsNullOrEmpty())
        {
            trainingDatas = await _trainingDataService.GetTrainingData(UserDatas.validData.trainerEmailAddress);
        }
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await IdeNavigalasAMenuBol();
    }
    public async Task IdeNavigalasAMenuBol()
    {
        Nyelvvaltas();
        TrainStartButton.BackgroundColor = Color.FromHex("#FF0000");
        if (UserDatas.validData.trainingDays.TryGetValue(MenuPage.DatumConvertAgeMonthDay(DateTime.Now), out var trainingList) && trainingList != null)
        {
            JelenlegiEdzesNap = trainingList;
        }
        tranings = JelenlegiEdzesNap.trainings;
        osszefoglalo = JelenlegiEdzesNap.summery;
        if (osszefoglalo == "")
        {
            WorkoutButton.Text = Nyelvbeallitas["noTrain"];
        }
        else
        {
            WorkoutButton.Text = Nyelvbeallitas["train"];
            await Letrehoz();
        }
        if (JelenlegiEdzesNap.finish == tranings.Count)
        {
            TrainDone();
        }
    }
    public async Task Letrehoz()
    {
        int index = 0;
        TrainButtons.Children.Clear();
        foreach (var train in tranings)
        {
            // Belső StackLayout a kép és a szöveg számára
            var innerLayout = new StackLayout { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.Center, Orientation = StackOrientation.Horizontal };

            // Kép
            var image = new Image
            {
                Source = trainingDatas[train.Id].AnimationUrl,
                HeightRequest = 80,
                WidthRequest = 80,
                Margin = new Thickness(10), // Belső térköz a kép körül
                IsAnimationPlaying = true
            };

            // Szöveg
            var text = new Label
            {
                Text = trainingDatas[train.Id].Name,
                VerticalOptions = LayoutOptions.Center,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)), // Nagyobb betűméret
                FontAttributes = FontAttributes.Bold, // Félkövér stílus
                TextColor = Color.FromHex("#000000"), // Szöveg színe
                Margin = new Thickness(5, 0, 0, 0) // Térköz a szöveg bal oldalán
            };

            innerLayout.Children.Add(image);
            innerLayout.Children.Add(text);

            // Kattintható ContentView a StackLayout-tal
            var frame = new Frame
            {
                BackgroundColor = (index < JelenlegiEdzesNap.finish) ? Color.FromHex("#008000") : Color.FromHex("#FFFFFF"), // Háttérszín beállítása
                CornerRadius = 20, // Kerekített sarkok
                Margin = new Thickness(20, 10), // Külső térköz
                HeightRequest = 100, // Magasság beállítása
                WidthRequest = 300, // A képernyő 80%-a
                Content = innerLayout
            };

            // TapGestureRecognizer hozzáadása
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (sender, e) =>
            {
                // Itt kezeld az eseményt
                TrainDetails.IsVisible = true;
                TrainOptions.IsVisible = false;
                UpdateTrainDetails(train);
            };
            frame.GestureRecognizers.Add(tapGestureRecognizer);
            index++;

            // Hozzáadás a fő StackLayout-hoz
            TrainButtons.Children.Add(frame);
        }
    }
    #endregion

    #region TrainDetalis

    private void UpdateTrainDetails(Training training)
    {

        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        KepesGrid.HeightRequest = screenHeight / 4;

        string idfajtaKOd = training.Id;

        animationView.Source = trainingDatas[idfajtaKOd].AnimationUrl;
        nameLabel.Text = trainingDatas[idfajtaKOd].Name;
        SulyIsmetlesSzam.Text = Nyelvbeallitas["suly"] + $": {training.Weight}" + "\n" + Nyelvbeallitas["darab"] + $": {training.Quantity}";
        HosszuLeiras.Text = trainingDatas[idfajtaKOd].Longdetail;

    }

    private void BackDetails(object sender, EventArgs e)
    {
        TrainDetails.IsVisible = false;
        TrainOptions.IsVisible = true;

    }

    #endregion


    #region Menunoptions

    private void TrainOptionsButton(object sender, EventArgs e)
    {
        if (osszefoglalo != "")
        {
            Options.IsVisible = !Options.IsVisible;
            TrainOptions.IsVisible = !TrainOptions.IsVisible;
        }

    }

    private async void BackMainPage(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///MenuPage" + MainPage.spawnPageCounter);
    }

    #endregion


    #region Train

    private System.Threading.Timer timer;
    private double secondsElapsed;
    private int durationInSeconds = 60; // Egy perc

    private async void TrainStartAndBackButton(object sender, EventArgs e)
    {
        if (JelenlegiEdzesNap.finish < tranings.Count || TrainShow.IsVisible)
        {
            TrainShow.IsVisible = !TrainShow.IsVisible;
            TrainOptions.IsVisible = !TrainOptions.IsVisible;
            if (TrainOptions.IsVisible)
            {
                await Letrehoz();
            }
            if (TrainShow.IsVisible)
            {
                kovetkezoPiheno();
            }
        }
    }

    private async void kovetkezo()
    {
        if (JelenlegiEdzesNap.finish < tranings.Count)
        {
            double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            KepesGrid2.HeightRequest = screenHeight / 4;

            string idfajtaKOd = tranings[JelenlegiEdzesNap.finish].Id;

            animationView2.Source = trainingDatas[idfajtaKOd].AnimationUrl;
            gyakorlatNev.Text =  trainingDatas[idfajtaKOd].Name;
            mennyisegSuly.Text = Nyelvbeallitas["suly"] + $": {tranings[JelenlegiEdzesNap.finish].Weight}" + "\n" + Nyelvbeallitas["darab"] + $": {tranings[JelenlegiEdzesNap.finish].Quantity}";
            StartTimer(0, tranings[JelenlegiEdzesNap.finish].ExerciseTime, "edzes");
            JelenlegiEdzesNap.finish++;
            await _mainFajlService.WriteMainFile();
        }
        else
        {
            TrainDone();
            TrainShow.IsVisible = false;
            TrainCongratulation();
        }
    }

    private void kovetkezoPiheno()
    {
        if (JelenlegiEdzesNap.finish < tranings.Count)
        {
            double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            KepesGrid3.HeightRequest = screenHeight / 4;

            string idfajtaKOd = tranings[JelenlegiEdzesNap.finish].Id;

            animationView3.Source = trainingDatas[idfajtaKOd].AnimationUrl;
            gyakorlatNev3.Text = trainingDatas[idfajtaKOd].Name;
            mennyisegSuly3.Text = Nyelvbeallitas["suly"] + $": {tranings[JelenlegiEdzesNap.finish].Weight}" + "\n" + Nyelvbeallitas["darab"] + $": {tranings[JelenlegiEdzesNap.finish].Quantity}";
            rovidLeiras3.Text = trainingDatas[idfajtaKOd].ShortDetail;
            StartTimer(0, tranings[JelenlegiEdzesNap.finish].FinalRestTime, "pihenes");
        }
        else
        {
            TrainDone();
            TrainShow.IsVisible = false;
            TrainCongratulation();
        }
    }

    private void Skip(object sender, EventArgs e)
    {
        secondsElapsed = durationInSeconds;
    }

    private void Plus(object sender, EventArgs e)
    {
        durationInSeconds += 10;
    }

    private void StartTimer(double start, int end, string fajta)
    {
        secondsElapsed = start;
        durationInSeconds = end;
        if (fajta == "edzes")
        {
            SlajMode.IsVisible = false;
            TrainMode.IsVisible = true;
            timer = new System.Threading.Timer(TimerTarin, null, 0, 1000); // Az intervallum 1000 ms = 1 másodperc  // A Timer konstruktor egy TimerCallback-ot, egy állapotot, egy kezdő késleltetést és egy intervallumot vár.
        }
        else
            if (fajta == "pihenes")
        {
            SlajMode.IsVisible = true;
            TrainMode.IsVisible = false;
            timer = new System.Threading.Timer(TimerPihenes, null, 0, 1000);
        }
    }

    [Obsolete]
    private void TimerTarin(object state)
    {
        if (!TrainShow.IsVisible)
        {
            timer?.Change(Timeout.Infinite, 0);
            return;
        }
        if (secondsElapsed >= durationInSeconds)
        {
            timer?.Change(Timeout.Infinite, 0); // Timer leállítása
            Device.BeginInvokeOnMainThread(() =>
            {
                kovetkezoPiheno();
            });
            return;
        }

        secondsElapsed++;

        Device.BeginInvokeOnMainThread(() =>
        {
            canvasView2.InvalidateSurface();
        });
    }

    [Obsolete]
    private void TimerPihenes(object state)
    {
        if (!TrainShow.IsVisible)
        {
            timer?.Change(Timeout.Infinite, 0);
            return;
        }
        if (secondsElapsed >= durationInSeconds)
        {
            timer?.Change(Timeout.Infinite, 0); // Timer leállítása
            Device.BeginInvokeOnMainThread(() =>
            {
                kovetkezo();
            });
            return;
        }

        secondsElapsed++; // Növeljük a eltelt időt

        Device.BeginInvokeOnMainThread(() =>
        {
            canvasView.InvalidateSurface(); // Frissítjük a SkiaSharp canvas-t
        });
    }

    private async void TrainDone()
    {
        await Letrehoz();
        TrainStartButton.Text = Nyelvbeallitas["vegeztel"];
        TrainStartButton.BackgroundColor = Color.FromHex("#008000");
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        timer?.Change(Timeout.Infinite, 0); // Az időzítő leállítása, amikor az oldal eltűnik
    }
    private void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        var info = e.Info;
        var surface = e.Surface;
        var canvas = surface.Canvas;

        canvas.Clear();

        // Progress kör rajzolása
        var progressPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 40, // Vastagabb vonal
            Color = SKColors.Red,
            IsAntialias = true
        };

        var backgroundPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 40, // Vastagabb vonal
            Color = SKColors.LightGray,
            IsAntialias = true
        };

        float radius = (float)(Math.Min(info.Width, info.Height) / 2 * 0.85); // Növekvő sugár
        var center = new SKPoint(info.Width / 2, info.Height / 2);
        var rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

        canvas.DrawCircle(center.X, center.Y, radius, backgroundPaint);

        double progress = secondsElapsed / durationInSeconds; // Példa progress érték
        float sweepAngle = (float)(360 * progress);
        canvas.DrawArc(rect, -90, sweepAngle, false, progressPaint);

        // Idő középen
        var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 120, // Nagyobb szöveg
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        canvas.DrawText((durationInSeconds - secondsElapsed).ToString(), center.X, center.Y + (textPaint.TextSize / 2), textPaint);
    }


    #endregion

    #region congraturationPanel

    private void TrainCongratulation()
    {
        canvasViewKonfetti.IsVisible = true;

        if (confettiPieces == null)
        {
            confettiPieces = new List<ConfettiPiece>();
        }

        animationTimer = new System.Timers.Timer(30); // 30 ms interval for animation
        animationTimer.Elapsed += OnAnimationTimerElapsed;
        CongratulationPanel.IsVisible = true;
        StartConfetti();
    }



    #region konfetti
    private void StartConfetti()
    {
        if (confettiPieces == null)
        {
            confettiPieces = new List<ConfettiPiece>();
        }

        confettiPieces.Clear();
        Random random = new Random();
        int initialCount = 50; // Kevesebb konfetti darab, de folyamatosan jelennek meg

        for (int i = 0; i < initialCount; i++)
        {
            confettiPieces.Add(new ConfettiPiece
            {
                X = random.Next(0, (int)canvasViewKonfetti.CanvasSize.Width),
                Y = random.Next(0, (int)canvasViewKonfetti.CanvasSize.Height),
                Color = SKColor.FromHsl(random.Next(0, 360), 100, 50),
                Size = random.Next(10, 20), // Nagyobb konfettik
                SpeedY = random.Next(3, 6), // Lassabb konfettik
                SpeedX = random.Next(-2, 2) // Vastagabb konfettik
            });
        }

        animationTimer.Start();
    }






    private void OnAnimationTimerElapsed(object sender, ElapsedEventArgs e)
    {
        Task.Run(() =>
        {
            Random random = new Random();
            List<ConfettiPiece> piecesToRemove = new List<ConfettiPiece>();

            // Új konfetti darabok hozzáadása
            if (confettiPieces.Count < 85) // Maximum 100 darab konfetti egyszerre
            {
                confettiPieces.Add(new ConfettiPiece
                {
                    X = random.Next(0, (int)canvasViewKonfetti.CanvasSize.Width),
                    Y = random.Next(0, (int)canvasViewKonfetti.CanvasSize.Height),
                    Color = SKColor.FromHsl(random.Next(0, 360), 100, 50),
                    SpeedY = random.Next(3, 6), // Lassabb konfettik
                    SpeedX = random.Next(-2, 2),
                    Size = random.Next(10, 20)
                });
            }

            for (int i = 0; i < confettiPieces.Count; i++)
            {
                confettiPieces[i].X += confettiPieces[i].SpeedX;
                confettiPieces[i].Y += confettiPieces[i].SpeedY;

                if (confettiPieces[i].Y > canvasViewKonfetti.CanvasSize.Height)
                {
                    confettiPieces[i].Y = 0;
                    confettiPieces[i].X = random.Next(0, (int)canvasViewKonfetti.CanvasSize.Width);
                }
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                canvasViewKonfetti.InvalidateSurface();
            });
        });
    }








    private void OnCanvasViewPaintSurface2(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (confettiPieces != null)
        {
            foreach (var piece in confettiPieces.ToList()) // Másolatot készítünk a biztonságos bejáráshoz
            {
                if (piece != null) // Ellenőrizzük, hogy a piece nem null
                {
                    using (var paint = new SKPaint { Color = piece.Color, Style = SKPaintStyle.Fill })
                    {
                        canvas.DrawRect(piece.X, piece.Y, piece.Size, piece.Size, paint);
                    }
                }
            }
        }
    }





    private class ConfettiPiece
    {
        public float X { get; set; }
        public float Y { get; set; }
        public SKColor Color { get; set; }
        public float SpeedX { get; set; }
        public float SpeedY { get; set; }
        public float Size { get; set; }
    }



    #endregion

    private void OnButtonClicked(object sender, EventArgs e)
    {
        int difficulty = 0;

        if (sender is Button button)
        {
            switch (button.Text)
            {
                case var text when text == Nyelvbeallitas["easy"]:
                    difficulty = 1;
                    break;
                case var text when text == Nyelvbeallitas["medium"]:
                    difficulty = 2;
                    break;
                case var text when text == Nyelvbeallitas["hard"]:
                    difficulty = 3;
                    break;
            }
        }

        // Itt hívd meg a függvényt a difficulty paraméterrel
        HandleDifficulty(difficulty);
    }

    private void HandleDifficulty(int difficulty)
    {
        canvasViewKonfetti.IsVisible = false;
        // Implementáld a logikát itt
        DisplayAlert("Kiválasztott nehézség", $"A kiválasztott nehézség: {difficulty}", "OK");

        CongratulationPanel.IsVisible = false;
        Options.IsVisible = true;
    }

    #endregion


    #region kozos
    public async void Nyelvvaltas()
    {
        Languages nyelvekLekeres;
        nyelvekLekeres = new Languages(UserDatas.profileData.Language);
        Nyelvbeallitas = nyelvekLekeres.MyLanguage;
        WarmUpButton.Text = Nyelvbeallitas["bemelegites"];
        StretchButton.Text = Nyelvbeallitas["nyujtas"];
        TrainStartButton.Text = Nyelvbeallitas["start"];
        workLabel.Text = Nyelvbeallitas["WORK"];
        pluszIdo.Text = "+10" + Nyelvbeallitas["pluszIdo"];
        pluszIdo2.Text = "+10" + Nyelvbeallitas["pluszIdo"];
        koevtkezoGyakorlat.Text = Nyelvbeallitas["kovetkezo"];
        koevtkezoGyakorlat2.Text = Nyelvbeallitas["kovetkezo"];
        phenesLabel.Text = Nyelvbeallitas["pihenes"];
        shotDetailLabel.Text = Nyelvbeallitas["shortDetail"];
        easyButton.Text = Nyelvbeallitas["easy"];
        mediumButton.Text = Nyelvbeallitas["medium"];
        hardButton.Text = Nyelvbeallitas["hard"];
        CongratulationLabel.Text = Nyelvbeallitas["gratulalas"];


    }

    #endregion

}
