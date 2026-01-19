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

            // Show status indicator based on Status property
            return task.Status switch
            {
                SubTaskStatus.Done => "[X]",
                SubTaskStatus.Ongoing => "[►]",
                _ => $"[{PriorityNames[Math.Clamp(task.Priority, 0, PriorityNames.Length - 1)]}]"
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = value switch
            {
                SubTaskStatus s => s,
                SubTask task => task.Status,
                _ => SubTaskStatus.None
            };

            return status switch
            {
                SubTaskStatus.Done => Application.Current?.Resources.TryGetValue("NeonAccent", out var done) == true
                    ? (Color)done : Colors.Lime,
                SubTaskStatus.Ongoing => Application.Current?.Resources.TryGetValue("OngoingStatus", out var ongoing) == true
                    ? (Color)ongoing : Colors.Cyan,
                _ => Application.Current?.Resources.TryGetValue("TextSecondary", out var none) == true
                    ? (Color)none : Colors.Gray
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var status = value switch
            {
                SubTaskStatus s => s,
                SubTask task => task.Status,
                _ => SubTaskStatus.None
            };

            return status switch
            {
                SubTaskStatus.Done => "●",
                SubTaskStatus.Ongoing => "►",
                _ => "○"
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TaskPriorityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not SubTask task)
                return null;

            if (parameter is string paramStr && int.TryParse(paramStr, out var priority))
            {
                return (task, priority);
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}