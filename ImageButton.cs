using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InstaCollage
{
    public class ImageButton : Button
    {
        public static readonly DependencyProperty BackgroundImageSourceProperty =
            DependencyProperty.Register("BackgroundImageSource", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));

        public ImageSource BackgroundImageSource
        {
            get { return (ImageSource)GetValue(BackgroundImageSourceProperty); }
            set { SetValue(BackgroundImageSourceProperty, value); }
        }
    }
}
