using System;
using System.Globalization;

namespace ProjectPlanner.helpers
{
    public class TextTruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;

            if (string.IsNullOrEmpty(text))
                return string.Empty;
            string cleanedText = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            

            int maxLength = 20; 
            if (parameter is string paramStr && int.TryParse(paramStr, out int parsedInt))
            {
                maxLength = parsedInt;
            }

            if (cleanedText.Length <= maxLength)
                return cleanedText;

            return cleanedText.Substring(0, maxLength) + "...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}