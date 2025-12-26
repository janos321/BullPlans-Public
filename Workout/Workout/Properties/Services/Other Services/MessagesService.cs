using Workout.Properties.class_interfaces.Accessories;
using Workout.Properties.class_interfaces.Other;
using Workout.Properties.Services.Accessories;

namespace Workout.Properties.Services.Other
{
    public class MessagesService
    {
        private const string Controller = "messages/";
        private readonly ApiClient _api;

        public MessagesService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<Conversation>> GetConversations(string myEmail)
        {
            var conversations = await FetchConversations(myEmail);

            bool welcomeWasAlreadyThere = await EnsureWelcomeMessage(myEmail, conversations);

            if (welcomeWasAlreadyThere)
                return conversations;

            return await FetchConversations(myEmail);
        }

        private async Task<List<Conversation>> FetchConversations(string email)
        {
            var response = await _api.PostAsync<List<Conversation>>(
                Controller + "get",
                new { email }
            );

            return response?.Data ?? new();
        }

        /// <summary>
        /// Igaz → már eleve volt welcome üzenet
        /// Hamis → most küldtük el
        /// </summary>
        private async Task<bool> EnsureWelcomeMessage(string myEmail, List<Conversation> conversations)
        {
            if (myEmail == Constans.serverEmail)
                return true;

            bool alreadyHasWelcome = conversations.Any(c =>
                c.email != null &&
                c.email.Contains(Constans.serverEmail)
            );

            if (alreadyHasWelcome)
                return true;

            await PutMessages(
                Constans.serverEmail,
                new List<string> { myEmail },
                "Szia, ha bármiben kell segítség, nyugodtan írj!"
            );

            return false;
        }


        public async Task<bool> PutMessages(string from, List<string> emails, string text)
        {
            var response = await _api.PutAsync<bool>(
                Controller + "put",
                new { from, emails, text }
            );

            return response?.Success ?? false;
        }
    }

}
