using System;
using System.Globalization;
using System.Windows.Data;

namespace InstaCollage.Converters
{
    public class Size2DisplayStringConverter : IValueConverter
    {
        private const string TwoDecimalPlaces = "{0:0.00}";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (long)value;
            if (size > (long)Sizes.MB)
            {
                var displaySizeMb = size / (double)Sizes.MB;
                return string.Format(TwoDecimalPlaces + "{1}", displaySizeMb, Sizes.MB);
            }

            var displaySizeKb = size / (double)Sizes.KB;
            return string.Format(TwoDecimalPlaces + "{1}", displaySizeKb, Sizes.KB);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private enum Sizes
        {
            KB = 1024,
            MB = 1024 * 1024,
        }
    }
}
