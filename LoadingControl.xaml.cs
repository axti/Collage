using System.Windows;
using System.Windows.Media;

namespace InstaCollage
{
    public partial class LoadingControl
    {
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(LoadingControl), new PropertyMetadata(true, IsIndeterminatePropertyChanged));

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register(
                "ProgressValue", typeof(double), typeof(LoadingControl), new PropertyMetadata(0.0, ProgressValuePropertyChanged));

        public static readonly DependencyProperty ProgressBarMessageProperty =
            DependencyProperty.Register(
            "ProgressBarMessage", typeof(string), typeof(LoadingControl), new PropertyMetadata("Загрузка...", ProgressBarMessagePropertyChanged));

        public static readonly DependencyProperty ForegroundBrushProperty =
            DependencyProperty.Register("ForegroundBrush", typeof(Brush), typeof(LoadingControl), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 48, 48, 48))));

        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(LoadingControl), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public Brush ForegroundBrush
        {
            get
            {
                return (Brush)GetValue(ForegroundBrushProperty);
            }
            set
            {
                SetValue(ForegroundBrushProperty, value);
            }
        }

        public Brush BackgroundBrush
        {
            get
            {
                return (Brush)GetValue(BackgroundBrushProperty);
            }
            set
            {
                SetValue(BackgroundBrushProperty, value);
            }
        }

        public LoadingControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public string ProgressBarMessage
        {
            get
            {
                return (string)GetValue(ProgressBarMessageProperty);
            }
            set
            {
                SetValue(ProgressBarMessageProperty, value);
            }
        }

        public double ProgressValue
        {
            get
            {
                return (double)GetValue(ProgressValueProperty);
            }
            set
            {
                SetValue(IsIndeterminateProperty, value);
            }
        }

        public bool IsIndeterminate
        {
            get
            {
                return (bool)GetValue(IsIndeterminateProperty);
            }
            set
            {
                SetValue(IsIndeterminateProperty, value);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            _progressBar.IsIndeterminate = IsIndeterminate;
            _progressBar.Value = ProgressValue;
            _progressBarMessage.Text = ProgressBarMessage;
            _progressBar.Foreground = ForegroundBrush;
            _progressBarMessage.Foreground = ForegroundBrush;
            LoadingControlLayoutRoot.Background = BackgroundBrush;
        }

        private static void ProgressValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LoadingControl)d).UpdateProgressBar();
        }

        private static void IsIndeterminatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LoadingControl)d).UpdateProgressBar();
        }

        private static void ProgressBarMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LoadingControl)d).UpdateProgressBar();
        }
    }
}