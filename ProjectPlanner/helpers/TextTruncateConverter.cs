using System.Globalization;
using Microsoft.Maui.Controls;
using ProjectPlanner.Model;

namespace ProjectPlanner.helpers
{
    public class TextTruncateConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string text || string.IsNullOrEmpty(text))
                return string.Empty;

            var cleanedText = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

            var maxLength = 20;
            if (parameter is string paramStr && int.TryParse(paramStr, out var parsedInt))
            {
                maxLength = parsedInt;
            }

            if (cleanedText.Length <= maxLength) return cleanedText;
            return string.Concat(cleanedText.AsSpan(0, maxLength), "...");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isDone && isDone) return "[X]";
            return "[ ]";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var isDone = value is bool b && b;

            if (isDone)
            {
                if (Application.Current?.Resources.TryGetValue("NeonAccent", out var neonColor) == true)
                    return (Color)neonColor;
                return Colors.Lime;
            }

            if (Application.Current?.Resources.TryGetValue("TextSecondary", out var grayColor) == true)
                return (Color)grayColor;
            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNotNullConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null && !string.IsNullOrWhiteSpace(value.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TaskStatusBadgeConverter : IValueConverter
    {
        private static readonly string[] PriorityNames =
            { "NONE", "LOW", "MEDIUM", "HIGH", "URGENT", "CRITICAL" };

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not SubTask task)
            {
                return "[ ]";
            }

            if (task.IsDone)
            {
                return "[X]";
            }

            var index = task.Priority;
            if (index < 0 || index >= PriorityNames.Length)
            {
                index = 0;
            }

            return $"[{PriorityNames[index]}]";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}