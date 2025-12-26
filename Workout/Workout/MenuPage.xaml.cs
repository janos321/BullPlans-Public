using CommunityToolkit.Mvvm.Messaging;
using FFImageLoading.Helpers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Maui.Layouts;
using SkiaSharp;
using System.Windows.Input;
using Workout.Messages;
using Workout.Properties.class_interfaces.Main;
using Workout.Properties.class_interfaces.Other;
using Workout.Properties.Services.Accessories;
using Workout.Properties.Services.Main;
using Workout.Properties.Services.Other;
using Workout.Properties.Services.Other_Services;

namespace Workout;

public partial class MenuPage : ContentPage
{
    #region változok és kezdetiLépés

    public Dictionary<string, string> Nyelvbeallitas;

    private int jelenlegihonap = 0;
    private int jelenlegiev = 0;
    private Button elozobutton = null;
    private Color elozoButtonColor = null;

    public static MenuPage menuPages = null;
    public static Train train = null;

    private Dictionary<string, List<string>> trainersFiles = null;

    private Dictionary<string, List<Offer>> offers;
    public static Dictionary<string, ImageSource> trainersProfilePic = new Dictionary<string, ImageSource>();

    private ImageSource _profileImageSource;
    public ImageSource ProfileImageSource
    {
        get => _profileImageSource;
        set
        {
            if (_profileImageSource != value)
            {
                _profileImageSource = value;
                OnPropertyChanged();
            }
        }
    }

    private readonly UserService _userService;
    private readonly MainFajlService _mainFajlService;
    private readonly MotivationService _motivationService;
    private readonly ProfilePicService _profilePicService;
    private readonly OtherFajlService _fajlService;
    private readonly MessagesService _messagesService;
    private readonly OfferService _offerService;
    private readonly CustomerService _costumerService;
    private readonly ProfilePicGetUpload _profilePicGetUpload;

    // Parancsok definiálása
    public ICommand CalendarNavCommand { get; }
    public ICommand StatsNavCommand { get; }
    public ICommand HomeNavCommand { get; }
    public ICommand ProfileNavCommand { get; }
    public ICommand MessageNavCommand { get; }

    public MenuPage(
        UserService userService,
        MainFajlService mainFajlService,
        MotivationService motivationService,
        ProfilePicService profilePicService,
        OtherFajlService fajlService,
        MessagesService messagesService,
        OfferService offerService,
        CustomerService costumerService)
    {
        _userService = userService;
        _mainFajlService = mainFajlService;
        _motivationService = motivationService;
        _profilePicService = profilePicService;
        _fajlService = fajlService;
        _messagesService = messagesService;
        _offerService = offerService;
        _costumerService = costumerService;
        _profilePicGetUpload = new ProfilePicGetUpload(profilePicService);

        InitializeComponent();
        Adataokhelyreallitas();

        HomeNavCommand = new Command(ShowHomeView);
        CalendarNavCommand = new Command(ShowCalendarView);
        StatsNavCommand = new Command(ShowStatsView);
        ProfileNavCommand = new Command(ShowProfileView);
        MessageNavCommand = new Command(ShowMessageView);

        // Fontos: A BindingContext beállítása
        this.BindingContext = this;

        // Feliratkozás a menü elrejtésére (pl. chat oldalról)
        WeakReferenceMessenger.Default.Register<ToggleNavMenuVisibilityMessage>(this, (r, m) =>
        {
            MyBottomNav.IsVisible = m.Value;
        });
    }

    private async void Adataokhelyreallitas()
    {

        train = ServiceHelper.GetService<Train>();

        menuPages = this;

        if (Application.Current.MainPage is AppShell shell)
        {
            // Létrehoz egy új MenuPage példányt és beállítja
            shell.SetMainMenuPage(train, "Train" + MainPage.spawnPageCounter, "");
        }

        ProfilName.Text = UserDatas.UserName;
        ProfilEmail.Text = UserDatas.Email;
        ProfilDate.Text = UserDatas.Date;
        MotivationSzoveg.Text = await _motivationService.GetMotivation(UserDatas.profileData.Language);
        ShowHomeView();


        //Shop feltöltése az ajánlatokka
        offers = await _offerService.GetOffers();
        //ProfileKep letoltesek
        if (!offers.IsNullOrEmpty())
        {
            foreach (var email in offers.Keys)
            {
                if (!email.IsNullOrEmpty())
                {
                    var result = await _profilePicService.DownloadProfilePic(email);
                    if (result != null) {
                        trainersProfilePic[email] = result.Value.Image;
                    }
                }
            }

        }
        ShopTermekek.Clear();
        foreach (var item in offers)
        {
            foreach (Offer offer in item.Value)
            {
                CreateNewFrame(item.Key, offer);
            }
        }

        Nyelvvaltas(true);
        presentLevelSize = 0;
        changeLevelSize(UserDatas.profileData.Level, true);

        if (!UserDatas.profileData.DailyRewardCheck())
        {
            OnRewardTapped(rewardFrame, null);
        }

        if (UserDatas.validData.eligibleMainTerv)
        {
            ValidData validData = await _userService.GetUserValidData(UserDatas.Email);
            if (!validData.eligibleMainTerv)
            {
                UserDatas.validData = validData;
            }
        }

        ProfileImageSource = await _profilePicGetUpload.LoadProfileImage() ?? ImageSource.FromFile("profile.png");

        trainersFiles = await Task.Run(() => _fajlService.GetFajl());

        await MessageView.GetConversation();

        await _mainFajlService.WriteMainFile();
    }


    #endregion

    #region navigacioResz
    private void ShowHomeView()
    {
        SwitchView(HomeView);
    }

    private void ShowCalendarView()
    {
        SwitchView(CalendarView);
        Hetre(null, null);
    }

    private void ShowStatsView()
    {
        SwitchView(staticView);
    }

    private void ShowProfileView()
    {
        SwitchView(ProfilView);
        NyelvOpciok.IsVisible = false;
        ProfileData.IsVisible = true;
    }

    private void ShowMessageView()
    {
        SwitchView(MessageView);
    }

    private void SwitchView(ContentView viewToShow)
    {
        HomeView.IsVisible = false;
        CalendarView.IsVisible = false;
        staticView.IsVisible = false;
        ProfilView.IsVisible = false;
        MessageView.IsVisible = false;

        viewToShow.IsVisible = true;
    }


    #endregion

    #region NaptarAblak

    private async void Honapra(object sender, EventArgs e)
    {
        HetTablazat.IsVisible = false;
        Naptar.IsVisible = true;
        NaptarMozgato.IsVisible = true;
        MonthlyViewButton.Style = (Style)this.Resources["ActiveDayButtonStyle"];
        WeeklyViewButton.Style = (Style)this.Resources["CalendarButtonStyle"];
        await GenerateCalendarAsync(DateTime.Now.Year, DateTime.Now.Month);

    }

    private void Hetre(object sender, EventArgs e)
    {
        HetTablazat.IsVisible = true;
        Naptar.IsVisible = false;
        NaptarMozgato.IsVisible = false;
        MonthlyViewButton.Style = (Style)this.Resources["CalendarButtonStyle"];
        WeeklyViewButton.Style = (Style)this.Resources["ActiveDayButtonStyle"];
        GenerateWeek();
    }

    private Dictionary<DateTime, string> napok = new Dictionary<DateTime, string>();
    private DateTime jelenlegiAtrakosGombosDatum;
    private int nemErtemHogyMiertHibasDeHaHaromszorMeghivomAFuggvenytAkkorRakjaBeRendesenAGombokat = 0;
    private async void EdzesNapAtrakasa(object sender, EventArgs e)
    {
        edzesLeirasa.IsVisible = false;
        atrakasGombokTarolo.Clear();
        foreach (var nap in napok)
        {
            var button = new Button
            {
                Text = nap.Value,
                Style = (Style)this.Resources["TransferButtonStyle"]
            };

            button.Clicked += (s, args) =>
            {
                UserDatas.validData.trainingDays[nap.Key] = UserDatas.validData.trainingDays[jelenlegiAtrakosGombosDatum];
                UserDatas.validData.trainingDays[jelenlegiAtrakosGombosDatum] = new TrainingDay();
                FajlSave();
                GenerateWeek();
            };

            atrakasGombokTarolo.Children.Add(button);
        }

        Atrakasgombok.IsVisible = true;
        atrakasEdzesTerv.IsVisible = false;
        if (nemErtemHogyMiertHibasDeHaHaromszorMeghivomAFuggvenytAkkorRakjaBeRendesenAGombokat < 3)
        {
            nemErtemHogyMiertHibasDeHaHaromszorMeghivomAFuggvenytAkkorRakjaBeRendesenAGombokat++;
            EdzesNapAtrakasa(sender, e);
        }
    }

    private async void FajlSave()
    {
        await _mainFajlService.WriteMainFile();
    }

    public void GenerateWeek()
    {
        HetGrid.Children.Clear(); // Töröljük a korábbi napokat
        // A hét napjainak nevei
        napok = new Dictionary<DateTime, string>();
        string[] dayNames;
        if (UserDatas.profileData.Language == Constans.LanguageName.En)
        {
            // Az aktuális kultúra angol nyelvű, használjuk az angol napneveket
            dayNames = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        }
        else if (UserDatas.profileData.Language == Constans.LanguageName.Hu)
        {
            // Az aktuális kultúra magyar nyelvű, használjuk a magyar napneveket
            dayNames = new string[] { "H", "K", "Sze", "Cs", "P", "Szo", "V" };
        }
        else
        {
            // Ha az aktuális kultúra nem támogatott, akkor használjuk az angol napneveket
            dayNames = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        }

        DateTime today = DateTime.Today;
        for (int i = 0; i < dayNames.Length; i++)
        {
            DateTime currentDate = StartOfWeek(today, DayOfWeek.Monday).AddDays(i);
            Style style;

            if (UserDatas.validData.trainingDays.ContainsKey(currentDate) &&
                UserDatas.validData.trainingDays[currentDate].summery != "")
            {
                style = currentDate.Date == today ? (Style)this.Resources["TrainWeekTodayButtonStyle"] :
                        UserDatas.validData.trainingDays[currentDate].finish != 0 ? (Style)this.Resources["InactiveTrainDayButtonStyle"] :
                        (Style)this.Resources["TrainWeekDayButton"];
            }
            else
            {
                style = currentDate.Date == today ? (Style)this.Resources["WeekTodayButtonStyle"] :
                        (Style)this.Resources["WeekDayButton"];

                if (currentDate.Date >= today)
                {
                    napok[currentDate.Date] = dayNames[i];
                }
            }

            var button = new Button
            {
                Text = dayNames[i],
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Style = style,
                CommandParameter = currentDate
            };

            // Eseménykezelő hozzáadása a gombokhoz
            button.Clicked += (sender, e) =>
            {
                DayButton_Clicked(sender, e, button);

                if (UserDatas.validData.trainingDays.ContainsKey(currentDate.Date) &&
                    UserDatas.validData.trainingDays[currentDate.Date].summery != "" &&
                    UserDatas.validData.trainingDays[currentDate.Date].finish == 0)
                {
                    atrakasEdzesTerv.IsVisible = true;
                    Atrakasgombok.IsVisible = false;
                    edzesLeirasa.IsVisible = true;
                    jelenlegiAtrakosGombosDatum = currentDate.Date;
                }
                else
                {
                    atrakasEdzesTerv.IsVisible = false;
                    Atrakasgombok.IsVisible = false;
                    edzesLeirasa.IsVisible = true;
                }
            };

            // Helyezd a gombot a megfelelő oszlopba, az első sorban (soronkénti napnevek)
            Grid.SetColumn(button, i);
            Grid.SetRow(button, 1); // A gombok mindig az első sorban vannak

            HetGrid.Children.Add(button);

            // Ha a mai nap, akkor programmatically trigger the Clicked event
            if (currentDate.Date == today)
            {
                button.SendClicked();
            }
        }
    }



    private DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }


    public async Task GenerateCalendarAsync(int year, int month)
    {
        NaptarGrid.Children.Clear(); // Töröljük a korábbi napokat
        atrakasEdzesTerv.IsVisible = false;
        jelenlegihonap = month;
        jelenlegiev = year;

        string[] dayNames;
        string monthName;
        string[] monthNamesEnglish = new string[]
        {
        "January", "February", "March", "April", "May", "June", "July",
        "August", "September", "October", "November", "December"
        };
        string[] monthNamesMagyar = new string[]
        {
        "január", "február", "március", "április", "május", "június", "július",
        "augusztus", "szeptember", "október", "november", "december"
        };
        if (UserDatas.profileData.Language == Constans.LanguageName.En)
        {
            monthName = monthNamesEnglish[month - 1];
            dayNames = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        }
        else if (UserDatas.profileData.Language == Constans.LanguageName.Hu)
        {
            monthName = monthNamesMagyar[month - 1];
            dayNames = new string[] { "H", "K", "Sze", "Cs", "P", "Szo", "V" };
        }
        else
        {
            monthName = month.ToString();
            dayNames = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        }

        var yearMonthLabel = new Label
        {
            Text = $"{year} {monthName}",
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            FontAttributes = FontAttributes.Bold,
            FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
        };

        Grid.SetColumnSpan(yearMonthLabel, 7);
        Grid.SetRow(yearMonthLabel, 0);
        NaptarGrid.Children.Add(yearMonthLabel);

        for (int i = 0; i < dayNames.Length; i++)
        {
            var dayNameLabel = new Label
            {
                Text = dayNames[i],
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            Grid.SetColumn(dayNameLabel, i);
            Grid.SetRow(dayNameLabel, 1);
            NaptarGrid.Children.Add(dayNameLabel);
        }

        await Task.Run(() =>
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var firstDayOfMonth = new DateTime(year, month, 1);
            var startingDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            int previousMonthDays = startingDayOfWeek == 0 ? 6 : startingDayOfWeek - 1;
            var currentDate = firstDayOfMonth.AddDays(-previousMonthDays);
            int totalDays = daysInMonth + previousMonthDays;
            totalDays += 7 - totalDays % 7;

            var buttons = new List<Button>();

            for (int day = 0; day < totalDays; day++)
            {
                int row = day / 7 + 2;
                int column = day % 7;

                var style = currentDate.Date == DateTime.Today ? (Style)this.Resources["TodayButtonStyle"] :
                            currentDate.Month == month ? (Style)this.Resources["DayButtonStyle"] :
                            (Style)this.Resources["InactiveDayButtonStyle"];

                if (UserDatas.validData.trainingDays.ContainsKey(currentDate.Date) &&
                    UserDatas.validData.trainingDays[currentDate.Date].summery != "")
                {
                    style = currentDate.Date == DateTime.Today ? (Style)this.Resources["TodayTrainDayButtonStyle"] :
                            UserDatas.validData.trainingDays[currentDate].finish != 0 ? (Style)this.Resources["InactiveTrainDayButtonStyle"] :
                            (Style)this.Resources["TrainDayButtonStyle"];
                }

                var button = new Button
                {
                    Text = currentDate.Day.ToString(),
                    Style = style,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    CommandParameter = currentDate.Date
                };



                button.Clicked += (sender, e) =>
                {
                    DayButton_Clicked(sender, e, button);
                };

                buttons.Add(button);

                currentDate = currentDate.AddDays(1);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var button in buttons)
                {
                    int row = (buttons.IndexOf(button)) / 7 + 2;
                    int column = (buttons.IndexOf(button)) % 7;
                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, column);
                    NaptarGrid.Children.Add(button);
                }
            });
        });
    }

    private async void bal(object sender, EventArgs e)
    {
        int Year = jelenlegiev;
        int Month = jelenlegihonap;
        if (Month == 1)
        {
            Month = 12;
            Year--;
        }
        else
        {
            Month--;
        }
        await GenerateCalendarAsync(Year, Month);
    }

    private async void jobb(object sender, EventArgs e)
    {
        int Year = jelenlegiev;
        int Month = jelenlegihonap;
        if (Month == 12)
        {
            Month = 1;
            Year++;
        }
        else
        {
            Month++;
        }
        await GenerateCalendarAsync(Year, Month);
    }



    private void DayButton_Clicked(object sender, EventArgs e, Button button)
    {
        if (button.Style != (Style)this.Resources["InactiveDayButtonStyle"])
        {
            var clickedButton = (Button)sender;
            var clickedDate = (DateTime)clickedButton.CommandParameter;
            if (elozobutton != null)
            {
                elozobutton.BackgroundColor = elozoButtonColor;
            }
            elozobutton = button;
            elozoButtonColor = button.BackgroundColor;
            button.BackgroundColor = Color.FromRgb(255, 192, 192);

            DateTime megfeleloNap = clickedDate;

            DatumLabel.Text = megfeleloNap.ToString("yyyy. MMMM dd.");
            if (button.Style != (Style)this.Resources["InactiveTrainDayButtonStyle"])
            {
                if (UserDatas.validData.trainingDays.ContainsKey(megfeleloNap) && UserDatas.validData.trainingDays[megfeleloNap].summery != "")
                {
                    OsszefoglaloLabel.Text = UserDatas.validData.trainingDays[megfeleloNap].summery;
                }
                else
                {
                    OsszefoglaloLabel.Text = Nyelvbeallitas["noEdzes"];
                }
            }
            else
            {
                OsszefoglaloLabel.Text = Nyelvbeallitas["edzesDone"];
            }
        }
    }


    #endregion

    #region ProfileAblak

    private async void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        NyelvOpciok.IsVisible = !NyelvOpciok.IsVisible;
        ProfileData.IsVisible = !ProfileData.IsVisible;
    }
    private async void OnOptionSelected(object sender, EventArgs e)
    {
        var button = sender as Button;
        await button.ScaleTo(1.1, 100);
        await button.ScaleTo(1.0, 100);
        UserDatas.profileData.SetLanguage(button.Text);
        Nyelvvaltas(false);

        languageButton.Text = button.Text;
        NyelvOpciok.IsVisible = false;
        ProfileData.IsVisible = true;
    }
    private async void Logout(object sender, EventArgs e)
    {
        StartRotatingImage();
        await Task.Delay(500);
        var boolResult = await Task.Run(async () => await _userService.LogoutUser(UserDatas.Email, UserDatas.profileData));

        if (boolResult)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                _profilePicGetUpload.KepTorles();
                _mainFajlService.DeleteFile();

                var menuPage = ServiceHelper.GetService<MainPage>(); // Létrehoz egy új MenuPage példányt
                MainPage.spawnPageCounter++;
                if (Application.Current.MainPage is AppShell shell)
                {
                    // Létrehoz egy új MenuPage példányt és beállítja
                    shell.SetMainMenuPage(menuPage, "MainPage" + MainPage.spawnPageCounter, "Bull Plans");
                }

                await Shell.Current.GoToAsync("///MainPage" + MainPage.spawnPageCounter);


                //await Navigation.(menuPage);
            });
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StopRotatingImage();
        });
    }

    private async void OnAddPhotoAddTapped(object sender, EventArgs e)
    {
        uploadResponse? result = await _profilePicGetUpload.PickAndUploadPhotoAsync();
        if (result != null)
        {
            if (result?.error != "" || result.image == null)
            {
                await DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas[result?.error], "OK");
            }
            else
            {
                ProfileImageSource = result.image;
            }
        }
    }


    #endregion

    #region FoMenuAblak

    private async void OnMaiEdzestervTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///Train" + MainPage.spawnPageCounter);
    }

    private void OnMaiEtrendTapped(object sender, EventArgs e)
    {

    }
    #region LevelSize

    public static int presentLevelSize = 0;

    private async void LevelShow(object sender, EventArgs e)//ha rákatintok a levelre akkor megjelnik az a level mutató ablak
    {
        // Itt hívja meg a kívánt funkciót
        levelsLayout.IsVisible = !levelsLayout.IsVisible;
        FoMenu.IsVisible = !FoMenu.IsVisible;
        MyBottomNav.IsVisible = !MyBottomNav.IsVisible;
    }

    public async static void changeLevelSize(int levelSize, bool kezdes)//level növekedés
    {
        presentLevelSize += levelSize;
        menuPages.LevelLabelShow.Text = ((int)(presentLevelSize / 100)).ToString() + (UserDatas.profileData.Language == Constans.LanguageName.En ? " level" : " szint");
        menuPages.progressBar.WidthRequest = ((int)(presentLevelSize % 100)) * 1.2;
        menuPages.canvasView.InvalidateSurface();
        UserDatas.profileData.SetLevel(presentLevelSize);
        if (!kezdes)
        {
            await new MainFajlService().WriteMainFile();
        }
    }

    private async void OnRewardTapped(object sender, EventArgs e) //ha rákatintok a motivácios szövegre akkor ez a függvény hivodik meg
    {
        Frame frame = sender as Frame;

        if (frame.IsEnabled)
        {
            // Animáció a kattintásra
            await frame.ScaleTo(1.1, 100);  // Növeljük a méretet
            await frame.ScaleTo(1.0, 100);  // Visszaállítjuk

            // Változtasd meg a háttérszínt, hogy kevésbé legyen feltűnő
            frame.BackgroundColor = Color.FromHex("#D3D3D3"); // Világosszürke

            // Tiltjuk a Frame-et, hogy ne legyen újra kattintható
            frame.IsEnabled = false;

            // Animáció letiltása
            frame.GestureRecognizers.Clear();

            // További logika hívása
            if (UserDatas.profileData.DailyRewardCheck())
            {
                UserDatas.profileData.setDailyReward();
                changeLevelSize(15, false);
            }
        }
    }


    private void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e) //megcsinálja a szintmutatónál azt a vonalszintett bal oldalon
    {
        var info = e.Info;
        var surface = e.Surface;
        var canvas = surface.Canvas;
        var totalUnits = 10000f; // Összesen 10000 egység
        var unitsPerLevel = 100; // Minden level 100 egység
        var currentUnits = presentLevelSize; // Példa aktuális pontszám

        canvas.Clear(SKColors.Transparent); // Átlátszó háttér

        float x = info.Width / 2 + 25; // Vonal középre helyezése, jobbra eltolva
        float startY = 0; // Vonal kezdő y koordinátája
        float endY = info.Height; // Vonal vég y koordinátája
        float levelHeight = info.Height / 100; // Körök távolsága egymástól

        // Vonal rajzolása
        var linePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Gray,
            StrokeWidth = 40 // Vonal vastagsága
        };
        var lineLevel = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Red,
            StrokeWidth = 40 // Vonal vastagsága
        };

        // Piros töltés rajzolása
        var levelCircle = new SKPaint
        {
            Style = SKPaintStyle.Fill, // Kitöltési stílus
            Color = SKColors.Red
        };
        var levelCircleBorder = new SKPaint
        {
            Style = SKPaintStyle.Fill, // Kitöltési stílus
            Color = SKColors.LightCoral
        };

        var textPaint = new SKPaint
        {
            Color = SKColors.Black, // Szöveg színe
            TextSize = 40, // Szöveg mérete
            IsAntialias = true, // Élsimítás bekapcsolása
            TextAlign = SKTextAlign.Center // Szöveg középre igazítása
        };

        // A progressz vonal megrajzolása
        canvas.DrawLine(x, startY, x, endY, linePaint);
        canvas.DrawLine(x, startY, x, currentUnits * endY / totalUnits, lineLevel);

        // Körök és progressz vonal rajzolása
        for (int i = 1; i <= 100; i++)
        {
            float y = i * levelHeight;
            float circleRadius = i % 2 == 0 ? 60 : 35;
            textPaint.TextSize = i % 2 == 0 ? 60 : 40;
            levelCircle.Color = currentUnits >= i * unitsPerLevel ? SKColors.Red : SKColors.Gray;
            levelCircleBorder.Color = currentUnits >= i * unitsPerLevel ? SKColors.LightCoral : SKColors.White;
            var colors = new SKColor[] { currentUnits >= i * unitsPerLevel ? SKColors.LightCoral : SKColors.Gray, SKColors.White }; var shader = SKShader.CreateLinearGradient(new SKPoint(x, y - circleRadius), new SKPoint(x, y + circleRadius), colors, new float[] { 0, 1 }, SKShaderTileMode.Clamp);
            levelCircleBorder.Shader = shader;
            // Először a szürke alap körök rajzolása
            canvas.DrawCircle(x, y, circleRadius + 10, levelCircleBorder);

            canvas.DrawCircle(x, y, circleRadius, levelCircle);

            canvas.DrawText(i.ToString(), x, y + (textPaint.TextSize / 2 - 4), textPaint);
        }
    }
    private void OnCanvasViewTextPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)//megcsinálja a level vonal mellé irt szöveget ami a jutalmat jelzi
    {
        var info = e.Info;
        var surface = e.Surface;
        var canvas = surface.Canvas;
        // Itt kezeld a szövegek rajzolását
        string rewardText = "Ezen a szinten kapsz egy jutalmat";

        float x = 0; // Vonal középre helyezése, jobbra eltolva
        float levelHeight = info.Height / 100; // Körök távolsága egymástól
        var totalUnits = 10000f; // Összesen 10000 egység
        var unitsPerLevel = 100; // Minden level 100 egység
        var currentUnits = presentLevelSize; // Példa aktuális pontszám

        var rewardTextPaint = new SKPaint
        {
            Color = SKColors.White, // Szöveg színe fehér
            TextSize = 50, // Szöveg mérete
            IsAntialias = true, // Élsimítás bekapcsolása
            TextAlign = SKTextAlign.Left // Szöveg balról kezdődik
        };

        for (int i = 2; i <= 100; i += 2)
        {
            float y = i * levelHeight;
            rewardTextPaint.Color = currentUnits >= i * unitsPerLevel ? SKColors.Red : SKColors.White;
            canvas.DrawText(rewardText, x, y + (rewardTextPaint.TextSize / 2 - 3), rewardTextPaint);

        }

    }
    #endregion


    #region EdzesTervKeres
    private void EdzesTerKeresTapped(object sender, EventArgs e)
    {
        ShopLayout.IsVisible = !ShopLayout.IsVisible;
        FoMenu.IsVisible = !FoMenu.IsVisible;
        MyBottomNav.IsVisible = !MyBottomNav.IsVisible;
    }
    private void CreateNewFrame(string email, Offer ajanlat)
    {
        QuestionData offer = ajanlat.mainData;
        // Main StackLayout to contain the profile image and the Content frame
        var mainStackLayout = new StackLayout
        {
            Padding = new Thickness(0),
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start
        };

        // Create the profile image (circular shape)
        var profileImageFrame = new Frame
        {
            Padding = 0,
            HeightRequest = 150,
            WidthRequest = 150,
            CornerRadius = 75, // Circular shape
            IsClippedToBounds = true,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            ZIndex = 5,
            Margin = new Thickness(0, 0, 0, -90) // Negative margin to overlap with the Content frame
        };

        var profileImage = new Image
        {
            Source = trainersProfilePic.ContainsKey(email) ? trainersProfilePic[email] : ImageSource.FromFile(Constans.basicProfilePic), // Replace with your actual image
            HeightRequest = 150,
            WidthRequest = 150,
            Aspect = Aspect.AspectFill
        };
        profileImageFrame.Content = profileImage;

        // Create the Content frame with a fixed width
        var contentFrame = new Frame
        {
            BackgroundColor = Color.FromHex("#FFFFFF"), // White
            Padding = new Thickness(10, 80, 10, 10), // Add top padding to account for the profile image overlap
            CornerRadius = 20,
            Margin = new Thickness(10),
            WidthRequest = 350, // Set fixed width for the frame
            IsEnabled = true,
            BorderColor = Color.FromRgb(255, 0, 0), // Red border
            HasShadow = true,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Radius = 10,
                Offset = new Point(5, 5)
            }
        };

        // StackLayout for the Content inside the frame (username, name, price, tags)
        var contentStack = new StackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start
        };
        Console.WriteLine(offer.userName);
        // Username Label
        var userNameLabel = new Label
        {
            Text = offer.userName,
            FontAttributes = FontAttributes.Bold,
            FontSize = 25,
            TextColor = Color.FromRgb(0, 0, 0),
            HorizontalOptions = LayoutOptions.Center
        };

        // Name Label
        var nameLabel = new Label
        {
            Text = offer.name,
            FontSize = 20,
            TextColor = Color.FromRgb(0, 0, 0),
            HorizontalOptions = LayoutOptions.Center
        };

        // Price Label
        var priceLabel = new Label
        {
            Text = $"{offer.price}",
            FontSize = 20,
            TextColor = Color.FromRgb(0, 0, 0),
            HorizontalOptions = LayoutOptions.Center
        };

        // FlexLayout for the tags to wrap if there's not enough space
        var tagsLayout = new FlexLayout
        {
            Wrap = FlexWrap.Wrap, // Wrap tags to the next line if needed
            AlignItems = FlexAlignItems.Center, // Align items vertically to the center
            JustifyContent = FlexJustify.Center, // Center tags horizontally
            HorizontalOptions = LayoutOptions.FillAndExpand
        };


        foreach (var tag in offer.tags)
        {
            var tagFrame = new Frame
            {
                Padding = new Thickness(5, 5),
                CornerRadius = 20,
                BorderColor = Color.FromHex("#CD5C5C"), // IndianRed border
                BackgroundColor = Color.FromHex("#FFE4E1"), // Light Red Background
                HasShadow = false,
                Margin = new Thickness(2, 2, 2, 2) // Add margin around each tag
            };

            var tagLabel = new Label
            {
                Text = tag.name,
                TextColor = Color.FromHex("#CD5C5C"), // IndianRed text
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            tagFrame.Content = tagLabel;
            tagsLayout.Children.Add(tagFrame);
        }

        // Add labels and tag layout to the Content stack
        contentStack.Children.Add(userNameLabel);
        contentStack.Children.Add(nameLabel);
        contentStack.Children.Add(priceLabel);
        contentStack.Children.Add(tagsLayout);

        // "Érdekel" button
        var erdekelButton = new Button
        {
            Text = "Érdekel",
            BackgroundColor = Color.FromHex("#CD5C5C"), // Red color for the button
            TextColor = Color.FromHex("#FFFFFF"), // White text
            CornerRadius = 20, // Oval-shaped button
            HeightRequest = 50,
            WidthRequest = 200, // Set a fixed width for the button
            FontSize = 18, // Increase font size for visibility
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0) // Margin for spacing
        };
        erdekelButton.Clicked += (s, e) => readLongDetailsPanel(email, ajanlat); // Attach the event

        // Add the "Érdekel" button to the Content stack
        contentStack.Children.Add(erdekelButton);

        contentFrame.Content = contentStack;

        // Add the profile image and the Content frame to the main stack layout
        mainStackLayout.Children.Add(profileImageFrame);
        mainStackLayout.Children.Add(contentFrame);

        // Finally, add the mainStackLayout to the parent layout (e.g., StackLayout or Grid)
        ShopTermekek.Children.Add(mainStackLayout);
    }




    #region AdatokKereseBevitele
    private string foCimFoKerdes = "";
    private Dictionary<Entry, Label> _entryList = new Dictionary<Entry, Label>();
    private Dictionary<string, List<string>> kerdesValaszok = new Dictionary<string, List<string>>();
    private Offer presentOffer = new Offer();
    private string emailCim = "";
    private int presentSide = 0;

    #region LongDetailsThings
    private async void readLongDetailsPanel(string email, Offer offer)
    {
        ShopLayout.IsVisible = !ShopLayout.IsVisible;
        longDetailPanel.IsVisible = !longDetailPanel.IsVisible;
        NameEntry3.Text = offer.mainData.name;
        PriceEntry3.Text = offer.mainData.price;
        felhasznaloNeve3.Text = offer.mainData.userName;
        presentOffer = offer;
        profileAjanlatKep3.Source = trainersProfilePic.ContainsKey(email)&& trainersProfilePic[email]!=null ? trainersProfilePic[email] : ImageSource.FromFile(Constans.basicProfilePic);
        emailCim = email;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            FajlokMegjelenitese();
        });

        DisplayAnswers(offer.mainData.answers);
    }

    private void FajlokMegjelenitese()
    {
        // Ellenőrizzük, hogy léteznek-e fájlok az adott email címhez
        if (!trainersFiles.ContainsKey(emailCim))
        {
            trainersFiles[emailCim] = new List<string>();
            return;
        }

        List<string> filesNames = trainersFiles[emailCim];

        // Töröljük az előző tartalmat
        informationFileInOfferSettings2.Children.Clear();

        foreach (var fileName in filesNames)
        {

            // Létrehozunk egy második Frame-et és labelt a második StackLayout-hoz
            var fileTagFrame2 = new Frame
            {
                Padding = new Thickness(10, 5),
                Margin = new Thickness(10, 5, 5, 5),
                CornerRadius = 20,
                BorderColor = Color.FromHex("#CD5C5C"),
                BackgroundColor = Color.FromHex("#FFE4E1"),
                HasShadow = false,
                HeightRequest = 30,
                WidthRequest = 150
            };

            var fileTagLabel2 = new Label
            {
                Text = fileName,
                FontSize = 14,
                TextColor = Color.FromHex("#CD5C5C"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1
            };

            var tapGestureRecognizer2 = new TapGestureRecognizer();
            tapGestureRecognizer2.Tapped += (s, e) => MegnyitFajlt(fileName);
            fileTagFrame2.GestureRecognizers.Add(tapGestureRecognizer2);
            fileTagFrame2.Content = fileTagLabel2;

            // Hozzáadás a második StackLayout-hoz
            informationFileInOfferSettings2.Children.Add(fileTagFrame2);
        }

    }

    // Fájl megnyitása funkció
    private async void MegnyitFajlt(string fileName)
    {
        StartRotatingImage(); // Forgó animáció elindítása

        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {

                // PHP hívás, hogy lekérjük a fájlt másik szálon
                var fileBytes = await Task.Run(() => _fajlService.DownloadFajl(emailCim, fileName));

                if (fileBytes != null)
                {
                    var localFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                    // Fájl mentése helyileg másik szálon
                    await Task.Run(() => File.WriteAllBytes(localFilePath, fileBytes));

                    // Fájl megnyitása a készüléken
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(localFilePath)
                    });
                }
                else
                {
                    await DisplayAlert("Error", "Failed to download or open the file.", "OK");
                }
            });
        }
        finally
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StopRotatingImage(); // Forgó animáció leállítása
            });
        }
    }
    private void DisplayAnswers(Dictionary<string, string> answers)
    {
        // Clear the existing Content
        ajanlatTudniValokLongDetailSatcklayout.Children.Clear();

        // Loop through each answer and add it to the StackLayout
        foreach (var answer in answers)
        {
            // Create a frame for each answer to give a nice visual look
            var answerFrame = new Frame
            {
                Padding = new Thickness(10),
                CornerRadius = 10,
                BorderColor = Color.FromHex("#CD5C5C"), // Indian Red border color
                BackgroundColor = Color.FromHex("#FFE4E1"), // Light red background
                HasShadow = false,
                Margin = new Thickness(5) // Add some margin between answers
            };

            // StackLayout to hold the question and answer labels
            var answerStack = new StackLayout
            {
                Spacing = 5,
                Orientation = StackOrientation.Vertical
            };

            // Create a Label for the question
            var questionLabel = new Label
            {
                Text = answer.Key,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                TextColor = Color.FromHex("#CD5C5C"), // Indian Red for question text
                HorizontalOptions = LayoutOptions.Start
            };

            // Create a Label for the answer
            var answerLabel = new Label
            {
                Text = answer.Value,
                FontSize = 14,
                TextColor = Color.FromHex("#333333"), // Dark grey for answer text
                HorizontalOptions = LayoutOptions.Start
            };

            // Add the question and answer to the StackLayout
            answerStack.Children.Add(questionLabel);
            answerStack.Children.Add(answerLabel);

            // Add the StackLayout to the Frame
            answerFrame.Content = answerStack;

            // Finally, add the Frame to the main StackLayout
            ajanlatTudniValokLongDetailSatcklayout.Children.Add(answerFrame);
        }
    }
    private void BackShop(object sender, EventArgs e)
    {
        ShopLayout.IsVisible = !ShopLayout.IsVisible;
        longDetailPanel.IsVisible = !longDetailPanel.IsVisible;
    }

    #endregion

    private async void vasarlas(object sender, EventArgs e)
    {
        //Itt lesz a vásárlási mehanizmus
        UserDatas.validData.trainerEmailAddress = emailCim;
        await _messagesService.PutMessages(UserDatas.Email, new List<string> { UserDatas.validData.trainerEmailAddress }, "Hello");
        longDetailPanel.IsVisible = false;
        sideChangeStacklayout.IsVisible = true;
        presentSide = 0;
        kerdesValaszok = new Dictionary<string, List<string>>();
        _entryList = new Dictionary<Entry, Label>();
        SideMegjelnites();
    }
    private void nextSide(object sender, EventArgs e)
    {
        foreach (var entry in _entryList)
        {
            if (string.IsNullOrEmpty(entry.Key.Text))
            {
                kerdesValaszok[foCimFoKerdes].Clear();
                DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas["hibaIrjValamit"], "OK");
                return;
            }
            kerdesValaszok[foCimFoKerdes].Add(entry.Value.Text + ":" + entry.Key.Text);
        }
        if (kerdesValaszok[foCimFoKerdes].Count == 0)
        {
            DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas["hibavalaszValamit"], "OK");
            return;
        }
        presentSide++;
        SideMegjelnites();
    }
    private async void befejezve()
    {
        CustomerData adatok = new CustomerData(kerdesValaszok, true);
        await _costumerService.PutCustomers(emailCim, UserDatas.Email, adatok);
        UserDatas.validData.eligibleMainTerv = true;
        await _userService.UpdateUserValidData(UserDatas.Email, UserDatas.validData);

        sideChangeStacklayout.IsVisible = false;
        MyBottomNav.IsVisible = true;
        FoMenu.IsVisible = true;
    }


    private void SideMegjelnites()
    {
        questionPart.Children.Clear();

        if (presentOffer.questionsSide.Count <= presentSide)
        {
            befejezve();
            return;
        }

        QuestionsSide showSide = presentOffer.questionsSide[presentSide];
        _entryList = new Dictionary<Entry, Label>();
        // 1. Progresszív indikátor
        var progressIndicator = new Grid
        {
            HeightRequest = 4,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Start
        };

        var backgroundBox = new BoxView
        {
            Style = (Style)Application.Current.Resources["ProgressBackground"],
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        var progressBox = new BoxView
        {
            Style = (Style)Application.Current.Resources["ProgressForeground"],
            WidthRequest = (400.0 / presentOffer.questionsSide.Count) * (presentSide + 1),
            HorizontalOptions = LayoutOptions.Start
        };

        progressIndicator.Children.Add(backgroundBox);
        progressIndicator.Children.Add(progressBox);

        questionPart.Children.Add(progressIndicator);
        foCimFoKerdes = showSide.mainTittle + ": " + showSide.mainQuestion;
        kerdesValaszok[foCimFoKerdes] = new List<string>();
        // 2. Főcím és Főkérdés
        var titleLabel = new Label
        {
            Text = showSide.mainTittle,
            Style = (Style)Application.Current.Resources["Focim"]
        };

        var questionLabel = new Label
        {
            Text = showSide.mainQuestion,
            Style = (Style)Application.Current.Resources["Fokerdes"]
        };

        questionPart.Children.Add(titleLabel);
        questionPart.Children.Add(questionLabel);

        // 3. Kérdés típusának kezelése (Többválasztós, Egyválasztós, Beírós)
        if (showSide.sideType == "Többválasztós" || showSide.sideType == "Egyválasztós")
        {          
            Button previousButton = null;

            var buttonsGrid = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            }
            };

            for (int i = 0; i < showSide.questionList.Count; i++)
            {
                var answerButton = new Button
                {
                    Text = showSide.questionList[i],
                    Style = (Style)Application.Current.Resources["tobbvalasztosButtonStyle"]
                };
                // Többválasztós logika
                if (showSide.sideType == "Többválasztós")
                {
                    answerButton.Clicked += (s, e) =>
                    {
                        var btn = s as Button;
                        if (kerdesValaszok[foCimFoKerdes].Contains(btn.Text))
                        {
                            btn.Style = (Style)Application.Current.Resources["tobbvalasztosButtonStyle"];
                            kerdesValaszok[foCimFoKerdes].Remove(btn.Text);
                        }
                        else
                        {
                            btn.Style = (Style)Application.Current.Resources["tobbvalasztosButtonPickStyle"];
                            kerdesValaszok[foCimFoKerdes].Add(btn.Text);
                        }
                    };
                }
                // Egyválasztós logika
                else if (showSide.sideType == "Egyválasztós")
                {
                    answerButton.Clicked += (s, e) =>
                    {
                        var btn = s as Button;
                        kerdesValaszok[foCimFoKerdes].Clear();
                        kerdesValaszok[foCimFoKerdes].Add(btn.Text);

                        if (previousButton != null)
                        {
                            previousButton.Style = (Style)Application.Current.Resources["tobbvalasztosButtonStyle"];
                        }

                        btn.Style = (Style)Application.Current.Resources["tobbvalasztosButtonPickStyle"];
                        previousButton = btn;
                    };
                }

                // Grid elhelyezés logika
                int row = i / 2;
                int column = i % 2;

                if (buttonsGrid.RowDefinitions.Count <= row)
                {
                    buttonsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                Grid.SetRow(answerButton, row);
                Grid.SetColumn(answerButton, column);

                buttonsGrid.Children.Add(answerButton);
            }

            questionPart.Children.Add(buttonsGrid);
        }
        else if (showSide.sideType == "Beírós")
        {
            // 4. Beírós kérdések kezelése
            var inputStack = new StackLayout
            {
                Spacing = 10,
                Padding = new Thickness(10)
            };

            for (int i = 0; i < showSide.questionList.Count; i++)
            {
                var label = new Label
                {
                    Text = showSide.questionList[i],
                    Style = (Style)Application.Current.Resources["ElegantLabelStyle"]
                };

                var entry = new Entry
                {
                    Style = (Style)Application.Current.Resources["ElegantEntryStyle"]
                };

                var entryFrame = new Frame
                {
                    Style = (Style)Application.Current.Resources["ElegantEntryFrameStyle"],
                    Content = new StackLayout
                    {
                        Spacing = 5,
                        Children = { label, entry }
                    }
                };
                _entryList[entry] = label;
                inputStack.Children.Add(entryFrame);
            }

            questionPart.Children.Add(inputStack);
        }
    }

    #endregion

    #endregion



    #endregion

    #region kozosok
    private void StartRotatingImage()
    {
        MainGrid.IsEnabled = false;
        ForgoKerekP.IsVisible = true;
        View image = ForgoKep;
        var animation = new Animation(callback: d => image.Rotation = d,
                                      start: image.Rotation,
                                      end: image.Rotation + 360,
                                      easing: Easing.Linear);

        animation.Commit(owner: this,
                         name: "LoopingAnimation",
                         length: 2000, // Az animáció hossza 2 másodperc
                         repeat: () => true); // A repeat: () => true kifejezés azt jelenti, hogy az animáció végtelenítve van
    }

    private void StopRotatingImage()
    {
        MainGrid.IsEnabled = true;
        ForgoKerekP.IsVisible = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        this.AbortAnimation("LoopingAnimation"); // Megállítja az animációt, ha az oldal eltűnik
    }
    public async Task VanAktivHálózatiKapcsolat()
    {

        DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas["noInternet"], "OK");

    }

    public async void Nyelvvaltas(bool kezdes)
    {
        if (!kezdes)
        {
            await _mainFajlService.WriteMainFile();
        }
        languageButton.Text = UserDatas.profileData.Language;
        Languages nyelvekLekeres;
        nyelvekLekeres = new Languages(UserDatas.profileData.Language);
        Nyelvbeallitas = nyelvekLekeres.MyLanguage;
        //ProfilLabel.Text = Nyelvbeallitas["Profil"];
        //messageLabel.Text = Nyelvbeallitas["Uzenet"];
        //HomeLabel.Text = Nyelvbeallitas["Home"];
        //StatsLabel.Text = Nyelvbeallitas["Stats"];
        //CalendarLabel.Text = Nyelvbeallitas["Calendar"];
        //SendButton.Text = Nyelvbeallitas["Send"];
        // messageEntry.Placeholder = Nyelvbeallitas["uzenetIras"];
        MonthlyViewButton.Text = Nyelvbeallitas["honap"];
        WeeklyViewButton.Text = Nyelvbeallitas["het"];
        AdMainTervText.Text = Nyelvbeallitas["bolt"];
        atrakasEdzesTerv.Text = Nyelvbeallitas["edzesAtrakas"];
        maiNapiEdzes.Text = Nyelvbeallitas["maiNapi"];
        edzes.Text = Nyelvbeallitas["edzes"];
        maiNapiKaja.Text = Nyelvbeallitas["maiNapi"];
        etrend.Text = Nyelvbeallitas["etrend"];
        backBolt.Text = Nyelvbeallitas["backButton"];
        mehetBolt.Text = Nyelvbeallitas["mehet"];
        logoutButton.Text = Nyelvbeallitas["logout"];


    }
    public static DateTime DatumConvertAgeMonthDay(DateTime Basic)
    {
        DateTime yearMonthDay = new DateTime(Basic.Year, Basic.Month, Basic.Day);
        return yearMonthDay;
    }

    #endregion


}