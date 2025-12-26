namespace Workout;
using CommunityToolkit.Mvvm.Messaging;
using FFImageLoading.Helpers;
using HeyRed.Mime;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System.IO;
using System.Windows.Input;
using Workout.Messages;
using Workout.Properties.class_interfaces.Main;
using Workout.Properties.class_interfaces.Other;
using Workout.Properties.Services.Accessories;
using Workout.Properties.Services.Main;
using Workout.Properties.Services.Other;
using Workout.Properties.Services.Other_Services;

public partial class TrainerPage : ContentPage
{

    #region kezdet es valtozok
    private Dictionary<string, string> Nyelvbeallitas;
    private Dictionary<string, List<string>> trainersFiles = null;

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

    // Parancsok definiálása
    public ICommand CalendarNavCommand { get; }
    public ICommand StatsNavCommand { get; }
    public ICommand HomeNavCommand { get; }
    public ICommand ProfileNavCommand { get; }
    public ICommand MessageNavCommand { get; }

    private readonly TrainerService _trainerService;
    private readonly MainFajlService _mainFajlService;
    private readonly OtherFajlService _fajlService;
    private readonly OfferService _offerService;
    private readonly CustomerService _customerService;
    private readonly TrainingDataService _trainingDataService;
    private readonly UserService _userService;
    private readonly ProfilePicGetUpload _profilePicGetUpload;
    public TrainerPage(
        TrainerService trainerService,
        MainFajlService mainFajlService,
        ProfilePicService profileService,
        OtherFajlService fajlService,
        OfferService offerService,
        CustomerService customerService,
        TrainingDataService trainingDataService,
        UserService userService)
    {
        _trainerService = trainerService;
        _mainFajlService = mainFajlService;
        _fajlService = fajlService;
        _offerService = offerService;
        _customerService = customerService;
        _trainingDataService = trainingDataService;
        _userService = userService;
        _profilePicGetUpload = new ProfilePicGetUpload(profileService);

        InitializeComponent();
        adatokHelyreallitas();

        HomeNavCommand = new Command(ShowHomeView);
        CalendarNavCommand = new Command(ShowCalendarView);
        StatsNavCommand = new Command(ShowAjanlatView);
        ProfileNavCommand = new Command(ShowProfileView);
        MessageNavCommand = new Command(ShowMessageView);

        // Fontos: A BindingContext beállítása
        this.BindingContext = this;

        // Feliratkozás a menü elrejtésére (pl. chat oldalról)
        WeakReferenceMessenger.Default.Register<ToggleNavMenuVisibilityMessage>(this, (r, m) =>
        {
            // MyBottomNav az x:Name, amit a XAML-ben adtál a komponensnek
            MyBottomNav.IsVisible = m.Value;
        });

    }


    private async void adatokHelyreallitas()
    {
        Storage.trainingDatas = await _trainingDataService.GetTrainingData(UserDatas.Email);
        Storage.trainerOffer = await _offerService.GetTrainerOffers(UserDatas.Email);
        Storage.costumers = await _customerService.GetCustomers(UserDatas.Email);

        Nyelvvaltas(true);

        ShowHomeView();

        fajtakmegjelenites();

        ProfilName.Text = UserDatas.UserName;
        ProfilEmail.Text = UserDatas.Email;
        ProfilDate.Text = UserDatas.Date;
        felhasznaloNeve.Text = UserDatas.UserName;
        felhasznaloNeve2.Text = UserDatas.UserName;
        felhasznaloNeve3.Text = UserDatas.UserName;


        trainersFiles = await Task.Run(() => _fajlService.GetFajl());
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            FajlokMegjelenitese();
        });

        CreateAvailableTagsButtons();

        //vásárlok adatai betöltése, lekezelése, lekérése, stb
        await UpdateHomeViewAsync(Storage.costumers);
        await _mainFajlService.WriteMainFile();

        kepekBerakasa();
        traningfajtak();

        await MessageView.GetConversation();

        ProfileImageSource = await _profilePicGetUpload.LoadProfileImage() ?? ImageSource.FromFile("profile.png");
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
    }

    private void ShowAjanlatView()
    {
        fajtakmegjelenites();
        SwitchView(ajanlatokView);
    }

    private void ShowProfileView()
    {
        SwitchView(ProfilView);
    }

    private void ShowMessageView()
    {
        SwitchView(MessageView);
    }

    private void SwitchView(ContentView viewToShow)
    {
        HomeView.IsVisible = false;
        CalendarView.IsVisible = false;
        ajanlatokView.IsVisible = false;
        ProfilView.IsVisible = false;
        MessageView.IsVisible = false;

        viewToShow.IsVisible = true;
    }

    #endregion

    #region ajanlatokTervezese

    private List<QuestionsSide> questionsSides = new List<QuestionsSide>();

    #region kezdoLapon szerkesztések

    private async void fajtakmegjelenites()
    {
        Edzesajanlatok.Clear();
        offerAdButton.IsVisible = Storage.trainerOffer.Count < 3;

        foreach (var fajta in Storage.trainerOffer)
        {
            CreateNewFrame(fajta);
        }
    }
    private void CreateNewFrame(Offer ajanlat)
    {
        QuestionData fajta = ajanlat.mainData;
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
            HeightRequest = 150,
            WidthRequest = 150,
            Aspect = Aspect.AspectFill,
            BindingContext = this
        };

        profileImage.SetBinding(
            Image.SourceProperty,
            nameof(ProfileImageSource)
        );

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

        // Username Label
        var userNameLabel = new Label
        {
            Text = fajta.userName,
            FontAttributes = FontAttributes.Bold,
            FontSize = 25,
            TextColor = Color.FromRgb(0, 0, 0),
            HorizontalOptions = LayoutOptions.Center
        };

        // Name Label
        var nameLabel = new Label
        {
            Text = fajta.name,
            FontSize = 20,
            TextColor = Color.FromRgb(0, 0, 0),
            HorizontalOptions = LayoutOptions.Center
        };

        // Price Label
        var priceLabel = new Label
        {
            Text = $"{fajta.price}",
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


        foreach (var tag in fajta.tags)
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
        erdekelButton.Clicked += (s, e) => readLongDetailsPanel(fajta); // Attach the event

        // Add the "Érdekel" button to the Content stack
        contentStack.Children.Add(erdekelButton);

        // Add the Delete button (side by side with Edit button at the bottom center)
        var deleteButton = new ImageButton
        {
            Source = "ximg.png", // Add your image path here
            BackgroundColor = Colors.Transparent,
            HeightRequest = 30,
            WidthRequest = 30,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 5, 0)
        };
        deleteButton.Clicked += (sender, args) => deleteAjanlat(sender, fajta.name);

        // Add the Edit button beside the delete button
        var editButton = new ImageButton
        {
            Source = "editingicon.png", // Add your image path here
            BackgroundColor = Colors.Transparent,
            HeightRequest = 30,
            WidthRequest = 30,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(5, 10, 0, 0)
        };
        editButton.Clicked += (sender, args) => editAjanlat(sender, ajanlat);

        // Horizontal stack for the edit and delete buttons
        var buttonsLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 10, 0, 0)
        };
        buttonsLayout.Children.Add(deleteButton);
        buttonsLayout.Children.Add(editButton);

        // Add buttons layout to the Content stack
        contentStack.Children.Add(buttonsLayout);

        contentFrame.Content = contentStack;

        // Add the profile image and the Content frame to the main stack layout
        mainStackLayout.Children.Add(profileImageFrame);
        mainStackLayout.Children.Add(contentFrame);

        // Finally, add the mainStackLayout to the parent layout (e.g., StackLayout or Grid)
        Edzesajanlatok.Add(mainStackLayout);
    }


    private string editajanlat = "";
    private void editAjanlat(object sender, Offer ajanlat)
    {
        QuestionData fajta = ajanlat.mainData;
        // Egyéb mezők visszaállítása
        NameEntry.Text = fajta.name;
        NameEntry2.Text = fajta.name;
        PriceEntry2.Text = fajta.price;

        // Price és pénznem külön kezelése
        var priceParts = fajta.price.Split(' ');
        if (priceParts.Length > 0)
        {
            PriceEntry.Text = priceParts[0];
        }
        if (priceParts.Length > 1)
        {
            CurrencyPicker.SelectedItem = priceParts[1];
        }

        foreach (var tag in _availableTags)
        {
            if (fajta.tags.Any(t => t.name == tag.name))
            {
                _selectedTags.Add(tag);
            }
        }

        CreateAvailableTagsButtons();

        foreach (var input in tagQuestionInputs)
        {
            if (fajta.answers.ContainsKey(input.Key))
            {
                input.Value.Text = fajta.answers[input.Key];
            }
        }

        foreach (var input in baseQuestionInputs)
        {
            if (fajta.answers.ContainsKey(input.Key))
            {
                input.Value.Text = fajta.answers[input.Key];
            }
        }

        editajanlat = fajta.name;

        questionsSides = ajanlat.questionsSide;

        // Panel megnyitása
        ajanlatHozzaAdasaPanelMegnyitasa(null, null);
    }


    private async void deleteAjanlat(object sender, string name)
    {
        bool answer = await DisplayAlert(Nyelvbeallitas["Megerosites"], Nyelvbeallitas["biztosTorlesKerdes"] + name, Nyelvbeallitas["igen"], Nyelvbeallitas["megse"]);

        if (answer)
        {
            StartRotatingImage();
            var fajtaToRemove = Storage.trainerOffer.FirstOrDefault(fajta => fajta.mainData.name == name);
            if (fajtaToRemove != null)
            {
                Storage.trainerOffer.Remove(fajtaToRemove);
            }

            await Task.Run(async () =>
            {
                await _offerService.PostTrainerOffers(UserDatas.Email, Storage.trainerOffer);
                await _mainFajlService.WriteMainFile();
            });
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StopRotatingImage();
            });

            fajtakmegjelenites();
        }
        else
        {
            return;
        }
    }

    private void BackAjanlatok(object sender, EventArgs e)
    {
        questionsSides = new List<QuestionsSide>();
        MyBottomNav.IsVisible = true;
        kezdostackLayout.IsVisible = true;
        longDetailPanel.IsVisible = false;
        PluszAjanlatBekeres.IsVisible = false;

        NameEntry.Text = "";
        NameEntry2.Text = "";
        PriceEntry2.Text = "";
        PriceEntry.Text = "";
        CurrencyPicker.SelectedItem = null;

        _selectedTags = new List<Tag>();
        tagQuestionInputs = new Dictionary<string, Entry>();
        SelectedTagsLayout.Children.Clear();
        CreateAvailableTagsButtons();



        foreach (var input in baseQuestionInputs.Values)
        {
            input.Text = "";  // Minden kérdés válaszának törlése
        }
        tagElements = new Dictionary<string, List<View>>();
        SelectedTagsLayoutQuestions.Children.Clear();

        editajanlat = "";
    }

    private void ajanlatHozzaAdasaPanelMegnyitasa(object sender, EventArgs e)
    {
        kezdostackLayout.IsVisible = !kezdostackLayout.IsVisible;
        PluszAjanlatBekeres.IsVisible = !PluszAjanlatBekeres.IsVisible;
        MyBottomNav.IsVisible = !MyBottomNav.IsVisible;
    }
    private string ValidateForm()
    {
        if (string.IsNullOrEmpty(NameEntry.Text))
        {
            return "A név mező kitöltése kötelező.";
        }

        if (string.IsNullOrEmpty(PriceEntry2.Text) || PriceEntry.Text == "" || CurrencyPicker.SelectedItem == null)
        {
            return "Az ár meg a pénznem mező kitöltése kötelező.";
        }

        if (_selectedTags.Count < 2)
        {
            return "Legalább két címke kiválasztása szükséges.";
        }

        foreach (var question in tagQuestionInputs)
        {
            if (string.IsNullOrEmpty(question.Value.Text))
            {
                return $"Az \"{question.Key}\" kérdésre nem válaszoltál.";
            }
        }




        foreach (var question in baseQuestionInputs)
        {
            if (string.IsNullOrEmpty(question.Value.Text))
            {
                return $"Az \"{question.Key}\" kérdésre nem válaszoltál.";
            }
        }

        return string.Empty;
    }

    private async void OkAjanlathozzadas(object sender, EventArgs e)
    {
        string hiba = ValidateForm();
        if (!string.IsNullOrEmpty(hiba))
        {
            DisplayAlert(Nyelvbeallitas["Hiba"], hiba, "OK");
            return;
        }
        Dictionary<string, string> answers = new Dictionary<string, string>();

        foreach (var tuple in baseQuestionInputs)
        {

            answers[tuple.Key] = tuple.Value.Text;
        }

        // Extract answers from tagElements
        foreach (var tuple in tagQuestionInputs)
        {

            answers[tuple.Key] = tuple.Value.Text;
        }

        var adatok = new QuestionData
           (
               NameEntry.Text,
               felhasznaloNeve.Text,
               PriceEntry2.Text,
               _selectedTags,
               answers
           );

        var fajtaKeres = Storage.trainerOffer.FirstOrDefault(fajta => fajta.mainData.name == adatok.name);
        if (fajtaKeres != null && adatok.name != editajanlat)
        {
            DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas["hibaletezik"], "OK");
            return;
        }
        bool elso = true, masodik = true;
        if (editajanlat != "")
        {
            for (int i = 0; i < Storage.trainerOffer.Count; i++)
            {
                if (Storage.trainerOffer[i].mainData.name == editajanlat)
                {
                    Storage.trainerOffer[i].mainData = adatok;
                }
            }
        }
        else
        {

            Storage.trainerOffer.Add(new Offer(adatok, questionsSides));
        }
        StartRotatingImage();
        await Task.Run(async () =>
        {
            await _offerService.PostTrainerOffers(UserDatas.Email, Storage.trainerOffer);
            await _mainFajlService.WriteMainFile();
        });

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StopRotatingImage();
        });

        fajtakmegjelenites();
        BackAjanlatok(null, null);


    }
    private const int MaxTags = 5;
    private List<Tag> _availableTags = Tags(); // Példa tagek
    private List<Tag> _selectedTags = new List<Tag>();
    //private List<Frame> tagsFrames = new List<Frame>();

    private void CreateAvailableTagsButtons()
    {
        AvailableTagsLayout.Children.Clear();

        foreach (var tag in _availableTags)
        {
            // Létrehozunk egy Frame-et minden tag gomb helyett, hogy ovális keretet kapjunk
            var tagFrame = new Frame
            {
                Padding = new Thickness(5, 5),
                Margin = new Thickness(2), // Távolság a gombok között
                CornerRadius = 20, // Lekerekített sarkok
                BorderColor = _selectedTags.Contains(tag) ? Color.FromHex("#CD5C5C") : Color.FromHex("#D3D3D3"), // Szín kiválasztás szerint
                BackgroundColor = Color.FromHex("#F5F5F5"), // Halvány szürke háttér
                HasShadow = false // Árnyék nélkül
            };

            // A tag neve egy Label-ben jelenik meg a Frame-en belül
            var tagLabel = new Label
            {
                Text = tag.name,
                TextColor = _selectedTags.Contains(tag) ? Color.FromHex("#CD5C5C") : Color.FromHex("#333333"), // Szín kiválasztás szerint
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // TapGestureRecognizer, hogy kattintással választhassuk ki a taget
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => OnTagButtonClicked(tagFrame, tag);
            tagFrame.GestureRecognizers.Add(tapGestureRecognizer);

            // A Label hozzáadása a Frame-hez
            tagFrame.Content = tagLabel;

            // A Frame hozzáadása a Layout-hoz
            AvailableTagsLayout.Children.Add(tagFrame);
            if (_selectedTags.Any(t => t.name == tag.name))
            {
                UpdateSelectedTagsLayout();
                UpdateSelectedTagsLayoutQuestions(tag);
            }
        }
        OnCloseTagSelection(null, null);
        DisplayBaseQuestions();
    }

    // Dictionary to store the question and corresponding Entry
    private Dictionary<string, Entry> baseQuestionInputs = new Dictionary<string, Entry>();
    private Dictionary<string, string> alapKerdesek = BaseQuestions();

    private void DisplayBaseQuestions()
    {
        alapKerdesekStacklayout.Children.Clear();  // Clear previous items

        foreach (var kvp in alapKerdesek)  // Loop through the dictionary
        {
            var question = kvp.Key;
            var placeholder = kvp.Value;

            // Create a Frame for each base question
            var questionFrame = new Frame
            {
                Padding = new Thickness(8, 5),
                Margin = new Thickness(0, 5),
                CornerRadius = 20, // Rounded corners
                BorderColor = Color.FromHex("#CD5C5C"), // IndianRed border
                BackgroundColor = Color.FromHex("#FFE4E1"), // Light red background
                HasShadow = false,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            // Create an Entry for the question input
            var questionInput = new Entry
            {
                Placeholder = placeholder, // Placeholder text is the example
                PlaceholderColor = Color.FromHex("#AAAAAA"),
                TextColor = Color.FromHex("#000000"),
                FontSize = 14,
                BackgroundColor = Color.FromHex("#00000000"), // Transparent to match the frame
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 35 // Height should match the Entry
            };

            // Create a Label above the Entry to show the actual question text
            var questionLabel = new Label
            {
                Text = question,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14,
                TextColor = Color.FromHex("#CD5C5C"), // IndianRed for the label
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            // Add the Label (question) and Entry (input) to a StackLayout
            var inputLayout = new StackLayout
            {
                Spacing = 5,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            inputLayout.Children.Add(questionLabel);  // Add the question Label
            inputLayout.Children.Add(questionInput);  // Add the Entry inside the Frame

            // Add the input layout inside the Frame
            questionFrame.Content = inputLayout;

            // Add the Frame to the base questions layout
            alapKerdesekStacklayout.Children.Add(questionFrame);

            // Save the question and corresponding Entry to the dictionary
            baseQuestionInputs[question] = questionInput;
        }
    }




    // Tag kiválasztása vagy eltávolítása
    private void OnTagButtonClicked(Frame tagFrame, Tag tag)
    {
        if (_selectedTags.Contains(tag))
        {
            // Tag eltávolítása
            _selectedTags.Remove(tag);
            tagFrame.BorderColor = Color.FromHex("#D3D3D3"); // LightGray szegély, ha nincs kiválasztva
            ((Label)tagFrame.Content).TextColor = Color.FromHex("#333333"); // Szöveg színe visszaállítva
        }
        else if (_selectedTags.Count < MaxTags)
        {
            // Tag hozzáadása
            _selectedTags.Add(tag);
            tagFrame.BorderColor = Color.FromHex("#CD5C5C"); // IndianRed szegély, ha ki van választva
            ((Label)tagFrame.Content).TextColor = Color.FromHex("#CD5C5C"); // Szöveg színe IndianRed
        }

        UpdateSelectedTagsLayout();
        UpdateSelectedTagsLayoutQuestions(tag);
    }

    private Dictionary<string, List<View>> tagElements = new Dictionary<string, List<View>>();

    private Dictionary<string, Entry> tagQuestionInputs = new Dictionary<string, Entry>();

    private void UpdateSelectedTagsLayoutQuestions(Tag tag)
    {
        // Check if the tag's questions already exist, if so, remove them
        if (!_selectedTags.Contains(tag))
        {
            if (tagElements.ContainsKey(tag.name))
            {
                foreach (var element in tagElements[tag.name])
                {
                    SelectedTagsLayoutQuestions.Children.Remove(element);
                }

                // Remove the tag-related Entry fields from tagQuestionInputs
                foreach (var question in tag.questions)
                {
                    if (tagQuestionInputs.ContainsKey(question))
                    {
                        tagQuestionInputs.Remove(question);
                    }
                }

                // Remove the tag elements from the dictionary
                tagElements.Remove(tag.name);
            }
            return;
        }

        // If the tag is already added, do nothing
        if (tagElements.ContainsKey(tag.name))
        {
            return;
        }

        // Keep track of added elements for this tag
        var tagRelatedViews = new List<View>();

        // Create a StackLayout for the tag header with horizontal lines on both sides
        var tagHeaderLayout = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.Center,
            Children =
        {
            new BoxView
            {
                Color = Color.FromHex("#CD5C5C"), // Line color same as IndianRed
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center
            },
            new Label
            {
                Text = tag.name,
                FontAttributes = FontAttributes.Bold,
                FontSize = 18,
                TextColor = Color.FromHex("#CD5C5C"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(10, 5)
            },
            new BoxView
            {
                Color = Color.FromHex("#CD5C5C"),
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center
            }
        }
        };

        // Add the tag name as a header
        SelectedTagsLayoutQuestions.Children.Add(tagHeaderLayout);
        tagRelatedViews.Add(tagHeaderLayout);

        // Create input fields for each question under the selected tag
        for (int i = 0; i < tag.questions.Count; i++)
        {
            var question = tag.questions[i];
            var placeholder = tag.exampleAnswer[i];

            // Create a StackLayout to combine the label (above the Entry) and the Entry
            var inputLayout = new StackLayout
            {
                Spacing = 2,
                Margin = new Thickness(0, 5),
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            // Label displaying the question (above the Entry)
            var questionLabel = new Label
            {
                Text = question,
                FontAttributes = FontAttributes.Bold, // Make the question more prominent
                FontSize = 14, // Slightly larger question size
                TextColor = Color.FromHex("#555555"), // Less faint gray color for better readability
                HorizontalOptions = LayoutOptions.Start
            };

            // Create a Frame for each input field
            var questionFrame = new Frame
            {
                Padding = new Thickness(10, 5),
                CornerRadius = 20,
                BorderColor = Color.FromHex("#CD5C5C"),
                BackgroundColor = Color.FromHex("#FFE4E1"),
                HasShadow = false,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            // Create an Entry for the question input, using `exampleAnswer` as the placeholder
            var questionInput = new Entry
            {
                Placeholder = placeholder, // Set the placeholder from the `exampleAnswer` list
                PlaceholderColor = Color.FromHex("#AAAAAA"),
                TextColor = Color.FromHex("#000000"),
                FontSize = 14,
                BackgroundColor = Color.FromHex("#00000000"), // Transparent to match the frame
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 35 // Height should match the Entry
            };

            // Store the question input Entry in the dictionary
            tagQuestionInputs[question] = questionInput;

            // Add the Entry inside the Frame
            questionFrame.Content = questionInput;

            // Add the label and the Frame (question input) to the input layout
            inputLayout.Children.Add(questionLabel);
            inputLayout.Children.Add(questionFrame);

            // Add the input layout to the main layout
            SelectedTagsLayoutQuestions.Children.Add(inputLayout);
            tagRelatedViews.Add(inputLayout);
        }

        // Store the added elements in the dictionary to track them
        tagElements.Add(tag.name, tagRelatedViews);
    }


    // Kiválasztott Tag-ek megjelenítése
    private void UpdateSelectedTagsLayout()
    {
        SelectedTagsLayout.Children.Clear();

        foreach (var tag in _selectedTags)
        {
            // A tagek megjelenítése egy Frame-en belül
            var tagFrame = new Frame
            {
                Padding = new Thickness(10, 2),
                Margin = new Thickness(2, 1),
                CornerRadius = 15,  // Lekerekített sarkok
                BorderColor = Color.FromHex("#CD5C5C"), // IndianRed keret
                BackgroundColor = Color.FromHex("#FFE4E1"), // Világosabb piros háttérszín (MistyRose hexadecimálisban)
                HasShadow = false,  // Árnyék eltávolítása
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            // A tag neve egy Label-ben
            var tagLabel = new Label
            {
                Text = tag.name,
                TextColor = Color.FromHex("#CD5C5C"), // IndianRed szín a szövegre
                FontAttributes = FontAttributes.Bold, // Félkövér szöveg
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Tag kiválasztása vagy törlése kattintással
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => OnTagSelectClicked(this, EventArgs.Empty);
            tagFrame.GestureRecognizers.Add(tapGestureRecognizer);

            // A Label hozzáadása a Frame-hez
            tagFrame.Content = tagLabel;

            // Hozzáadjuk a tagekhez a megjelenítéshez
            SelectedTagsLayout.Children.Add(tagFrame);
        }
    }



    // Panel bezárása
    private void OnCloseTagSelection(object sender, EventArgs e)
    {
        TagSelectionPanel.IsVisible = false;
    }

    private void OnTagSelectClicked(object sender, EventArgs e)
    {
        TagSelectionPanel.IsVisible = true;
    }

    private void ResulNameTextChanged(object sender, TextChangedEventArgs e)
    {
        NameEntry2.Text = NameEntry.Text;
    }

    private void ResulPricesTextChanged(object sender, TextChangedEventArgs e)
    {
        PriceEntry2.Text = PriceEntry.Text + " " + (CurrencyPicker.SelectedItem != null ? CurrencyPicker.SelectedItem.ToString() : "");
    }

    private void OnCurrencyPickerChanged(object sender, EventArgs e)
    {
        if (CurrencyPicker.SelectedItem != null)
        {
            PriceEntry2.Text = PriceEntry.Text + " " + CurrencyPicker.SelectedItem.ToString();
        }
    }

    private void FajlokTagStilusbanMegjelenitese()
    {
        // Ellenőrizzük, hogy léteznek-e fájlok az adott email címhez
        if (!trainersFiles.ContainsKey(UserDatas.Email))
        {
            trainersFiles[UserDatas.Email] = new List<string>();
            return;
        }

        List<string> filesNames = trainersFiles[UserDatas.Email];

        // Töröljük az előző tartalmat
        informationFileInOfferSettings.Children.Clear();
        informationFileInOfferSettings2.Children.Clear();

        foreach (var fileName in filesNames)
        {
            // Létrehozunk egy új Frame-et és labelt az első StackLayout-hoz
            var fileTagFrame1 = new Frame
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

            var fileTagLabel1 = new Label
            {
                Text = fileName,
                FontSize = 14,
                TextColor = Color.FromHex("#CD5C5C"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1
            };

            var tapGestureRecognizer1 = new TapGestureRecognizer();
            tapGestureRecognizer1.Tapped += (s, e) => MegnyitFajlt(fileName);
            fileTagFrame1.GestureRecognizers.Add(tapGestureRecognizer1);
            fileTagFrame1.Content = fileTagLabel1;

            // Hozzáadás az első StackLayout-hoz
            informationFileInOfferSettings.Children.Add(fileTagFrame1);

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


    private void readLongDetailsPanel(QuestionData fajta)
    {
        MyBottomNav.IsVisible = !MyBottomNav.IsVisible;
        kezdostackLayout.IsVisible = !kezdostackLayout.IsVisible;
        longDetailPanel.IsVisible = !longDetailPanel.IsVisible;
        NameEntry3.Text = fajta.name;
        PriceEntry3.Text = fajta.price;
        DisplayAnswers(fajta.answers);
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


    #endregion

    #region oldal Szerkesztése, megjelenitése
    private int presentSide = 0;
    private void sideCahngeButtonClick(object sender, EventArgs e)
    {
        PluszAjanlatBekeres.IsVisible = false;
        sideChangeStacklayout.IsVisible = true;
        presentSide = 0;
        SideMegjelnites();
    }

    private void leftSide(object sender, EventArgs e)
    {
        presentSide--;
        SideMegjelnites();
    }
    private void rightSide(object sender, EventArgs e)
    {
        presentSide++;
        SideMegjelnites();
    }
    private void sideCahngeOK(object sender, EventArgs e)
    {
        //Ide jön a mentes
        PluszAjanlatBekeres.IsVisible = true;
        sideChangeStacklayout.IsVisible = false;
    }

    private void SideMegjelnites()
    {
        questionPart.Children.Clear();

        if (questionsSides.Count == 0)
        {
            Utility.IsVisible = false;
            return;
        }

        leftArrow.IsVisible = presentSide != 0;
        rightArrow.IsVisible = presentSide != questionsSides.Count - 1;
        Utility.IsVisible = true;

        QuestionsSide showSide = questionsSides[presentSide];

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
            WidthRequest = (400.0 / questionsSides.Count) * (presentSide + 1),
            HorizontalOptions = LayoutOptions.Start
        };

        progressIndicator.Children.Add(backgroundBox);
        progressIndicator.Children.Add(progressBox);

        questionPart.Children.Add(progressIndicator);

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
            List<string> selectedAnswers = new List<string>();
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
                        if (selectedAnswers.Contains(btn.Text))
                        {
                            btn.Style = (Style)Application.Current.Resources["tobbvalasztosButtonStyle"];
                            selectedAnswers.Remove(btn.Text);
                        }
                        else
                        {
                            btn.Style = (Style)Application.Current.Resources["tobbvalasztosButtonPickStyle"];
                            selectedAnswers.Add(btn.Text);
                        }
                    };
                }
                // Egyválasztós logika
                else if (showSide.sideType == "Egyválasztós")
                {
                    answerButton.Clicked += (s, e) =>
                    {
                        var btn = s as Button;
                        selectedAnswers.Clear();
                        selectedAnswers.Add(btn.Text);

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

                inputStack.Children.Add(entryFrame);
            }

            questionPart.Children.Add(inputStack);
        }
    }



    private async void SideDelete(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Törlés megerősítése", "Biztosan törölni szeretnéd ezt a Főcimü elemet: " + questionsSides[presentSide].mainTittle, "Igen", "Nem");

        if (answer)
        {
            questionsSides.Remove(questionsSides[presentSide]);

            if (presentSide >= questionsSides.Count && questionsSides.Count != 0)
            {
                presentSide--;
            }

            SideMegjelnites();
        }
    }


    #region oldal Hozzáadása/modositása
    private bool edit = false;
    private void SideEdit(object sender, EventArgs e)
    {
        edit = true;
        QuestionsSide side = questionsSides[presentSide];
        focimEntry.Text = side.mainTittle;
        fokerdesEntry.Text = side.mainQuestion;
        tobbvalasztosRadio.IsChecked = side.sideType == "Többválasztós";
        egyvalasztosRadio.IsChecked = side.sideType == "Egyválasztós";
        beirosRadio.IsChecked = side.sideType == "Beírós";

        kerdesekStack.Clear();
        foreach (var item in side.questionList)
        {
            OnAddNewKerdes(item);
        }


        adSideStacklayout.IsVisible = true;
        sideChangeStacklayout.IsVisible = false;
    }
    private void SideAd(object sender, EventArgs e)
    {
        focimEntry.Text = "";
        fokerdesEntry.Text = "";
        tobbvalasztosRadio.IsChecked = false;
        egyvalasztosRadio.IsChecked = false;
        beirosRadio.IsChecked = false;
        kerdesekStack.Clear();
        OnAddNewKerdes("");
        adSideStacklayout.IsVisible = true;
        sideChangeStacklayout.IsVisible = false;
    }
    private void OnAddNewKerdesClicked(object sender, EventArgs e)
    {
        OnAddNewKerdes("");
    }


    private void OnAddNewKerdes(string text)
    {
        // Grid használata az Entry és az X kép gomb egymás melletti megjelenítéséhez
        var newQuestionGrid = new Grid
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star },  // Entry
            new ColumnDefinition { Width = GridLength.Auto }   // X kép gomb
        },
            Margin = new Thickness(0, 10)  // Távolság egymástól
        };

        // Új kérdésmező (Entry)
        var newKerdesEntry = new Entry
        {
            Placeholder = text == "" ? "Ad meg a kérdést" : text,
            Text = text,
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        // Törlés gomb (X kép)
        var deleteButton = new ImageButton
        {
            Source = "ximg.png",  // Az X kép
            BackgroundColor = Color.FromHex("#00FFFFFF"),
            WidthRequest = 30,
            HeightRequest = 30,
            HorizontalOptions = LayoutOptions.End
        };

        // Törlés funkció hozzáadása az X kép gombhoz
        deleteButton.Clicked += (s, args) =>
        {
            // A kérdésmezőt tartalmazó Grid eltávolítása
            kerdesekStack.Children.Remove(newQuestionGrid);
        };

        // Hozzáadás a Grid-hez
        Grid.SetColumn(newKerdesEntry, 0);
        Grid.SetRow(newKerdesEntry, 0);

        Grid.SetColumn(deleteButton, 1);
        Grid.SetRow(deleteButton, 0);

        newQuestionGrid.Children.Add(newKerdesEntry);
        newQuestionGrid.Children.Add(deleteButton);

        kerdesekStack.Children.Add(newQuestionGrid);
    }


    private bool ValidateInputs()
    {
        // Ellenőrizzük, hogy a mainTittle és a mainQuestion ki van-e töltve
        if (string.IsNullOrWhiteSpace(focimEntry.Text))
        {
            DisplayAlert("Hiba", "A Főcím nem lehet üres!", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(fokerdesEntry.Text))
        {
            DisplayAlert("Hiba", "A Fő kérdés nem lehet üres!", "OK");
            return false;
        }

        // Ellenőrizzük, hogy van-e kiválasztva rádiógomb
        if (!tobbvalasztosRadio.IsChecked && !egyvalasztosRadio.IsChecked && !beirosRadio.IsChecked)
        {
            DisplayAlert("Hiba", "Kérjük, válassz egy válasz típust!", "OK");
            return false;
        }

        // Ellenőrizzük, hogy legalább 2 kérdésmező ki van-e töltve
        int validKerdesCount = 0;

        foreach (var child in kerdesekStack.Children)
        {
            if (child is Grid grid)
            {
                foreach (var gridChild in grid.Children)
                {
                    if (gridChild is Entry entry && !string.IsNullOrWhiteSpace(entry.Text))
                    {
                        validKerdesCount++;
                    }
                }
            }
        }

        if (validKerdesCount < 2 && (!beirosRadio.IsChecked))
        {
            if (beirosRadio.IsChecked && validKerdesCount == 1)
            {
                return true;
            }
            DisplayAlert("Hiba", "Legalább 2 kérdést meg kell adni!", "OK");
            return false;
        }

        // Ha minden rendben van
        return true;
    }

    private void SideAdOk(object sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            // Ha a validáció sikertelen, kilépünk
            return;
        }

        // Rádiógomb kiválasztott típusának lekérése
        string selectedLapfajta = tobbvalasztosRadio.IsChecked ? "Többválasztós" :
                                  egyvalasztosRadio.IsChecked ? "Egyválasztós" : "Beírós";

        // Új QuestionsSide objektum létrehozása a bevitt adatokkal
        QuestionsSide newSide = new QuestionsSide
        {
            mainTittle = focimEntry.Text,
            mainQuestion = fokerdesEntry.Text,
            sideType = selectedLapfajta,
            questionList = kerdesekStack.Children
                                                .OfType<Grid>()  // Először Grid elemeket keresünk
                                                .SelectMany(grid => grid.Children.OfType<Entry>())  // A Grid-ekből keressük ki az Entry elemeket
                                                .Where(e => !string.IsNullOrWhiteSpace(e.Text))  // Csak a nem üres Entry mezők szövegét vesszük
                                                .Select(e => e.Text)
                                                .ToList()

        };
        if (edit)
        {
            questionsSides[presentSide] = newSide;
        }
        else
        {
            questionsSides.Add(newSide);
        }
        // Oldal váltás
        adSideStacklayout.IsVisible = false;
        sideChangeStacklayout.IsVisible = true;
        presentSide = edit ? presentSide : questionsSides.Count - 1;
        edit = false;
        SideMegjelnites();
    }


    #endregion

    #endregion

    #endregion

    #region kijelentkezo oldal

    private async void Logout(object sender, EventArgs e)
    {
        StartRotatingImage();
        await Task.Delay(500);
        var boolResult = await Task.Run(async () => await _trainerService.LogoutTrainer(UserDatas.Email,UserDatas.profileData));

        if (boolResult)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                _profilePicGetUpload.KepTorles();
                _mainFajlService.DeleteFile();

                var menuPage = ServiceHelper.GetService<MainPage>();  // Létrehoz egy új MenuPage példányt
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

    private async void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        NyelvOpciok.IsVisible = !NyelvOpciok.IsVisible;
        ProfileData.IsVisible = !ProfileData.IsVisible;
    }

    private async void OnAddPhotoAddTapped(object sender, EventArgs e)
    {
        uploadResponse? result = await _profilePicGetUpload.PickAndUploadPhotoAsync();
        if (result!=null)
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

    #region fajlFeltoltese
    private FileResult pickedFile;


    // File picker event
    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        try
        {
            // Fájltípusok definiálása, amelyek megengedettek
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS, new[] { "public.image", "public.text", "com.adobe.pdf", "com.microsoft.excel.xls", "com.microsoft.powerpoint.pptx" } }, // PNG, JPEG, TXT, PDF, XLS, PPTX
            { DevicePlatform.Android, new[] { "image/*", "text/plain", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/pdf", "application/vnd.ms-excel", "application/vnd.ms-powerpoint" } }, // PNG, JPEG, TXT, DOC, DOCX, PDF, XLS, PPTX
            { DevicePlatform.WinUI, new[] { ".png", ".jpeg", ".jpg", ".txt", ".doc", ".docx", ".pdf", ".xlsx", ".pptx", ".csv", ".gif", ".bmp", ".rtf" } } // PNG, JPEG, TXT, DOC, DOCX, PDF, XLSX, PPTX, CSV, GIF, BMP, RTF
        });

            pickedFile = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = customFileType, // Speciális fájltípusok megengedése
                PickerTitle = "Please select a valid file"
            });

            if (pickedFile != null)
            {
                // Utólagos ellenőrzés a fájltípusra
                if (!IsValidFileType(pickedFile))
                {
                    await DisplayAlert("Error", "Invalid file type selected. Please choose a valid file.", "OK");
                    pickedFile = null;
                    SelectedFileLabel.Text = "No file selected";
                    return;
                }

                // MIME típus ellenőrzés
                if (!IsValidMimeType(pickedFile.FullPath))
                {
                    await DisplayAlert("Error", "Invalid file MIME type selected.", "OK");
                    pickedFile = null;
                    SelectedFileLabel.Text = "No file selected";
                    return;
                }

                // Ellenőrizzük a fájl méretét
                using (var stream = await pickedFile.OpenReadAsync())
                {
                    if (!_profilePicGetUpload.IsFileSizeValid(stream))
                    {
                        await DisplayAlert("Error", "The file size exceeds 1 MB. Please select a smaller file.", "OK");
                        pickedFile = null;
                        SelectedFileLabel.Text = "No file selected";
                        return;
                    }

                    SelectedFileLabel.Text = $" {pickedFile.FileName} ({stream.Length / 1024.0} KB)";
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"File picking failed: {ex.Message}", "OK");
        }
    }

    // Ellenőrzi, hogy a kiválasztott fájl megengedett típusú-e
    private bool IsValidFileType(FileResult file)
    {
        var allowedExtensions = new[] { ".png", ".jpeg", ".jpg", ".txt", ".doc", ".docx", ".pdf", ".xlsx", ".pptx", ".csv", ".gif", ".bmp", ".rtf" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        return allowedExtensions.Contains(fileExtension);
    }

    private bool IsValidMimeType(string filePath)
    {
        var allowedMimeTypes = new[]
        {
        "image/jpeg", "image/png", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain", "application/pdf", "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation", "image/gif",
        "image/bmp", "text/rtf"
    };

        // MIME típus lekérdezése
        string mimeType = MimeTypesMap.GetMimeType(filePath);
        return allowedMimeTypes.Contains(mimeType);
    }




    // Upload file event
    private async void OnUploadFileClicked(object sender, EventArgs e)
    {
        if (pickedFile == null || string.IsNullOrWhiteSpace(UserDatas.Email))
        {
            await DisplayAlert("Error", "Please select a file", "OK");
            return;
        }

        var email = UserDatas.Email;

        // Megnyitjuk a fájlt streamként
        using (var stream = await pickedFile.OpenReadAsync())
        {
            Stream uploadStream = stream; // Eredeti stream feltöltéshez

            // Ha a fájl kép, tömörítjük
            if (_profilePicGetUpload.IsImageFile(pickedFile))
            {
                uploadStream = _profilePicGetUpload.CompressImage(stream); // Kép tömörítése
            }

            // Kérjük be a fájl nevét a felhasználótól
            string originalFileName = pickedFile.FileName;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            string fileExtension = Path.GetExtension(originalFileName);

            // Kérd a fájl nevét a kiterjesztés nélkül
            string newFileNameWithoutExtension = await DisplayPromptAsync("File Name", "Please enter the file name:", "OK", "Cancel", fileNameWithoutExtension, -1, Keyboard.Text, fileNameWithoutExtension);

            // Illeszd össze az új fájlnevet a kiterjesztéssel
            string newFileName = newFileNameWithoutExtension + fileExtension;


            if (string.IsNullOrWhiteSpace(newFileName))
            {
                await DisplayAlert("Error", "File name cannot be empty", "OK");
                return;
            }

            // Feltöltés a szerverre az új fájlnévvel
            var success = await _fajlService.UploadFile(newFileName, uploadStream, email);
            if (success)
            {
                trainersFiles[UserDatas.Email].Add(newFileName);
                FajlokMegjelenitese();
                SelectedFileLabel.Text = "No file selected";
                pickedFile = null;
                await DisplayAlert("Success", "File uploaded successfully!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "File upload failed.", "OK");
            }
        }
    }

    private void FajlokMegjelenitese()
    {
        // Ellenőrizd, hogy léteznek-e fájlok az adott email címhez
        if (!trainersFiles.ContainsKey(UserDatas.Email))
        {
            trainersFiles[UserDatas.Email] = new List<string>();
            return;
        }

        FajlokTagStilusbanMegjelenitese();

        fajlFeltoloGombok.IsVisible = trainersFiles[UserDatas.Email].Count < 3;

        // Töröld az előző tartalmat
        feltoltottAdatok.Children.Clear();

        List<string> filesNames = trainersFiles[UserDatas.Email];

        foreach (var fileName in filesNames)
        {
            // Hozz létre egy új Frame-et minden fájlnévhez, fehér kerettel és kattintható hatással
            var fileLayout = new Frame
            {
                BackgroundColor = Color.FromHex("#F7F7F7"),  // Halvány belső szín
                BorderColor = Color.FromHex("#FFFFFF"),  // Fehér keret
                CornerRadius = 15,  // Lekerekített sarkok
                Padding = new Thickness(15),
                Margin = new Thickness(0, 10),
                HasShadow = true,  // Árnyék a lebegő hatásért
            };

            // Létrehozunk egy Label-t a fájlnév megjelenítésére
            var fileLabel = new Label
            {
                Text = fileName,
                FontSize = 16,
                TextColor = Color.FromHex("#333333"),  // Sötétszürke betűszín
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };

            // Létrehozunk egy kattintási eseményt a Frame-hez
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (sender, e) =>
            {
                // Esemény kezelése, amikor a fájlra kattintanak
                string action = await DisplayActionSheet($"{fileName} fájl kezelése", "Mégse", null, "Megnyitás", "Törlés");

                if (action == "Megnyitás")
                {
                    MegnyitFajlt(fileName);
                }
                else if (action == "Törlés")
                {
                    var confirm = await DisplayAlert("Törlés", $"Biztosan törölni szeretnéd a(z) {fileName} fájlt?", "Igen", "Nem");
                    if (confirm)
                    {
                        TorolFajlt(fileName);
                    }
                }
            };

            // Hozzáadjuk a kattintási eseményt a fileLabel-hez és a fileLayout-hoz
            fileLabel.GestureRecognizers.Add(tapGestureRecognizer);
            fileLayout.GestureRecognizers.Add(tapGestureRecognizer);

            // Hozzáadjuk a komponenst a Frame-hez
            fileLayout.Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { fileLabel }
            };

            // Hozzáadjuk a fő StackLayout-hoz
            feltoltottAdatok.Children.Add(fileLayout);
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
                var email = UserDatas.Email;

                // PHP hívás, hogy lekérjük a fájlt másik szálon
                var fileBytes = await Task.Run(() => _fajlService.DownloadFajl(email, fileName));

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


    // Fájl törlése funkció
    private async void TorolFajlt(string fileName)
    {
        StartRotatingImage(); // Forgó animáció elindítása

        try
        {
            await Task.Run(async () =>
            {
                var email = UserDatas.Email;

                // Fájl törlése a szerverről külön szálon
                bool success = await _fajlService.DeleteFajl(email, fileName);

                if (success)
                {
                    // UI módosítások főszálon
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        trainersFiles[email].Remove(fileName);
                        FajlokMegjelenitese();
                    });
                }
                else
                {
                    // Hibaüzenet főszálon
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlert("Error", "Failed to delete file from server.", "OK");
                    });
                }
            });
        }
        finally
        {
            // Forgó animáció leállítása főszálon
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StopRotatingImage();
            });
        }
    }




    #endregion

    #endregion

    #region homeWiew
    private string emailTervKeszites;
    private string jelenlegiHet = "1.het", jelenlegiNap = "H";
    private TrainingPlanCreat edzesTerv = new TrainingPlanCreat();
    private List<string> kepekutvonala = new List<string>();

    public async Task UpdateHomeViewAsync(Dictionary<string, CustomerData> costumers)
    {

        // Csoportosítás státusz alapján
        var activeCostumers = costumers.Where(c => c.Value.activeCustomer).Select(c => new { Email = c.Key }).ToList();
        var completedCostumers = costumers.Where(c => !c.Value.activeCustomer).Select(c => new { Email = c.Key }).ToList();

        // Megjelenítés frissítése az aktív ügyfelekhez
        ActiveCustomersView.ItemsSource = activeCostumers;

        // Megjelenítés frissítése a befejezett ügyfelekhez
        CompletedCustomersView.ItemsSource = completedCostumers;

        var allCustomers = new List<dynamic>();
        allCustomers.AddRange(activeCostumers);
        allCustomers.AddRange(completedCostumers);
    }

    // Eseménykezelő, amely a gombra kattintáskor fut le
    private void OnCustomerButtonClicked(object sender, EventArgs e)
    {
        // A gomb szövege az emailcím lesz
        var button = sender as Button;
        if (button != null)
        {
            emailTervKeszites = button.Text;
            FelhasznaloNeve.Text = emailTervKeszites;
            AdatokBerakasa();
            vasrlokMegjelenitese.IsVisible = false;
            edzesTervMegtervezese.IsVisible = true;
            MyBottomNav.IsVisible = false;
        }
    }
    private void AdatokBerakasa()
    {
        fajtakIdKodBerakasaAPanelbe();

        // TODO: BERAKNI a már megcsinált dolgokat
        edzesTerv = ConvertToEdzesTervKeszites(Storage.costumers[emailTervKeszites].trainingDays);
        jelenlegiHet = "1.het"; jelenlegiNap = "H";
        OnWeekButtonClicked(Week1Button, new EventArgs());
        OnDayButtonClicked(DayHButton, new EventArgs());
    }
    private void FelhasznaloAdatokMegmuatatasa(object sender, EventArgs e)
    {
        edzesTervMegtervezese.IsVisible = false;
        vasarloAdatai.IsVisible = true;
        vasarloAdatai.Clear();

        Dictionary<string, List<string>> jelenlegiAdataok = Storage.costumers[emailTervKeszites].questionAndAnswer;

        Button button = new Button
        {
            Text = "Vissza",
            Style = (Style)Application.Current.Resources["AdministratorButton"],
        };
        button.Clicked += (s, e) =>
        {
            edzesTervMegtervezese.IsVisible = true;
            vasarloAdatai.IsVisible = false;
        };
        vasarloAdatai.Children.Add(button);

        foreach (var kvp in jelenlegiAdataok)
        {
            var kerdes = kvp.Key;

            var stackLayout = new StackLayout
            {
                Children =
            {
                new Label
                {
                    Text = kerdes,
                    Style = (Style)Application.Current.Resources["QuestionLabelStyle"]
                }
            }
            };

            foreach (var valasz in kvp.Value)
            {
                stackLayout.Children.Add(new Label
                {
                    Text = valasz,
                    Style = (Style)Application.Current.Resources["AnswerLabelStyle"]
                });
            }

            var frame = new Frame
            {
                Style = (Style)Application.Current.Resources["ElegantFrameStyle"],
                Content = stackLayout
            };

            vasarloAdatai.Children.Add(frame);
        }

    }

    private async void mentesEsKilepes(object sender, EventArgs e)
    {

        Dictionary<DateTime, TrainingDay> newTrainingData = ConvertToNewTrainingData(edzesTerv);

        Storage.costumers[emailTervKeszites].trainingDays = newTrainingData;

        await _mainFajlService.WriteMainFile();

        vasrlokMegjelenitese.IsVisible = true;
        edzesTervMegtervezese.IsVisible = false;
        MyBottomNav.IsVisible = true;
    }

    private void OnWeekButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button2)
        {
            jelenlegiHet = button2.Text;
        }
        var buttons = new List<Button> { Week1Button, Week2Button, Week3Button, Week4Button };
        foreach (var button in buttons)
        {
            button.Style = (Style)Resources["WeekButtonStyle"];
        }
        if (Week1Button == sender)
        {
            ElsoHetesCheckBoxStackLayout.IsVisible = false;
        }
        else
        {
            ElsoHetesCheckBoxStackLayout.IsVisible = true;
        }
        OnDayButtonClicked(DayHButton, e);
        ((Button)sender).Style = (Style)Resources["SelectedWeekButtonStyle"];
    }

    private void OnDayButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button2)
        {
            jelenlegiNap = button2.Text;
        }
        var buttons = new List<Button>
            {
                DayHButton, DayKButton, DaySZButton,
                DayCSButton, DayPButton, DaySZOButton, DayVButton
            };

        foreach (var button in buttons)
        {
            button.Style = (Style)Resources["DayButtonStyle"];
        }

        ((Button)sender).Style = (Style)Resources["SelectedDayButtonStyle"];
        Edzesekberakasa();
    }

    private void Osszefoglalo_TextChanged(object sender, TextChangedEventArgs e)
    {
        edzesTerv.UpdateSummery(jelenlegiHet, jelenlegiNap, Osszefoglalo.Text);
    }

    private void Edzesekberakasa()
    {
        EdzesekListaja.Clear();
        SameAsFirstWeekCheckBox.IsChecked = edzesTerv.sameWeekQuestion[melyikhetnelVagyunk()];
        Osszefoglalo.Text = edzesTerv.GetTrainDay(jelenlegiHet, jelenlegiNap).summery;
        foreach (Training traning in edzesTerv.GetTrainDay(jelenlegiHet, jelenlegiNap).trainings)
        {
            Button button = new Button
            {
                Text = traning.Id,
                Style = (Style)Application.Current.Resources["AdminEdzesekGombokFelsorakozasaFancy"],
            };
            button.Clicked += AdatokUjraMegnezese;
            EdzesekListaja.Children.Add(button);
        }
    }

    private void AdEdzes(object sender, EventArgs e)
    {
        MainTervOsszerakasa.IsVisible = !MainTervOsszerakasa.IsVisible;
        Bovites.IsVisible = !Bovites.IsVisible;
    }

    private void pluszosAd(object sender, EventArgs e)
    {
        fajtaIdKOd.Text = "Ad meg a fajtat";
        GyakorlatiIdoEntry.Text = "";
        MennyisegEntry.Text = "";
        SulyEntry.Text = "";
        VegsoPihenoIdoEntry.Text = "";
        TorlesButton.IsVisible = false;
        AdEdzes(sender, e);
    }

    private void fajtaIsKodValasztosPanel(object sender, EventArgs e)
    {
        Bovites.IsVisible = !Bovites.IsVisible;
        TrainDatas.IsVisible = !TrainDatas.IsVisible;
    }

    private void fajtakIdKodBerakasaAPanelbe()
    {
        TrainDatas.Clear();
        foreach (KeyValuePair<string, TrainingData> idFajtaKeys in Storage.trainingDatas)
        {
            Button button = new Button
            {
                Text = idFajtaKeys.Key,
                Style = (Style)Application.Current.Resources["AdminEdzesekGombokFelsorakozasaFancy"],
            };
            button.Clicked += (sender, e) =>
            {
                fajtaIdKOd.Text = idFajtaKeys.Key;
                fajtaIsKodValasztosPanel(null, null);
            };
            TrainDatas.Children.Add(button);
        }

    }

    private void OnMentesButtonClicked(object sender, EventArgs e)
    {
        Training UjEdzes = new Training(fajtaIdKOd.Text, GyakorlatiIdoEntry.Text, MennyisegEntry.Text, SulyEntry.Text, VegsoPihenoIdoEntry.Text);
        foreach (Training traning in edzesTerv.GetTrainDay(jelenlegiHet, jelenlegiNap).trainings)
        {
            if (traning.Id == UjEdzes.Id)
            {
                edzesTerv.Update(jelenlegiHet, jelenlegiNap, UjEdzes);
                AdEdzes(sender, e);
                return;
            }
        }
        TrainingDay EgeszEdzesNap = new TrainingDay();
        EgeszEdzesNap.summery = Osszefoglalo.Text;
        EgeszEdzesNap.trainings = edzesTerv.GetTrainDay(jelenlegiHet, jelenlegiNap).trainings;
        EgeszEdzesNap.trainings.Add(UjEdzes);
        edzesTerv.PutTrainDay(jelenlegiHet, jelenlegiNap, EgeszEdzesNap);
        Edzesekberakasa();
        AdEdzes(sender, e);
    }

    private void OnTorlesButtonClicked(object sender, EventArgs e)
    {
        edzesTerv.DeleteTraning(jelenlegiHet, jelenlegiNap, fajtaIdKOd.Text);
        Edzesekberakasa();
        AdEdzes(sender, e);
    }

    private void AdatokUjraMegnezese(object sender, EventArgs e)
    {
        if (sender is Button button2)
        {
            TorlesButton.IsVisible = true;
            string nev = button2.Text;
            foreach (Training traning in edzesTerv.GetTrainDay(jelenlegiHet, jelenlegiNap).trainings)
            {
                if (traning.Id == nev)
                {
                    fajtaIdKOd.Text = traning.Id;
                    GyakorlatiIdoEntry.Text = traning.ExerciseTime.ToString();
                    MennyisegEntry.Text = traning.Quantity.ToString();
                    SulyEntry.Text = traning.Weight.ToString();
                    VegsoPihenoIdoEntry.Text = traning.FinalRestTime.ToString();
                }
            }
            AdEdzes(sender, e);
        }
    }

    public int melyikhetnelVagyunk()
    {
        if (jelenlegiHet == "1.het")
        {
            return 0;
        }
        else if (jelenlegiHet == "2.het")
        {
            return 1;
        }
        else if (jelenlegiHet == "3.het")
        {
            return 2;
        }
        else if (jelenlegiHet == "4.het")
        {
            return 3;
        }
        return 0;
    }

    private async void KuldesButtonClicked(object sender, EventArgs e)
    {
        StartRotatingImage();
        Dictionary<DateTime, TrainingDay> newTrainingData = ConvertToNewTrainingData(edzesTerv);

        // Itt hívhatod meg az UpdateUserTrainingAsync metódust a newTrainingData paraméterrel
        // Például: await UpdateUserTrainingAsync(email, newTrainingData);
        Storage.costumers[emailTervKeszites].trainingDays = newTrainingData;
        Storage.costumers[emailTervKeszites].activeCustomer = false;
        ValidData validData = new ValidData();
        validData.trainingDays = newTrainingData;
        validData.eligibleMainTerv = false;
        validData.trainerEmailAddress = UserDatas.Email;

        await Task.Run(async () =>
        {
            await _customerService.PutCustomers(UserDatas.Email, emailTervKeszites, Storage.costumers[emailTervKeszites]);
            await _userService.UpdateUserValidData(emailTervKeszites, validData);
            await _mainFajlService.WriteMainFile();
        });
        await UpdateHomeViewAsync(Storage.costumers);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StopRotatingImage();
            vasrlokMegjelenitese.IsVisible = true;
            edzesTervMegtervezese.IsVisible = false;
            MyBottomNav.IsVisible = true;
        });
    }

    private Dictionary<DateTime, TrainingDay> ConvertToNewTrainingData(TrainingPlanCreat edzesTerv)
    {
        Dictionary<DateTime, TrainingDay> newTrainingData = new Dictionary<DateTime, TrainingDay>();
        DateTime today = DateTime.Today;

        // A hét napjai
        List<string> weekDays = new List<string> { "H", "K", "SZ", "CS", "P", "SZO", "V" };

        // Az összes hét feldolgozása
        for (int i = 0; i < edzesTerv.weeks.Count; i++)
        {
            foreach (var day in weekDays)
            {
                int dayOffset = GetDayOffset(day);
                DateTime dayDate = today.AddDays(i * 7 + dayOffset);

                if (edzesTerv.sameWeekQuestion[i])
                {
                    TrainingDay firstWeekDay = edzesTerv.weeks["1.het"][day];
                    if (!string.IsNullOrEmpty(firstWeekDay.summery))
                    {
                        newTrainingData[dayDate] = firstWeekDay;
                    }
                }
                else
                {
                    TrainingDay currentDay = edzesTerv.weeks[$"{i + 1}.het"][day];
                    if (!string.IsNullOrEmpty(currentDay.summery))
                    {
                        newTrainingData[dayDate] = currentDay;
                    }
                }
            }
        }

        return newTrainingData;
    }

    private TrainingPlanCreat ConvertToEdzesTervKeszites(Dictionary<DateTime, TrainingDay> trainingData)
    {
        var edzesTerv = new TrainingPlanCreat();
        var startOfWeek = DateTime.Today; // Az első hét kezdete
        var weekDays = new List<string> { "H", "K", "SZ", "CS", "P", "SZO", "V" };

        // Csoportosítjuk az edzéseket hetek szerint
        foreach (var entry in trainingData)
        {
            var date = entry.Key;
            var edzesNap = entry.Value;

            // Hét index kiszámítása a kezdő héttől
            int weekIndex = (int)((date - startOfWeek).TotalDays / 7) + 1;
            string weekKey = $"{weekIndex}.het";
            string dayOfWeek = weekDays[(int)date.DayOfWeek];

            // Ha a hét még nem létezik, hozzáadjuk alapértelmezett értékekkel
            if (!edzesTerv.weeks.ContainsKey(weekKey))
            {
                edzesTerv.weeks[weekKey] = new Dictionary<string, TrainingDay>();
                foreach (var day in weekDays)
                {
                    edzesTerv.weeks[weekKey][day] = new TrainingDay(); // Alapértelmezett értékek
                }
            }

            // Beállítjuk az aktuális edzésnapot
            edzesTerv.weeks[weekKey][dayOfWeek] = edzesNap;
        }

        // Az `sameWeekQuestion` lista beállítása: az ismétlődő hetek azonosításához
        for (int i = 0; i < edzesTerv.weeks.Count; i++)
        {
            var currentWeek = edzesTerv.weeks[$"{i + 1}.het"];
            edzesTerv.sameWeekQuestion.Add(i == 0 || AreWeeksIdentical(edzesTerv.weeks["1.het"], currentWeek));
        }

        return edzesTerv;
    }

    // Segédfüggvény az ismétlődő hetek azonosításához
    private bool AreWeeksIdentical(Dictionary<string, TrainingDay> week1, Dictionary<string, TrainingDay> week2)
    {
        foreach (var day in week1.Keys)
        {
            if (!week2.ContainsKey(day) || week1[day].summery != week2[day].summery)
            {
                return false;
            }
        }
        return true;
    }



    private int GetDayOffset(string day)
    {
        return day switch
        {
            "H" => 0,
            "K" => 1,
            "SZ" => 2,
            "CS" => 3,
            "P" => 4,
            "SZO" => 5,
            "V" => 6,
            _ => throw new ArgumentException($"Érvénytelen nap: {day}")
        };
    }

    private void OnCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        edzesTerv.sameWeekQuestion[melyikhetnelVagyunk()] = e.Value;
        if (e.Value)
        {
            edzesTerv.weeks[jelenlegiHet] = edzesTerv.weeks[Week1Button.Text];
        }
        else
        {
            edzesTerv.weeks[jelenlegiHet] = new Dictionary<string, TrainingDay>
            {
                { "H", new TrainingDay() },
                { "K", new TrainingDay() },
                { "SZ", new TrainingDay() },
                { "CS", new TrainingDay() },
                { "P", new TrainingDay() },
                { "SZO", new TrainingDay() },
                { "V", new TrainingDay() }
            };
        }
        Edzesekberakasa();
    }





    #endregion

    #region edzesekBovitese

    private string szerkesztes = "";
    public string alapSzovegAzAnimacionak = "";

    private void traningfajtak()
    {
        traningFajtak.Clear();

        foreach (KeyValuePair<string, TrainingData> idFajtaKeys in Storage.trainingDatas)
        {
            // Create a grid to hold the main button and delete button
            var grid = new Grid
            {
                Margin = new Thickness(0, 5)
            };

            // Define row and column definitions for the grid
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });  // Main button
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // Delete button

            // Main button with the training name
            var mainButton = new Button
            {
                Text = idFajtaKeys.Key,
                Style = (Style)Application.Current.Resources["TrainingButtonStyle"],
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, -40, 0)
            };

            mainButton.Clicked += (sender, e) =>
            {
                szerkesztes = idFajtaKeys.Key;
                idFajtatTrainData.Text = idFajtaKeys.Key;
                AnimacioEleresEntry.Text = idFajtaKeys.Value.AnimationUrl;
                NevGYEntryHU.Text = idFajtaKeys.Value.Name;
                RovidLeirasEntryHU.Text = idFajtaKeys.Value.ShortDetail;
                HosszuLeirasEntryHU.Text = idFajtaKeys.Value.Longdetail;
                traningFajtak.IsVisible = !traningFajtak.IsVisible;
                TrainDatasBovites.IsVisible = !TrainDatasBovites.IsVisible;
                MyBottomNav.IsVisible = !MyBottomNav.IsVisible;
            };

            // Delete button ("X" icon) with updated style and positioning
            // Frame to hold the delete button with a light red circular background
            var deleteButtonFrame = new Frame
            {
                BackgroundColor = Color.FromHex("#FFCCCC"), // Light red background
                CornerRadius = 15, // Half of the HeightRequest/WidthRequest to make it circular
                HeightRequest = 30,
                WidthRequest = 30,
                Padding = 0,
                Margin = new Thickness(0, 0, 15, 0),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                HasShadow = false // Optional: remove shadow for a flat look
            };

            // Delete button ("X" icon) inside the frame
            var deleteButton = new ImageButton
            {
                Source = "ximg.png", // Add your image path here
                BackgroundColor = Colors.Transparent,
                HeightRequest = 25,
                WidthRequest = 25,
                Padding = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            // Add the ImageButton to the Frame
            deleteButtonFrame.Content = deleteButton;


            deleteButton.Clicked += (sender, args) => deleteTrainDatas(sender, idFajtaKeys.Key);

            // Add elements to the grid
            Grid.SetColumn(mainButton, 0);
            Grid.SetColumn(deleteButtonFrame, 1);

            grid.Children.Add(mainButton);
            grid.Children.Add(deleteButtonFrame);

            // Add the grid to the layout
            traningFajtak.Children.Add(grid);
        }

        // Button to add a new training entry
        Button addButton = new Button
        {
            Text = "+",
            Style = (Style)Application.Current.Resources["AddNewTrainingButtonStyle"]
        };

        addButton.Clicked += (sender, e) =>
        {
            szerkesztes = "";
            idFajtatTrainData.Text = "";
            AnimacioEleresEntry.Text = "Add meg a képet";
            NevGYEntryHU.Text = "";
            RovidLeirasEntryHU.Text = "";
            HosszuLeirasEntryHU.Text = "";
            alapSzovegAzAnimacionak = AnimacioEleresEntry.Text;
            traningFajtak.IsVisible = !traningFajtak.IsVisible;
            TrainDatasBovites.IsVisible = !TrainDatasBovites.IsVisible;
            MyBottomNav.IsVisible = !MyBottomNav.IsVisible;
        };

        traningFajtak.Children.Add(addButton);
    }

    private void megseBackButton(object sender, EventArgs e)
    {
        traningFajtak.IsVisible = !traningFajtak.IsVisible;
        TrainDatasBovites.IsVisible = !TrainDatasBovites.IsVisible;
        MyBottomNav.IsVisible = !MyBottomNav.IsVisible;
    }



    private async void deleteTrainDatas(object sender, string name)
    {
        bool answer = await DisplayAlert(Nyelvbeallitas["Megerosites"], Nyelvbeallitas["biztosTorlesKerdes"] + name, Nyelvbeallitas["igen"], Nyelvbeallitas["megse"]);

        if (answer)
        {
            StartRotatingImage();
            Storage.trainingDatas.Remove(name);

            await Task.Run(async () =>
            {
                await _mainFajlService.WriteMainFile();
            });
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StopRotatingImage();
                traningfajtak();
            });
        }
        else
        {
            return;
        }
    }

    private string ValidateFormAdTraining()
    {
        if (string.IsNullOrEmpty(AnimacioEleresEntry.Text) || alapSzovegAzAnimacionak == AnimacioEleresEntry.Text)
        {
            return "Az animáció elérési útja kitöltése kötelező.";
        }

        if (string.IsNullOrEmpty(NevGYEntryHU.Text))
        {
            return "A név mező kitöltése kötelező.";
        }

        if (string.IsNullOrEmpty(RovidLeirasEntryHU.Text))
        {
            return "A rövid leírás kitöltése kötelező.";
        }

        if (string.IsNullOrEmpty(HosszuLeirasEntryHU.Text))
        {
            return "A hosszú leírás kitöltése kötelező.";
        }

        if (string.IsNullOrEmpty(idFajtatTrainData.Text))
        {
            return "Az azonosító mező kitöltése kötelező.";
        }
        if (szerkesztes != "")
        {
            if (Storage.trainingDatas.ContainsKey(idFajtatTrainData.Text) && idFajtatTrainData.Text != szerkesztes)
            {
                return "Sajnos ilyen Id fajta kod már létezik";
            }
        }
        if (szerkesztes == "" && Storage.trainingDatas.ContainsKey(idFajtatTrainData.Text))
        {
            return "Sajnos ilyen Id fajta kod már létezik";
        }

        return string.Empty; // Minden mező kitöltve
    }

    private async void OnMentesTraindatasButtonClicked(object sender, EventArgs e)
    {
        string hiba = ValidateFormAdTraining();
        if (!string.IsNullOrEmpty(hiba))
        {
            await DisplayAlert(Nyelvbeallitas["Hiba"], hiba, "OK");
            return;
        }

        StartRotatingImage();

        TrainingData trainData = new TrainingData(AnimacioEleresEntry.Text, NevGYEntryHU.Text, RovidLeirasEntryHU.Text, HosszuLeirasEntryHU.Text);

        if (szerkesztes != "" && Storage.trainingDatas.ContainsKey(szerkesztes))
        {
            Storage.trainingDatas.Remove(szerkesztes);
            Storage.trainingDatas[idFajtatTrainData.Text] = trainData;
        }
        else
        {
            Storage.trainingDatas[idFajtatTrainData.Text] = trainData;
        }

        await Task.Run(async () =>
        {
            await _mainFajlService.WriteMainFile();
            await _trainingDataService.SaveTrainingData(UserDatas.Email, Storage.trainingDatas);
        });
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StopRotatingImage();
            traningfajtak();
            traningFajtak.IsVisible = !traningFajtak.IsVisible;
            TrainDatasBovites.IsVisible = !TrainDatasBovites.IsVisible;
            MyBottomNav.IsVisible = !MyBottomNav.IsVisible;
        });
    }

    private void PicChoose(object sender, EventArgs e)
    {
        PicFajtak.IsVisible = !PicFajtak.IsVisible;
        TrainDatasBovites.IsVisible = !TrainDatasBovites.IsVisible;
    }

    private void kepekBerakasa()
    {
        LoadImageNames();
        PicFajtak.Clear();

        foreach (var kepe in kepekutvonala)
        {
            // Az Image létrehozása és beállítása
            var image = new Image
            {
                Source = ImageSource.FromFile($"Resources/animation/{kepe}"),
                WidthRequest = 100,
                HeightRequest = 100,
                BackgroundColor = Colors.Transparent,
                Margin = new Thickness(0, 10),
                IsAnimationPlaying = true
            };

            // Átlátszó gomb létrehozása és beállítása
            var transparentButton = new Button
            {
                BackgroundColor = Colors.Transparent,
                WidthRequest = 100,
                HeightRequest = 100,
                Margin = new Thickness(0, 10) // Közötti hely hozzáadása
            };

            transparentButton.Clicked += (sender, e) =>
            {
                AnimacioEleresEntry.Text = kepe;
                PicChoose(sender, e);
            };

            // Kép neve szövegként
            var imageNameLabel = new Label
            {
                Text = kepe,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5)
            };

            // Elválasztó vonal
            var separator = new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Margin = new Thickness(0, 5)
            };

            // Grid létrehozása, hogy egymásra helyezzük az elemeket
            var grid = new Grid
            {
                WidthRequest = 100,
                HeightRequest = 100
            };

            grid.Children.Add(image);
            grid.Children.Add(transparentButton);

            // StackLayout a képhez és a szöveghez
            var imageBlock = new StackLayout
            {
                Spacing = 5,
                Padding = new Thickness(10),
                Children =
            {
                imageNameLabel,
                grid,
                separator
            }
            };

            // Hozzáadás a fő StackLayout-hoz
            PicFajtak.Children.Add(imageBlock);
        }
    }

    private void LoadImageNames()
    {
        kepekutvonala.Clear();
        // Képek neveinek hozzáadása a listához
        kepekutvonala.Add("alkar_felcsavaras.gif");
        kepekutvonala.Add("benchpress.gif");
        kepekutvonala.Add("bicepsz_egykezes.gif");
        kepekutvonala.Add("bicepsz_padnal.gif");
        kepekutvonala.Add("fullbody_felemeles.gif");
        kepekutvonala.Add("hat_deadlift.gif");
        kepekutvonala.Add("hat_huzas.gif");
        kepekutvonala.Add("hat_lehuzas.gif");
        kepekutvonala.Add("lab_gepnel.gif");
        kepekutvonala.Add("mell_gyemantpushup.gif");
        kepekutvonala.Add("tricepsz_ruddal.gif");
        kepekutvonala.Add("tricepsz_tolodszkodas_padon.gif");
        kepekutvonala.Add("tricepsz_dumbleel.gif");
        kepekutvonala.Add("val_dumblel.gif");
        kepekutvonala.Add("val_felemelesketkezzel.gif");
        kepekutvonala.Add("val_tarcsaval.gif");
    }

    #endregion

    #region kozos
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
        this.AbortAnimation("LoopingAnimation"); // Megállítja az animációt, ha az oldal elt?nik
    }

    public async void Nyelvvaltas(bool kezdes)
    {
        if (!kezdes)
        {
            await _mainFajlService.WriteMainFile();
        }
        //languageButton.Text = UserDatas.profileData.Language;
        Languages nyelvekLekeres;
        nyelvekLekeres = new Languages(UserDatas.profileData.Language);
        Nyelvbeallitas = nyelvekLekeres.MyLanguage;

        _availableTags = Tags();
        alapKerdesek = BaseQuestions();
        CreateAvailableTagsButtons();
        //ProfilLabel.Text = Nyelvbeallitas["Profil"];

    }

    public static List<Tag> Tags()
    {
        List<Tag> tags = new List<Tag>();

        tags.Add(new Tag("Edzésterv", new List<string> { "Fogyás, tömegelés, stb", "Milyenfajta" }, new List<string> { "Fogyás, tömegelés, stb", "Pl: sajáttestsulyos,kondis" }));
        tags.Add(new Tag("valami", new List<string> { "valami1", "valami2" }, new List<string> { "valami1", "valami2" }));
        tags.Add(new Tag("Étrend", new List<string> { "Milyen fajta étrend" }, new List<string> { "PL: szálkásito" }));
        tags.Add(new Tag("Étrend2", new List<string> { "Milyen fajta étrend" }, new List<string> { "PL: szálkásito" }));

        return tags;
    }

    public static Dictionary<string, string> BaseQuestions()
    {
        Dictionary<string, string> alapKerdesek = new Dictionary<string, string>();
        alapKerdesek["Heti hany napos?"] = "PL: 4 napos edzésterv";
        alapKerdesek["Kinek ajanlod?"] = "PL: Jani";

        return alapKerdesek;
    }

    #endregion
}