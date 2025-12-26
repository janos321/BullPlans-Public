using System.Windows.Input;

namespace Workout;

public partial class BottomNavMenu : ContentView
{
    // --- SLOT 1 TULAJDONSÁGOK (Grid.Column="0") ---
    public static readonly BindableProperty Icon1Property = BindableProperty.Create(nameof(Icon1), typeof(ImageSource), typeof(BottomNavMenu));
    public ImageSource Icon1 { get => (ImageSource)GetValue(Icon1Property); set => SetValue(Icon1Property, value); }

    public static readonly BindableProperty Text1Property = BindableProperty.Create(nameof(Text1), typeof(string), typeof(BottomNavMenu));
    public string Text1 { get => (string)GetValue(Text1Property); set => SetValue(Text1Property, value); }

    public static readonly BindableProperty Command1Property = BindableProperty.Create(nameof(Command1), typeof(ICommand), typeof(BottomNavMenu));
    public ICommand Command1 { get => (ICommand)GetValue(Command1Property); set => SetValue(Command1Property, value); }

    // --- SLOT 2 TULAJDONSÁGOK (Grid.Column="1") ---
    public static readonly BindableProperty Icon2Property = BindableProperty.Create(nameof(Icon2), typeof(ImageSource), typeof(BottomNavMenu));
    public ImageSource Icon2 { get => (ImageSource)GetValue(Icon2Property); set => SetValue(Icon2Property, value); }

    public static readonly BindableProperty Text2Property = BindableProperty.Create(nameof(Text2), typeof(string), typeof(BottomNavMenu));
    public string Text2 { get => (string)GetValue(Text2Property); set => SetValue(Text2Property, value); }

    public static readonly BindableProperty Command2Property = BindableProperty.Create(nameof(Command2), typeof(ICommand), typeof(BottomNavMenu));
    public ICommand Command2 { get => (ICommand)GetValue(Command2Property); set => SetValue(Command2Property, value); }

    // --- SLOT 3 (HOME) TULAJDONSÁGOK (Grid.Column="2") ---
    public static readonly BindableProperty Icon3Property = BindableProperty.Create(nameof(Icon3), typeof(ImageSource), typeof(BottomNavMenu));
    public ImageSource Icon3 { get => (ImageSource)GetValue(Icon3Property); set => SetValue(Icon3Property, value); }

    public static readonly BindableProperty Text3Property = BindableProperty.Create(nameof(Text3), typeof(string), typeof(BottomNavMenu));
    public string Text3 { get => (string)GetValue(Text3Property); set => SetValue(Text3Property, value); }

    public static readonly BindableProperty Command3Property = BindableProperty.Create(nameof(Command3), typeof(ICommand), typeof(BottomNavMenu));
    public ICommand Command3 { get => (ICommand)GetValue(Command3Property); set => SetValue(Command3Property, value); }

    // --- SLOT 4 TULAJDONSÁGOK (Grid.Column="3") ---
    // ... (Hozd létre az Icon4, Text4, Command4 tulajdonságokat) ...
    public static readonly BindableProperty Icon4Property = BindableProperty.Create(nameof(Icon4), typeof(ImageSource), typeof(BottomNavMenu));
    public ImageSource Icon4 { get => (ImageSource)GetValue(Icon4Property); set => SetValue(Icon4Property, value); }
    public static readonly BindableProperty Text4Property = BindableProperty.Create(nameof(Text4), typeof(string), typeof(BottomNavMenu));
    public string Text4 { get => (string)GetValue(Text4Property); set => SetValue(Text4Property, value); }
    public static readonly BindableProperty Command4Property = BindableProperty.Create(nameof(Command4), typeof(ICommand), typeof(BottomNavMenu));
    public ICommand Command4 { get => (ICommand)GetValue(Command4Property); set => SetValue(Command4Property, value); }

    // --- SLOT 5 TULAJDONSÁGOK (Grid.Column="4") ---
    // ... (Hozd létre az Icon5, Text5, Command5 tulajdonságokat) ...
    public static readonly BindableProperty Icon5Property = BindableProperty.Create(nameof(Icon5), typeof(ImageSource), typeof(BottomNavMenu));
    public ImageSource Icon5 { get => (ImageSource)GetValue(Icon5Property); set => SetValue(Icon5Property, value); }
    public static readonly BindableProperty Text5Property = BindableProperty.Create(nameof(Text5), typeof(string), typeof(BottomNavMenu));
    public string Text5 { get => (string)GetValue(Text5Property); set => SetValue(Text5Property, value); }
    public static readonly BindableProperty Command5Property = BindableProperty.Create(nameof(Command5), typeof(ICommand), typeof(BottomNavMenu));
    public ICommand Command5 { get => (ICommand)GetValue(Command5Property); set => SetValue(Command5Property, value); }


    // --- KONSTRUKTOR ---
    public BottomNavMenu()
    {
        InitializeComponent();
        AnimateSelection(Slot3Frame);
    }

    // --- KATTINTÁS ESEMÉNYKEZELÕK ---

    private void OnSlot1Tapped(object sender, TappedEventArgs e)
    {
        AnimateSelection(Slot1Frame); // Animáció
        Command1?.Execute(null);      // Oldalváltás (külsõ parancs)
    }

    private void OnSlot2Tapped(object sender, TappedEventArgs e)
    {
        AnimateSelection(Slot2Frame);
        Command2?.Execute(null);
    }

    private void OnSlot3Tapped(object sender, TappedEventArgs e)
    {
        AnimateSelection(Slot3Frame);
        Command3?.Execute(null);
    }

    private void OnSlot4Tapped(object sender, TappedEventArgs e)
    {
        AnimateSelection(Slot4Frame);
        Command4?.Execute(null);
    }

    private void OnSlot5Tapped(object sender, TappedEventArgs e)
    {
        AnimateSelection(Slot5Frame);
        Command5?.Execute(null);
    }

    // --- A TE EREDETI 'nagyitas' LOGIKÁD, ÁTHELYEZVE IDE ---
    private void AnimateSelection(Frame valszato)
    {
        // 1. Mindent alaphelyzetbe
        Slot1Frame.ScaleTo(1, 150);
        Slot2Frame.ScaleTo(1, 150);
        Slot3Frame.ScaleTo(1, 150);
        Slot4Frame.ScaleTo(1, 150);
        Slot5Frame.ScaleTo(1, 150);

        Slot1Frame.HeightRequest = 50;
        Slot2Frame.HeightRequest = 50;
        Slot3Frame.HeightRequest = 50; // A Home is 50 alapból
        Slot4Frame.HeightRequest = 50;
        Slot5Frame.HeightRequest = 50;

        Slot1Stack.HeightRequest = 50;
        Slot2Stack.HeightRequest = 50;
        Slot3Stack.HeightRequest = 50; // A Home Stack alapból nagyobb
        Slot4Stack.HeightRequest = 50;
        Slot5Stack.HeightRequest = 50;

        /*// 2. A kiválasztott nagyítása
        if (Slot3Frame == valszato) // Ha a Home a kiválasztott
        {
            (valszato.Content as StackLayout).HeightRequest = 90;
            valszato.ScaleTo(1.4, 150);
            valszato.HeightRequest = 105;
        }
        else // Ha bármelyik MÁSIK a kiválasztott
        {*/
            (valszato.Content as StackLayout).HeightRequest = 70;
            valszato.ScaleTo(1.25, 150);
            valszato.HeightRequest = 95;
        // }
    }
}