using CommunityToolkit.Mvvm.Messaging;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Workout.Messages;
using Workout.Properties.class_interfaces.Main;
using Workout.Properties.class_interfaces.Other;
using Workout.Properties.Services.Other;

namespace Workout;

public class ChatMessage
{
    public string Text { get; set; }
    public bool IsSentByUser { get; set; }
    public bool IsFirstInGroup { get; set; }
    public bool IsLastInGroup { get; set; }

    // Ide jöhet még pl. Időbélyeg, stb.
}
public partial class MessageView : ContentView
{
    private readonly MessagesService _messagesService;
    public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();
    public IEnumerable ItemsSource
    {
        get { return messageCustomersView.ItemsSource; }
        set { messageCustomersView.ItemsSource = value; }
    }
    public MessageView()
    {
        if (IPlatformApplication.Current?.Services != null)
        {
            _messagesService = IPlatformApplication.Current.Services.GetService<MessagesService>();
        }
        InitializeComponent();
        MessagesCollectionView.ItemsSource = Messages;
    }

    private List<Conversation> conversationAll;
    private Conversation presentConversation;
    private string firstEmail = "";
    private ChatMessage previous = null;

    public async Task GetConversation()
    {
        conversationAll = await _messagesService.GetConversations(UserDatas.Email);

        if (conversationAll != null)
        {
            messageCustomersView.ItemsSource = conversationAll
                .Where(c => c.email != null && c.email.Count > 0)
                .Select(c => c.email[0])
                .Distinct()
                .ToList();
        }

    }
    private async void OnMessageCustomerButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            presentConversation = conversationAll.Find(c => c.email != null && c.email.Contains(button.Text));

            if (presentConversation != null)
            {
                firstEmail = presentConversation.email.FirstOrDefault() ?? "";
                messageCustomersView.IsVisible = false;
                messagePanel.IsVisible = true;
                WeakReferenceMessenger.Default.Send(new ToggleNavMenuVisibilityMessage(false));
                UserName.Text = firstEmail;
                await MessageLoad();
            }
            else
            {
                Console.WriteLine("Nem találtam ilyen beszélgetést.");
            }
        }
    }
    private void BackMessage(object sender, EventArgs e)
    {
        messageCustomersView.IsVisible = true;
        messagePanel.IsVisible = false;
        WeakReferenceMessenger.Default.Send(new ToggleNavMenuVisibilityMessage(true));
    }

    private async void SendMessage(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(messageEntry.Text))
        {
            //uzenetberakasa(messageEntry.Text, true);

            var ok = await _messagesService.PutMessages(UserDatas.Email, new List<string> { firstEmail }, messageEntry.Text);
            if (ok)
            {
                var newMessage = new Content(UserDatas.Email, firstEmail, messageEntry.Text);
                presentConversation.content.Add(newMessage);//Azért müködik mert referncia szerint másolodik
                presentConversation.updatedAt = DateTime.Now;
                if (previous != null && previous.IsSentByUser)
                {
                    previous.IsLastInGroup = false;
                    int index = Messages.IndexOf(previous);
                    if (index >= 0)
                    {
                        Messages[index] = Messages[index]; //frissiteni kell az objectumot, mert maskepp nem frissül a desagn
                    }

                }
                PutMessage(newMessage);
                previous.IsLastInGroup = true;
                LastElement();
            }

            messageEntry.Text = string.Empty;
        }
    }

    public async void LastElement()
    {
        await Task.Delay(50);
        if (Messages.Any())
        {
            MessagesCollectionView.ScrollTo(Messages.Last(), position: ScrollToPosition.End, animate: true);
        }
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        try
        {
            await GetConversation();
            presentConversation = conversationAll.Find(c => c.email != null && c.email.Contains(firstEmail));
            await MessageLoad();
        }
        finally
        {
            Rv.IsRefreshing = false;
        }
    }

    private async Task MessageLoad()
    {
        Messages.Clear();

        foreach (Content content in presentConversation.content)
        {
            PutMessage(content);
        }

        if (previous != null)
            previous.IsLastInGroup = true;

        LastElement();
    }

    private void PutMessage(Content content)
    {
        var current = new ChatMessage
        {
            Text = content.text,
            IsSentByUser = (content.From != firstEmail)
        };

        if (previous == null || previous.IsSentByUser != current.IsSentByUser)
        {
            current.IsFirstInGroup = true;

            if (previous != null)
                previous.IsLastInGroup = true;
        }

        Messages.Add(current);
        previous = current;
    }
}