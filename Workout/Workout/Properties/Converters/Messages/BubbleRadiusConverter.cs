using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace Workout.Properties.Converters
{
    internal class BubbleRadiusConverter : IValueConverter
    {
        private const float Big = 18;
        private const float Small = 5;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ChatMessage m)
                return new RoundRectangle { CornerRadius = new CornerRadius(Big) };

            // Küldött üzenet (jobb oldal)
            if (m.IsSentByUser)
            {
                return new RoundRectangle
                {
                    CornerRadius = new CornerRadius(
                        Big,                              // bal felső
                        m.IsFirstInGroup ? Big : Small,   // jobb felső
                        Big,                              // bal alsó
                        m.IsLastInGroup ? Big : Small     // jobb alsó
                    )
                };
            }

            // Fogadott üzenet (bal oldal)
            return new RoundRectangle
            {
                CornerRadius = new CornerRadius(
                    m.IsFirstInGroup ? Big : Small,       // bal felső
                    Big,                                  // jobb felső
                    m.IsLastInGroup ? Big : Small,        // bal alsó
                    Big                                   // jobb alsó
                )
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    }
