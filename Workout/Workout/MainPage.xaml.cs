using FFImageLoading.Helpers;
using System.Net;
using System.Net.Mail;
using Workout.Properties.class_interfaces.Main;
using Workout.Properties.class_interfaces.Other;
using Workout.Properties.Services.Accessories;
using Workout.Properties.Services.Main;
using Workout.Properties.Services.Other;


namespace Workout
{
    public partial class MainPage : ContentPage
    {
        #region változok és kezdetiLépés
        private int Titkosito = -1;
        public Dictionary<string, string>? Nyelvbeallitas;
        private bool administrator = false;
        private string pluszkerdesfajta = "";
        private string kerdesoldalak = "1";
        private int RadioButtonertek = 1;
        private bool isTrainerSelected = false;

        public static int spawnPageCounter = 0;
        public static MenuPage menuPage = null;
        public static MainPage mainPage = null;

        private readonly UserService _userService;
        private readonly TrainerService _trainerService;
        private readonly MainFajlService _mainFajlService;
        private readonly LogInRegService _logInRegService;
        private readonly MotivationService _motivationService;
        private readonly MessagesService _messagesService;
        private readonly ProfilePicService _profileService;

        //public static MainPage Instance { get; private set; }

        public MainPage(UserService userService,
        MainFajlService mainFajlService,
        LogInRegService logInRegService,
        MotivationService motivationService,
        MessagesService messagesService,
        TrainerService trainerService,
        ProfilePicService profileService)
        {
            _userService = userService;
            _mainFajlService = mainFajlService;
            _logInRegService = logInRegService;
            _motivationService = motivationService;
            _messagesService = messagesService;
            _profileService = profileService;
            _trainerService = trainerService;

            mainPage = this;
            FajlCheck();
        }
        public async void FajlCheck()
        {

            if (await _mainFajlService.ReadMainFile())
            {
                if (UserDatas.profileData.Trainer)
                {
                    UgrasTrainerre();
                }
                else
                {
                    UgrasMenure();
                }
                return;
            }

            InitializeComponent();

            Nyelvvaltas();
        }
        #endregion

        #region MainMenu
        private async void OnLanguageButtonClicked(object sender, EventArgs e)
        {
            NyelvOpciok.IsVisible = !NyelvOpciok.IsVisible;
            welcomeLayout.IsVisible = !welcomeLayout.IsVisible;
        }
        private async void OnOptionSelected(object sender, EventArgs e)
        {
            var button = sender as Button;
            await button.ScaleTo(1.1, 100); // Növeli a méretet
            await button.ScaleTo(1.0, 100);
            UserDatas.profileData.SetLanguage(button.Text);
            Nyelvvaltas();

            languageButton.Text = button.Text; // Frissíti a nyelvválasztó gomb szövegét
            NyelvOpciok.IsVisible = false;
            welcomeLayout.IsVisible = true;
        }
        private void OnLoginClicked(object sender, EventArgs e)
        {
            welcomeLayout.IsVisible = !welcomeLayout.IsVisible;
            loginForm.IsVisible = !loginForm.IsVisible;
        }
        private void OnRegisterClicked(object sender, EventArgs e)
        {
            RegisterFrom.IsVisible = !RegisterFrom.IsVisible;
            welcomeLayout.IsVisible = !welcomeLayout.IsVisible;
        }


        private void OnEmailTapped(object sender, EventArgs e)
        {
            var email = Constans.serverEmail;
            var subject = "";
            var body = "";
            var mailtoUri = $"mailto:{email}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";

            try
            {
                Launcher.Default.OpenAsync(mailtoUri);
            }
            catch (Exception ex)
            {
                // Hiba esetén jeleníts meg egy üzenetet
                Console.WriteLine("Hiba" + $"Hiba történt az email megnyitásakor: {ex.Message}" + "OK");
            }
        }


        #endregion

        #region Regisztracio
        private async void Regisztracio(object sender, EventArgs e)
        {
            string hibaUzenet = EllenorizBeviteliMezoket();
            if (!string.IsNullOrEmpty(hibaUzenet))
            {
                DisplayAlert(Nyelvbeallitas["Hiba"], hibaUzenet, "OK");
                return;
            }

            string emailStatus = await _logInRegService.CheckEmail(emailEntry2.Text);

            if (emailStatus == "user_exists" || emailStatus == "trainer_exists")
            {
                await DisplayAlert(Nyelvbeallitas["Hiba"], "email mar letezik","OK");
                return;
            }

            StartRotatingImage();
            await Task.Delay(500);
            Random rnd = new Random();
            Titkosito = rnd.Next(1000, 10000);
            string emailTartalom = $@"
                                <html>
                                <head>
                                    <style>
                                        body {{ font-family: 'Arial', sans-serif; background-color: #f7f7f7; color: #333; margin: 0; padding: 0; }}
                                        .email-container {{ max-width: 600px; margin: 40px auto; background-color: #ffffff; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }}
                                        .email-header {{ background-color: #a52a2a; color: #ffffff; padding: 20px 0; text-align: center; }}
                                        .email-header h1 {{ margin: 0; }}
                                        .email-body {{ padding: 20px; text-align: center; line-height: 1.6; }}
                                        .email-body p {{ margin: 20px 0; }}
                                        .code-container {{ background-color: #000000; color: #d4af37; font-size: 22px; font-weight: bold; padding: 10px; border-radius: 5px; display: inline-block; margin: 20px auto; }}
                                        .email-footer {{ background-color: #c0c0c0; color: #333333; padding: 10px; text-align: center; font-size: 12px; }}
                                        .highlight {{ color: #ff0000; font-weight: bold; }}
                                    </style>
                                </head>
                                <body>
                                    <div class='email-container'>
                                        <div class='email-header'>
                                            <h1>{Nyelvbeallitas["email1"]}</h1>
                                        </div>
                                        <div class='email-body'>
                                            <p>{Nyelvbeallitas["email2"]}</p>
                                            <p>{Nyelvbeallitas["email3"]}</p>
                                            <div class='code-container'>{Titkosito}</div>
                                            <p>{Nyelvbeallitas["email4"]}<span class='highlight'>{Nyelvbeallitas["email5"]}</span>{Nyelvbeallitas["email6"]}</p>
                                        </div>
                                        <div class='email-footer'>
                                            {Nyelvbeallitas["email7"]}<br>
                                            {Nyelvbeallitas["email8"]}<br>Bull Plans
                                        </div>
                                    </div>
                                </body>
                                </html>";
            await KuldesEmail(emailEntry2.Text, emailTartalom, Nyelvbeallitas["regMegerosites"]);
            RegisterFrom.IsVisible = false;
            EmailKeres.IsVisible = true;
            //DisplayAlert("Siker", "Regisztrációd sikeresen elküldve!", "OK");
        }
        private void EmailKuldesBack(object sender, EventArgs e)
        {
            CodeBox1.Text = "";
            CodeBox2.Text = "";
            CodeBox3.Text = "";
            CodeBox4.Text = "";
            RegisterFrom.IsVisible = true;
            EmailKeres.IsVisible = false;
        }
        private string EllenorizBeviteliMezoket()
        {
            // Vezetéknév ellenőrzése
            if (string.IsNullOrWhiteSpace(Vezeteknev.Text))
                return Nyelvbeallitas["hibavezeteknev"];

            // Keresztnév ellenőrzése
            if (string.IsNullOrWhiteSpace(Keresztnev.Text))
                return Nyelvbeallitas["hibaKeresztnev"];

            // E-mail cím ellenőrzése
            if (string.IsNullOrWhiteSpace(emailEntry2.Text) || !EmailEllenorzes(emailEntry2.Text))
                return Nyelvbeallitas["hibaemail"];

            // Születési dátum ellenőrzése
            if (!DatumEllenorzes(EvEntry.Text, HonapEntry.Text, NapEntry.Text))
                return Nyelvbeallitas["hibadatum"];

            // Jelszó ellenőrzése
            if (string.IsNullOrWhiteSpace(passwordEntry2.Text) || passwordEntry2.Text.Length < 6)
                return Nyelvbeallitas["hibajelszo"];

            if (!PrivacyPolicyCheckbox.IsChecked)
            {
                return Nyelvbeallitas["hibaPrivacePolice"];
            }

            // További ellenőrzések szükség szerint...

            return string.Empty;
        }
        private void TogglePasswordVisibility(object sender, EventArgs e)
        {
            passwordEntry2.IsPassword = !passwordEntry2.IsPassword;
            // Itt szükség esetén frissítse a gomb ikonját is
        }

        private async void OnPrivacyPolicyTapped(object sender, EventArgs e)
        {
            // Link megnyitása az adatkezelési tájékoztatóhoz
            await Launcher.OpenAsync(new Uri("https://www.cookiebot.com/"));
        }

        private bool EmailEllenorzes(string email)
        {
            // Egyszerű e-mail ellenőrzés
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool DatumEllenorzes(string ev, string honap, string nap)
        {
            // Dátum ellenőrzése
            if (int.TryParse(ev, out int evSzam) && int.TryParse(honap, out int honapSzam) && int.TryParse(nap, out int napSzam))
            {
                try
                {
                    new DateTime(evSzam, honapSzam, napSzam);
                    return true;
                }
                catch
                {
                    // Érvénytelen dátum
                    return false;
                }
            }
            return false;
        }

        private void OnTrainerClicked(object sender, EventArgs e)
        {
            isTrainerSelected = true;
            UpdateButtonColors();
        }

        private void OnClientClicked(object sender, EventArgs e)
        {
            isTrainerSelected = false;
            UpdateButtonColors();
        }

        private void UpdateButtonColors()
        {
            if (isTrainerSelected)
            {
                TrainerButton.BackgroundColor = Color.FromHex("#FF6347"); // Piros ha kiválasztva
                ClientButton.BackgroundColor = Color.FromHex("#444444"); // Szürke ha nem
            }
            else
            {
                TrainerButton.BackgroundColor = Color.FromHex("#444444"); // Szürke ha nem
                ClientButton.BackgroundColor = Color.FromHex("#FF6347"); // Piros ha kiválasztva
            }
        }

        private async Task KuldesEmail(string email, string emailTartalom, string subject)
        {

            var feladoEmail = Constans.serverEmail; // Az Ön e-mail címe
            var jelszo = "stdqcfcjhdmrakrw"; // Az e-mail fiókhoz tartozó jelszó vagy token



            var smtpClient = new SmtpClient("smtp.gmail.com") // Az SMTP szerver címe
            {
                Port = 587, // Az SMTP szerver portja, gyakran 587 vagy 465
                Credentials = new NetworkCredential(feladoEmail, jelszo),
                EnableSsl = true, // SSL/TLS használata a biztonságos kapcsolathoz
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(feladoEmail),
                Subject = subject,
                Body = emailTartalom,
                IsBodyHtml = true, // Ha HTML formátumú üzenetet szeretne, állítsa ezt true-ra
            };
            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("E-mail sikeresen elküldve.");
                StopRotatingImage();
            }
            catch (SmtpException smtpEx)
            {
                // SMTP specifikus hiba információk
                StopRotatingImage();
                DisplayAlert(Nyelvbeallitas["Hiba"], "SMT Wrong", "OK");
                Console.WriteLine("SMTP Error: {0}", smtpEx.ToString());
            }
            catch (Exception ex)
            {
                // Egyéb általános hiba információk
                StopRotatingImage();
                DisplayAlert(Nyelvbeallitas["Hiba"], "Something Wrong", "OK");
                Console.WriteLine("General Error: {0}", ex.ToString());
            }
        }
        private async void CodeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var currentBox = sender as Entry;

            if (!e.NewTextValue.All(char.IsDigit))
            {
                // Ha nem számjegy, eltávolítjuk az utolsó karaktert
                currentBox.Text = "";
                return;
            }

            if (e.NewTextValue.Length == 1 && currentBox != null)
            {
                // Ugrás a következő mezőre, ha van beírva egy számjegy
                if (currentBox == CodeBox1)
                    CodeBox2.Focus();
                else if (currentBox == CodeBox2)
                    CodeBox3.Focus();
                else if (currentBox == CodeBox3)
                    CodeBox4.Focus();
            }
            else if (e.OldTextValue != null && e.NewTextValue == "" && currentBox != null)
            {
                // Ugrás az előző mezőre törlés esetén
                if (currentBox == CodeBox4)
                    CodeBox3.Focus();
                else if (currentBox == CodeBox3)
                    CodeBox2.Focus();
                else if (currentBox == CodeBox2)
                    CodeBox1.Focus();
            }

            if (!string.IsNullOrEmpty(CodeBox1.Text) &&
                !string.IsNullOrEmpty(CodeBox2.Text) &&
                !string.IsNullOrEmpty(CodeBox3.Text) &&
                !string.IsNullOrEmpty(CodeBox4.Text))
            {
                int a = int.Parse(CodeBox1.Text) * 1000 +
                        int.Parse(CodeBox2.Text) * 100 +
                        int.Parse(CodeBox3.Text) * 10 +
                        int.Parse(CodeBox4.Text);

                if (a == Titkosito)
                {
                    StartRotatingImage();
                    await Task.Delay(500); // Várjunk egy kicsit, hogy az utolsó karakter is beíródjon
                    await ProcessUserAsync();
                    StopRotatingImage();
                }
            }
        }

        private async Task ProcessUserAsync()
        {
            var email = emailEntry2.Text;
            var nev = $"{Vezeteknev.Text} {Keresztnev.Text}";
            var date = $"{EvEntry.Text}-{HonapEntry.Text}-{NapEntry.Text}";
            UserDatas.profileData = new ProfileData(isTrainerSelected, Constans.LanguageName.En, "0", new DateTime());
            UserDatas.validData = new ValidData();
            UserDatas.trainerDatas = new TrainerData();

            var payload = new RegisterPayload
            {
                name = nev,
                email = email,
                date = date,
                password = passwordEntry2.Text,
                profile_data = UserDatas.profileData,
                valid_data = isTrainerSelected ? UserDatas.trainerDatas : UserDatas.validData
            };

            bool success = isTrainerSelected
                ? await _logInRegService.RegisterTrainer(payload)
                : await _logInRegService.RegisterUser(payload);

            if (!success)
            {
                await DisplayAlert("Hiba", "Email már foglalt!", "OK");
                return;
            }

            UserDatas.Email = email;
            UserDatas.Date = date;
            UserDatas.UserName = nev;
            await _mainFajlService.WriteMainFile();

            if (isTrainerSelected)
                UgrasTrainerre();
            else
                UgrasMenure();
        }

        #endregion

        #region Bejelentkezes
        private void TogglePasswordVisibility2(object sender, EventArgs e)
        {
            passwordEntry.IsPassword = !passwordEntry.IsPassword;
            // Itt szükség esetén frissítse a gomb ikonját is
        }
        private async void Login(object sender, EventArgs e)
        {
            string hibaUzenet = await Task.Run(() => EllenorizBeviteliMezoketBejelentkezes());
            if (!string.IsNullOrEmpty(hibaUzenet))
            {
                await DisplayAlert(Nyelvbeallitas["Hiba"], hibaUzenet, "OK");
                return;
            }
            StartRotatingImage();

            string? role = await _logInRegService.GetRole(emailEntry.Text, passwordEntry.Text);

            StopRotatingImage();

            if (role == null)
            {
                await DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas["helytelenbejelntkezes"], "OK");
                return;
            }

            UserDatas.Email = emailEntry.Text;

            switch (role)
            {
                case "admin":
                    administrator = true;
                    Administraciosdolgok.IsVisible = true;
                    loginForm.IsVisible = false;
                    break;

                case "trainer":
                    {
                        bool ok = await _trainerService.LoadTrainer(emailEntry.Text);

                        if (ok)
                        {
                            loginForm.IsVisible = false;
                            UgrasTrainerre();
                        }
                        break;
                    }

                case "user":
                    {
                        bool ok = await _userService.LoadUser(emailEntry.Text);

                        if (ok)
                        {
                            loginForm.IsVisible = false;
                            UgrasMenure();
                        }
                        break;
                    }

                case "already_logged_in":
                    await DisplayAlert(Nyelvbeallitas["Hiba"],Nyelvbeallitas["foglaltaccount"],"OK");
                    break;

                default:
                    await DisplayAlert(Nyelvbeallitas["Hiba"],Nyelvbeallitas["helytelenbejelntkezes"],"OK");
                    break;
            }                 
        }

        private void ForgotePasswordPanel(object sender, EventArgs e)
        {
            forgotePasswordForm.IsVisible = !forgotePasswordForm.IsVisible;
            loginForm.IsVisible = !loginForm.IsVisible;
            kodBekeres.IsVisible = false;
            jelszoBekeres.IsVisible = true;
            emailEntry3.Text = "";
            forgottePassword.Text = "";
            CodeBox12.Text = "";
            CodeBox22.Text = "";
            CodeBox32.Text = "";
            CodeBox42.Text = "";
        }

        private async void SendEmail(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(emailEntry3.Text) || !EmailEllenorzes(emailEntry3.Text))
            {
                await DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas["hibaemail"], "OK");
                return;
            }
            else
                if (string.IsNullOrWhiteSpace(forgottePassword.Text) || forgottePassword.Text.Length < 6)
            {
                await DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas["hibajelszo"], "OK");
                return;
            }
            Random rnd = new Random();
            Titkosito = rnd.Next(1000, 10000);
            string emailTartalom = $@"
                                    <html>
                                    <head>
                                        <style>
                                            body {{ font-family: 'Arial', sans-serif; background-color: #f7f7f7; color: #333; margin: 0; padding: 0; }}
                                            .email-container {{ max-width: 600px; margin: 40px auto; background-color: #ffffff; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }}
                                            .email-header {{ background-color: #a52a2a; color: #ffffff; padding: 20px 0; text-align: center; }}
                                            .email-header h1 {{ margin: 0; }}
                                            .email-body {{ padding: 20px; text-align: center; line-height: 1.6; }}
                                            .email-body p {{ margin: 20px 0; }}
                                            .code-container {{ background-color: #000000; color: #d4af37; font-size: 22px; font-weight: bold; padding: 10px; border-radius: 5px; display: inline-block; margin: 20px auto; }}
                                            .email-footer {{ background-color: #c0c0c0; color: #333333; padding: 10px; text-align: center; font-size: 12px; }}
                                            .highlight {{ color: #ff0000; font-weight: bold; }}
                                        </style>
                                    </head>
                                    <body>
                                        <div class='email-container'>
                                            <div class='email-header'>
                                                <h1>{Nyelvbeallitas["email1"]}</h1>
                                            </div>
                                            <div class='email-body'>
                                                <p>{Nyelvbeallitas["email2"]}</p>
                                                <div class='code-container'>{Titkosito}</div>
                                                <p>{Nyelvbeallitas["email4"]}</p>
                                            </div>
                                            <div class='email-footer'>
                                                {Nyelvbeallitas["email7"]}<br>
                                                {Nyelvbeallitas["email8"]}<br>Bull Plans
                                            </div>
                                        </div>
                                    </body>
                                    </html>";
            StartRotatingImage();
            await KuldesEmail(emailEntry3.Text, emailTartalom, Nyelvbeallitas["elfelejtettJelszoEmail"]);

            kodBekeres.IsVisible = true;
            jelszoBekeres.IsVisible = false;

        }

        private async void CodeBox_TextChanged2(object sender, TextChangedEventArgs e)
        {
            var currentBox = sender as Entry;

            if (!e.NewTextValue.All(char.IsDigit))
            {
                // Ha nem számjegy, eltávolítjuk az utolsó karaktert
                currentBox.Text = "";
                return;
            }

            if (e.NewTextValue.Length == 1 && currentBox != null)
            {
                // Ugrás a következő mezőre, ha van beírva egy számjegy
                if (currentBox == CodeBox12)
                    CodeBox22.Focus();
                else if (currentBox == CodeBox22)
                    CodeBox32.Focus();
                else if (currentBox == CodeBox3)
                    CodeBox42.Focus();
            }
            else if (e.OldTextValue != null && e.NewTextValue == "" && currentBox != null)
            {
                // Ugrás az előző mezőre törlés esetén
                if (currentBox == CodeBox42)
                    CodeBox32.Focus();
                else if (currentBox == CodeBox32)
                    CodeBox22.Focus();
                else if (currentBox == CodeBox22)
                    CodeBox12.Focus();
            }

            if (!string.IsNullOrEmpty(CodeBox12.Text) &&
                !string.IsNullOrEmpty(CodeBox22.Text) &&
                !string.IsNullOrEmpty(CodeBox32.Text) &&
                !string.IsNullOrEmpty(CodeBox42.Text))
            {
                int a = int.Parse(CodeBox12.Text) * 1000 +
                        int.Parse(CodeBox22.Text) * 100 +
                        int.Parse(CodeBox32.Text) * 10 +
                        int.Parse(CodeBox42.Text);

                if (a == Titkosito)
                {
                    StartRotatingImage();
                    await Task.Delay(500);
                    await Task.Run((Func<Task<bool>?>)(() =>
                    {
                        return _logInRegService.UpdatePassword(emailEntry3.Text, forgottePassword.Text);
                    }));

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        StopRotatingImage();
                        ForgotePasswordPanel(null, null);
                    });

                }
            }
        }

        private string EllenorizBeviteliMezoketBejelentkezes()
        {

            // E-mail cím ellenőrzése
            if (string.IsNullOrWhiteSpace(emailEntry.Text) || !EmailEllenorzes(emailEntry.Text))
                return Nyelvbeallitas["hibaemail"];

            // Jelszó ellenőrzése
            if (string.IsNullOrWhiteSpace(passwordEntry.Text) || passwordEntry.Text.Length < 6)
                return Nyelvbeallitas["hibajelszo"];


            return string.Empty;
        }

        #endregion



        #region Admin
        private void Foadminbavissza(object sender, EventArgs e)
        {
            UzenetLista.IsVisible = false;
            MotivacioHozzadas.IsVisible = false;
            //FelhasznaloLista.IsVisible = false;
            Administraciosdolgok.IsVisible = true;
        }
        #region UzenetezesAdmin
        private List<Conversation>? conversationAll;
        private Conversation? presentConversation;
        private string firstEmail = "";
        private async void UzenetMegnezeseAdmin(object sender, EventArgs e)
        {
            UzenetLista.Children.Clear();
            UzenetPanel.Children.Clear();
            MessageView.IsVisible = false;
            Administraciosdolgok.IsVisible = false;
            conversationAll = await _messagesService.GetConversations(Constans.serverEmail);
            for (int i = 0; i < conversationAll.Count; i++)
            {
                Button button = new Button
                {
                    Text = conversationAll[i].email[0],
                    Style = (Style)Application.Current.Resources["AdministratorButton"],
                };

                // Itt adhatod hozzá az eseménykezelőt, ha szükséges
                button.Clicked += UzenetMegnyitasa;

                UzenetLista.Children.Add(button);
            }

            Button button2 = new Button
            {
                Text = "Vissza",
                Style = (Style)Application.Current.Resources["AdministratorButton"],
            };

            // Itt adhatod hozzá az eseménykezelőt, ha szükséges
            button2.Clicked += Foadminbavissza;

            UzenetLista.Children.Add(button2);

            UzenetLista.IsVisible = true;
        }

        private async void UzenetMegnyitasa(object sender, EventArgs e)
        {
            UzenetLista.IsVisible = false;

            if (sender is Button button)
            {
                firstEmail = button.Text;
                UserNamee.Text = firstEmail;
                presentConversation = conversationAll.Find(c => c.email != null && c.email.Contains(firstEmail));
                var imageSource = await _profileService.DownloadProfilePic(firstEmail);
                if (imageSource != null)
                {
                    UserProfileImage.Source = imageSource.Value.Image;
                }
                else
                {
                    Console.WriteLine("A kép letöltése sikertelen.");
                }
                if (presentConversation == null)
                {
                    return;
                }

                foreach (Content content in presentConversation.content)
                {
                    uzenetberakasa(content.text, content.From != firstEmail);
                }
            }

            MessageView.IsVisible = true;
        }

        private async void UzenetKuldes(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageEntry.Text))
            {
                uzenetberakasa(MessageEntry.Text, true);

                bool ok = await _messagesService.PutMessages(Constans.serverEmail, new List<string> { firstEmail }, MessageEntry.Text);
                if (ok)
                {
                    var newMessage = new Content(Constans.serverEmail, firstEmail, MessageEntry.Text);
                    presentConversation.content.Add(newMessage);//Azért müködik mert referncia szerint másolodik
                    presentConversation.updatedAt = DateTime.Now;
                }
                MessageEntry.Text = string.Empty;
            }
        }
        private async void uzenetberakasa(string uzenet, bool userkuld)
        {
            Frame frame = new Frame
            {
                CornerRadius = 20,
                BackgroundColor = userkuld ? Color.FromRgb(205, 92, 92) : Color.FromRgb(165, 42, 42),
                HorizontalOptions = userkuld ? LayoutOptions.End : LayoutOptions.Start,
                Padding = 10,
                Margin = new Thickness(0)
            };

            Label label = new Label
            {
                Text = uzenet,
                HorizontalOptions = userkuld ? LayoutOptions.End : LayoutOptions.Start,
                TextColor = Color.FromRgb(255, 255, 255)
            };

            frame.Content = label;
            UzenetPanel.Children.Add(frame);

            // Görgetés az új üzenet elemre
            await Task.Delay(100); // 100 milliszekundum késleltetés

            await MyScrollView.ScrollToAsync(frame, ScrollToPosition.End, true);
        }

        #endregion

        #region MotivaciosSzoveg
        private void MotivaciosAdMegnyitasa(object sender, EventArgs e)
        {
            Administraciosdolgok.IsVisible = false;
            MotivacioHozzadas.IsVisible = true;
        }
        private async void OnKuldesButtonClicked(object sender, EventArgs e)
        {
            await _motivationService.PostMotivation(
            new Dictionary<string, string>
            {
                { Constans.LanguageName.Hu, MagyarMotivaciosSzoveg.Text },
                { Constans.LanguageName.En, AngolMotivaciosSzoveg.Text }
            });
            MagyarMotivaciosSzoveg.Text = "";
            AngolMotivaciosSzoveg.Text = "";
        }
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

        private async void UgrasMenure()
        {
            var menuPage = ServiceHelper.GetService<MenuPage>(); // Létrehoz egy új MenuPage példányt

            if (Application.Current.MainPage is AppShell shell)
            {
                // Létrehoz egy új MenuPage példányt és beállítja
                shell.SetMainMenuPage(menuPage, "MenuPage" + spawnPageCounter, "");
            }

            await Shell.Current.GoToAsync("///MenuPage" + spawnPageCounter);
        }

        private async void UgrasTrainerre()
        {
            var trainerPage = ServiceHelper.GetService<TrainerPage>(); // Létrehoz egy új MenuPage példányt

            if (Application.Current.MainPage is AppShell shell)
            {
                // Létrehoz egy új MenuPage példányt és beállítja
                shell.SetMainMenuPage(trainerPage, "TrainerPage" + spawnPageCounter, "");
            }

            await Shell.Current.GoToAsync("///TrainerPage" + spawnPageCounter);
        }

        public async Task VanAktivHálózatiKapcsolat()
        {
            DisplayAlert(Nyelvbeallitas["Hiba"], Nyelvbeallitas["noInternet"], "OK");
        }

        public async void Nyelvvaltas()
        {
            Languages nyelvekLekeres;
            nyelvekLekeres = new Languages(UserDatas.profileData.Language);
            Nyelvbeallitas = nyelvekLekeres.MyLanguage;
            languageButton.Text = UserDatas.profileData.Language;
            loginButton.Text = Nyelvbeallitas["loginButton"];
            loginButton2.Text = Nyelvbeallitas["loginButton"];
            loginButton3.Text = Nyelvbeallitas["loginButton"];
            registerButton3.Text = Nyelvbeallitas["registerButton"];
            registerButton.Text = Nyelvbeallitas["registerButton"];
            registerButton2.Text = Nyelvbeallitas["registerButton"];
            emailEntry.Placeholder = Nyelvbeallitas["emailEntry"];
            emailEntry2.Placeholder = Nyelvbeallitas["emailEntry"];
            emailEntry3.Placeholder = Nyelvbeallitas["emailEntry"];
            passwordEntry.Placeholder = Nyelvbeallitas["passwordEntry"];
            passwordEntry2.Placeholder = Nyelvbeallitas["passwordEntry"];
            Belepesiadatad2.Text = Nyelvbeallitas["Belepesiadatad"];
            Belepesiadatad.Text = Nyelvbeallitas["Belepesiadatad"];
            Vezeteknev.Placeholder = Nyelvbeallitas["Vezeteknev"];
            SzulDatum.Text = Nyelvbeallitas["SzulDatum"];
            Keresztnev.Placeholder = Nyelvbeallitas["Keresztnev"];
            EvEntry.Placeholder = Nyelvbeallitas["EvEntry"];
            NapEntry.Placeholder = Nyelvbeallitas["NapEntry"];
            HonapEntry.Placeholder = Nyelvbeallitas["HonapEntry"];
            KodbekeresLabel.Text = Nyelvbeallitas["KodbekeresLabel"];
            KodbekeresLabel2.Text = Nyelvbeallitas["KodbekeresLabel"];
            KodUjraKuldesButton.Text = Nyelvbeallitas["KodUjraKuldesButton"];
            Elfogadom.Text = Nyelvbeallitas["Elfogadom"];
            PrivacyPolicyLink.Text = Nyelvbeallitas["PrivacyPolicyLink"];
            gondkerdesLabel.Text = Nyelvbeallitas["gondemailiras"];
            ForgotemyAccountPasswor.Text = Nyelvbeallitas["elfelejtettJelszo"];
            SendButton2.Text = Nyelvbeallitas["Send"];
            forgottePassword.Placeholder = Nyelvbeallitas["ujJelszoLabel"];
            ClientButton.Text = Nyelvbeallitas["client"];
            TrainerButton.Text = Nyelvbeallitas["edzo"];

        }

        #endregion


    }
}
