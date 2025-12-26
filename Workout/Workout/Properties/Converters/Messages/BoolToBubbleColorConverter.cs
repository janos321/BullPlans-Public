using System.Globalization;

namespace Workout.Properties.Converters
{
    public class BoolToBubbleColorConverter : IValueConverter
    {
        // Definiáld a színeket (ezeket akár kívülről is beadhatnád)
        private readonly Color SentColor = Color.FromRgb(205, 92, 92);
        private readonly Color ReceivedColor = Color.FromRgb(165, 42, 42);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Ha IsSentByUser = true, akkor a küldött szín
            return (bool)value ? SentColor : ReceivedColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
