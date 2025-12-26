using System.Globalization;
using static Workout.ChatMessage;

namespace Workout.Properties.Converters
{
    internal class BubblePaddingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ChatMessage m)
                return new Thickness(10, 0);

            return new Thickness(10, m.IsFirstInGroup ? 7 : -1, 10, m.IsLastInGroup ? 7 : -1);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
