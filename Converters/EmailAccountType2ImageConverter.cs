using System;
using System.Globalization;
using System.Windows.Data;

namespace InstaCollage.Converters
{
    public class EmailAccountType2ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var emailAccountType = (EmailAccountType)value;
            switch (emailAccountType)
            {
                case EmailAccountType.Gmail:
                    return "/Assets/gmail.png";
                case EmailAccountType.Microsoft:
                    return "/Assets/Live-Hotmail-Metro-256.png";
                case EmailAccountType.Mailru:
                    return "/Assets/mail.ru.png";
                case EmailAccountType.Yandex:
                    return "/Assets/ya_mail.png";
                case EmailAccountType.Custom:
                    return "/Assets/Live-Mail-Metro-256.png";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}