using System.Diagnostics;

namespace Workout.Properties.class_interfaces.Other
{
    public class Content
    {
        public string From { get; set; }
        public string To { get; set; }
        public string text { get; set; }

        public Content(string from, string to, string text)
        {
            From = from;
            To = to;
            this.text = text;
        }
    }

    public class Conversation
    {
        public List<string> email { get; set; }
        public List<Content> content { get; set; }
        public DateTime updatedAt { get; set; }

        public Conversation(List<string> email, List<Content> content, DateTime updatedAt)
        {
            this.email = email;
            this.content = content;
            this.updatedAt = updatedAt;
        }
        public void PrintDebug()
        {
            Debug.WriteLine("══════════════════════════════════════════════════════════════");
            Debug.WriteLine($"📧 Conversation updated: {updatedAt:G}");
            Debug.WriteLine("Participants:");
            foreach (var e in email)
                Debug.WriteLine($"   • {e}");
            Debug.WriteLine("Messages:");
            int index = 1;
            foreach (var msg in content)
            {
                Debug.WriteLine($"   #{index++} From: {msg.From}");
                Debug.WriteLine($"      To: {msg.To}");
                Debug.WriteLine($"      Text: {msg.text}");
            }
            Debug.WriteLine("══════════════════════════════════════════════════════════════");
        }
    }
}
