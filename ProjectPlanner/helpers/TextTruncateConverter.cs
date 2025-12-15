using System.Globalization;
using Microsoft.Maui.Controls;
using ProjectPlanner.Model;

namespace ProjectPlanner.helpers
{
    public class TextTruncateConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrEmpty(text)) return string.Empty;

            string cleanedText = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

            int maxLength = 20;
            if (parameter is string paramStr && int.TryParse(paramStr, out int parsedInt))
            {
                maxLength = parsedInt;
            }

            if (cleanedText.Length <= maxLength) return cleanedText;
            return cleanedText.Substring(0, maxLength) + "...";
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
            bool isDone = value is bool b && b;

            if (isDone)
            {
                if (Application.Current != null && Application.Current.Resources.TryGetValue("NeonAccent", out var neonColor))
                    return (Color)neonColor;
                return Colors.Lime;
            }
            else
            {
                if (Application.Current != null && Application.Current.Resources.TryGetValue("TextSecondary", out var grayColor))
                    return (Color)grayColor;
                return Colors.Gray;
            }
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
            if (value is SubTask task)
            {
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

            return "[ ]";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}