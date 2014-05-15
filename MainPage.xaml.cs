using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using InstaCollage.Resources;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Storage.Streams;
using System.IO;
using System.Windows.Media;
using System.IO.IsolatedStorage;
using Windows.System.Threading;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Threading;
using Microsoft.Phone.Info;
using Venetasoft.WP.Net;
using Windows.Storage;
using Microsoft.Xna.Framework.Media;


namespace InstaCollage
{
    public partial class MainPage : BasePhonePage
    {
        //Instagram Client_id
        private const string CLIENT_ID = "6f04938dbc5e4e02982cce1523146b60";
        private const int LOW_RESOLUTION = 306;
        private const int THUMB_RESOLUTION = 150;
        private const int STANDARD_RESOLUTION = 640;
        private string _username;
        private int _num;
        private int _resolution;
        ObservableCollection<UserMedia> medias;
        IApplicationBarIconButton sendemail, save, choose, settings;

        private bool _isCanGetData;

        public bool IsCanGetData
        {
            get 
            { 
                return _isCanGetData; 
            }
            set 
            {
                _isCanGetData = value;
                NotifyPropertyChanged("IsCanGetData");
            }
        }
                
        // Конструктор
        public MainPage()
        {
            
            InitializeComponent();
            _resolution = SettingsManager.GetSettings<int>("res", 1);
            Loaded += OnLoaded;
            DataContext = this;
            InitializeMemotyTimer();
            IsCanGetData = false;
#if DEBUG
            tbMemory.Visibility = System.Windows.Visibility.Visible;
            nickname.Text = "shmyaktov";
#else
            tbMemory.Visibility = System.Windows.Visibility.Collapsed;
#endif
            IsCanGetData = false;
        }

        private void InitializeMemotyTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromMilliseconds(500);

            timer.Start();

            timer.Tick += delegate
            {

                GC.Collect();
                tbMemory.Text = string.Format("Memory: {0} Mbytes", Int32.Parse(DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage").ToString()) /1024.0 /1024.0);

            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get a dictionary of URI parameters and values.
            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;
            foreach (var p in queryStrings)
            {
                if (p.Key == "ClearBackStack")
                    if (bool.Parse(p.Value))
                    {
                        while (NavigationService.RemoveBackEntry() != null)
                        {
                            ; // ничего не делать
                        }
                    }
            }

            base.OnNavigatedTo(e);
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Customize the ApplicationBar Buttons by providing the right text
            if (null != ApplicationBar)
            {
                ApplicationBar.IsMenuEnabled = false;
                ApplicationBar.BackgroundColor = Color.FromArgb(255, 255,164, 0);
                ApplicationBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
                foreach (object obj in ApplicationBar.Buttons)
                {
                    IApplicationBarIconButton button = obj as IApplicationBarIconButton;
                    if (null != button)
                    {
                        if ("Send email" == button.Text)
                        {
                            sendemail = button;
                        }
                        else if ("Save" == button.Text)
                        {
                            save = button;
                        }
                        else if ("Check" == button.Text)
                        {
                            choose = button;
                        }
                        else if ("Images quality" == button.Text)
                        {
                            settings = button;
                        }
                    }
                }
            }
        }

        //Return url for get all media files for user by user_id
        private string GetUserMediaURL(string id)
        {
            return string.Format("https://api.instagram.com/v1/users/{0}/media/recent?client_id={1}", id, CLIENT_ID);
        }

        //Return url for get users by user nick_name
        private string GetSearchUserURL(string username)
        {
            return string.Format("https://api.instagram.com/v1/users/search?q={0}&client_id={1}", username, CLIENT_ID );
        }

        //Set SystemTray.ProgressIndicator text status
        private void SetProgress(string text)
        {
            SystemTray.ProgressIndicator.Text = text;
        }

        //Set SystemTray.ProgressIndicator visibility status
        private void SetProgress(bool visibility)
        {
            SystemTray.ProgressIndicator.IsVisible = visibility;
        }
        //Set SystemTray.ProgressIndicator Determinate status
        private void SetProgressD(bool determinate)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = determinate;
        }
        //Set SystemTray.ProgressIndicator Determinate status
        private void SetProgressD(double val)
        {
            SystemTray.ProgressIndicator.Value = val;
        }

        private void SetEnabledButtons(bool val)
        {
            sendemail.IsEnabled = val;
            save.IsEnabled = val; 
            choose.IsEnabled = val;
            settings.IsEnabled = val;
        }

        //Button "Make Collage"
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string user_nick = nickname.Text;
            SetEnabledButtons(false);
            IsCanGetData = false;
            ImageCollage.Stretch = Stretch.None;
            ImageCollage.Source = new BitmapImage(new Uri("/Assets/Instagram_logo.png", UriKind.Relative));
            string url = GetSearchUserURL(user_nick);    
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    SystemTray.ProgressIndicator = new ProgressIndicator() 
                    {
                        IsVisible = true, Text = "Connecting to instagram...", IsIndeterminate = true 
                    };
                    var response = await httpClient.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<InstaResponse>(response);
                    var code = result.meta.code;
                    if (code == 200)
                    {
                        var data = (List<IDictionary<string, object>>)result.data;
                        if (data.Count == 0)
                        {
                            MessageBox.Show("Не найдено ни одного пользователя похожего на " + user_nick, "Измените запрос", MessageBoxButton.OK);
                            SetProgress(false);
                        }
                        else if (data.Count > 1)
                        {
                            MessageBox.Show("Найдено больше 1 пользователя", "Уточните запрос", MessageBoxButton.OK);
                            SetProgress(false);
                        }
                        else
                        {
                            SetProgress("The user is found.");
                            var userinfo = data[0];
                            var username = (string)userinfo["username"];
                            var profile_picture = (string)userinfo["profile_picture"];
                            var full_name = (string)userinfo["full_name"];
                            var id = (string)userinfo["id"];
                            GetUserMedia(id);
                            _username = user_nick;
                        }
                    }
                    else
                    {
                        var message = result.meta.error_message;
                        MessageBox.Show("Что-то пошло не так." + (message.Length > 0 ? "\r\n" + message : String.Empty), "Ошибка", MessageBoxButton.OK);
                        SetProgress(false);
                        IsCanGetData = true;
                    }
                }
                catch (Exception exp)
                {
                    SetProgress(false);
                    MessageBox.Show("Что-то пошло не так, возможно отсутствует интернет соединение?", "Ошибка", MessageBoxButton.OK);
                    IsCanGetData = true;
                    //do nothing
                }
            }
            }
            
            //Get JSON user media by user_id
            private async void GetUserMedia(string id)
            {
                string url = GetUserMediaURL(id);                      // "https://api.instagram.com/v1/users/" + id + "/media/recent";
                using (HttpClient httpClient = new HttpClient())
                {
                    SetProgress("Get user photos...");
                    try
                    {
                        var response = await httpClient.GetStringAsync(url);
                        var result = JsonConvert.DeserializeObject<InstaResponse>(response);
                        var code = result.meta.code;
                        if (code == 200)
                        {
                            var data = (List<IDictionary<string, object>>)result.data;
                            medias = new ObservableCollection<UserMedia>();
                            if (data.Count > 0)
                            {
                                foreach (var info in data)
                                {
                                    if ((string)info["type"] == "image")
                                    {
                                        var p = info["likes"].ToString();
                                        //medias.Add(new UserMedia() { id = (string)info["id"], likes_count = (int)((IDictionary<string, object>)info["likes"])["count"], thumbnail_url = (string)((IDictionary<string, object>)info["likes"])["url"] });
                                        medias.Add(new UserMedia() { id = (string)info["id"], likes_count = (JsonConvert.DeserializeObject<InstaLikes>(info["likes"].ToString())).count, thumbnail_url = (JsonConvert.DeserializeObject<InstaImages>(info["images"].ToString())).thumbnail.url, low_url = (JsonConvert.DeserializeObject<InstaImages>(info["images"].ToString())).low_resolution.url, standard_url = (JsonConvert.DeserializeObject<InstaImages>(info["images"].ToString())).standard_resolution.url, caption = (JsonConvert.DeserializeObject<InstaCaption>(info["caption"].ToString())).text });
                                    }
                                }
                                //LLS_items.ItemsSource = medias;
                                List<UserMedia> templist = GenerateUserMediaList(medias);
                                /*List<UserMedia> ppp = new List<UserMedia>();
                                for (int i = 0; i < 4; i++)
                                { 
                                    ppp.Add(medias.ElementAt(i));
                                }*/
                                await CreateCollage(templist);
                                //SystemTray.ProgressIndicator.IsVisible = false;
                            }
                            else
                            {
                                MessageBox.Show("Изображений не найдено", "Упссс", MessageBoxButton.OK);
                                SetProgress(false);
                                settings.IsEnabled = true;
                                IsCanGetData = true;
                            }
                        }
                        else
                        {
                            var message = result.meta.error_message;
                            MessageBox.Show("Что-то пошло не так." + (message.Length > 0 ? "\r\n" + message : String.Empty), "Ошибка", MessageBoxButton.OK);
                            SetProgress(false);
                            settings.IsEnabled = true;
                            IsCanGetData = true;
                        }
                    }
                    catch (Exception exp)
                    {
                        SetProgress(false);
                        settings.IsEnabled = true;
                        MessageBox.Show("Что-то пошло не так, возможно отсутствует интернет соединение?", "Ошибка", MessageBoxButton.OK);
                        IsCanGetData = true;
                        //do nothing
                    }
                }

            }

            //Return List media depending on the amount of elements
            private List<UserMedia> GenerateUserMediaList(IList<UserMedia> medias)
            {
                List<UserMedia> templist = new List<UserMedia>();
                if (medias.Count >= 36)
                {
                    templist = GetBestMedia(medias, 36);
                }
                else if (medias.Count >= 25)
                {
                    templist = GetBestMedia(medias, 25);
                }
                else if (medias.Count >= 16)
                {
                    templist = GetBestMedia(medias, 16);
                }
                else if (medias.Count >= 9)
                {
                    templist = GetBestMedia(medias, 9);
                }
                else if (medias.Count >= 4)
                {
                    templist = GetBestMedia(medias, medias.Count);
                }
                else if (medias.Count <= 4)
                {
                    templist = medias.ToList();
                }
                return templist;
            }

            //Get only best media by only n elements
            private List<UserMedia> GetBestMedia(IList<UserMedia> medias, int p)
            {
                List<UserMedia> temp = new List<UserMedia>();
                temp = medias.OrderByDescending(x => x.likes_count).ToList();
                if (temp.Count > p)
                    temp = temp.Take(p).ToList();
                return temp;
            }

            public Task<List<BitmapImage>> GetImagesAsync(List<string> requestUri)
            {
                SetProgressD(false);
                SetProgressD(0.0);
                SetProgress("Loading images...");
                double val = 1.0 / (double)requestUri.Count;
                var op = new TaskCompletionSource<List<BitmapImage>>();
                List<BitmapImage> images = new List<BitmapImage>();
                
                foreach (var file in requestUri)
                {
                    // Create a Bitmap from the file and add it to the list                 
                    BitmapImage img = new BitmapImage();

                    WebClient ImageWebClient = new WebClient();
                        ImageWebClient.AllowReadStreamBuffering = true;
                        ImageWebClient.OpenReadCompleted += (o, e) =>
                            {
                                if (e.Error == null)
                                {

                                    if (e.Cancelled != true)
                                    {
                                        img.SetSource(e.Result);
                                        images.Add(img);
                                        SetProgressD(val);
                                        val += val;
                                        SetProgress("Loading images..." + images.Count + " of " + requestUri.Count);
                                        if (images.Count == requestUri.Count)
                                        {
                                            op.SetResult(images);
                                            SetProgressD(true);
                                            SetProgress("Images loading success.");
                                        }
                                    }
                                    else
                                    {
                                        op.SetCanceled();
                                        SetProgressD(true);
                                    }

                                }
                                else
                                {
                                    op.SetException(e.Error);
                                    SetProgressD(true);
                                }
                            };

                        ImageWebClient.OpenReadAsync(new Uri(file));
                }
                return op.Task;
            }

            async Task CreateCollage(IReadOnlyList<UserMedia> files)
            {
                SetProgress("Generate collage...");
                
                if (files.Count() == 0) return;
                
                try
                {
                        // Do a square-root of the number of images to get the
                        // number of images on x and y axis
                        int number = (int)Math.Ceiling(Math.Sqrt((double)files.Count));
                        _num = number;
                        // Calculate the width of each small image in the collage
                        int numberX = (int)(ImageCollage.ActualWidth / number);
                        int numberY = (int)(ImageCollage.ActualHeight / number);
                        // Initialize an empty WriteableBitmap.
                        int thumb = _resolution == 3 ? STANDARD_RESOLUTION : _resolution == 2 ? LOW_RESOLUTION : THUMB_RESOLUTION;  //Can make Low and standard
                        WriteableBitmap destination = new WriteableBitmap(thumb * number, thumb * number);
                        int col = 0; // Current Column Position
                        int row = 0; // Current Row Position
                        List<BitmapImage> images = new List<BitmapImage>();
                        List<string> imageuri = new List<string>();
                        switch (_resolution)
                        {
                            case 1:
                                imageuri = files.Select(x => x.thumbnail_url).ToList();
                                break;
                            case 2:
                                imageuri = files.Select(x => x.low_url).ToList();
                                break;
                            case 3:
                                imageuri = files.Select(x => x.standard_url).ToList();
                                break;
                            default:
                                imageuri = files.Select(x => x.thumbnail_url).ToList();
                                break;
                        }
                        //imageuri = files.Select(x => x.thumbnail_url).ToList();
                        images = await GetImagesAsync(imageuri);
              
                    SetProgress("Make collage file...");
                   
                        using (MemoryStream mem = new MemoryStream())
                        {
                            int tempWidth = 0;   // Parameter for Translate.X 
                            int tempHeight = 0;  // Parameter for Translate.Y 

                            foreach (BitmapImage item in images)
                            {
                                Image image = new Image();
                                image.Height = item.PixelHeight; //(double)numberY;
                                image.Width = item.PixelWidth; //(double)numberX;
                                image.Source = item;
                                image.Stretch = Stretch.UniformToFill;
                                // destination.Render(image, tf); 
                                tempHeight = item.PixelHeight;
                                tempWidth = item.PixelWidth;
                                if (row < number)
                                {
                                    // Resize the in-memory bitmap and Blit (paste) it at the correct tile
                                    // position (row, col)
                                    // TranslateTransform                       
                                    TranslateTransform tf = new TranslateTransform();
                                    tf.X = col * thumb; // numberX;
                                    tf.Y = row * thumb; // numberY;
                                    /*System.Windows.Media.ScaleTransform st = new ScaleTransform();
                                    st.ScaleX = (double)numberX / (double)tempWidth;
                                    st.ScaleY = (double)numberY / (double)tempHeight;*/
                                    TransformGroup tg = new TransformGroup();
                                    tg.Children.Add(tf);
                                    //tg.Children.Add(st);
                                    destination.Render(image, tg);
                                    //destination.Render(image, tf); 

                                    col++;
                                    if (col >= number)
                                    {
                                        row++;
                                        col = 0;
                                    }
                                }
                                image.Source = null;
                                item.UriSource = null;
                            }
                        }

                    images.Clear();
                    SetProgress("End make file.");
                    ImageCollage.Source = destination;
                    ImageCollage.Stretch = Stretch.Fill;
                    ((WriteableBitmap)ImageCollage.Source).Invalidate();
                    //ImageCollage.Visibility = System.Windows.Visibility.Visible;
                    SetProgress(false);
                    //destination = null;
                    SetEnabledButtons(true);
                    IsCanGetData = true;
                }
                catch (Exception ex)
                {
                    SetProgress(false);
                    settings.IsEnabled = true;
                    IsCanGetData = true;
                    MessageBox.Show("Что-то пошло не так." + (ex.Message.Length > 0 ? ("\r\n" + ex.Message) : String.Empty ), "Ошибка", MessageBoxButton.OK);
                }
            }

            //Save Image to Media Gallery
            private void SaveImageButton_Click(object sender, EventArgs e)
            {
                try
                {
                    string res = _resolution == 3 ? "standard" : _resolution == 2 ? "low" : "thumb";
                    string fileName = string.Format("MyCollage_{0}_{1}x{1}_{2}_by_InstaCollage.jpg", _username, _num, res);
                    WriteableBitmap wb = (WriteableBitmap)ImageCollage.Source;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        wb.SaveJpeg(ms, wb.PixelWidth, wb.PixelHeight, 0, 80);
                        using (MediaLibrary lib = new MediaLibrary())
                        {
                            lib.SavePicture(fileName, ms.ToArray());
                        }
                    }
                    MessageBox.Show("Файл сохранен \r\n" + fileName, "Save successful", MessageBoxButton.OK);
                }
                catch(Exception exp)
                {
                    MessageBox.Show("Что-то пошло не так." + (exp.Message.Length > 0 ? ("\r\n" + exp.Message) : String.Empty), "Ошибка", MessageBoxButton.OK);
                }
            }

            //Choose Image Quality
            private void SettingsButton_Click(object sender, EventArgs e)
            {
                MessagePrompt p = new MessagePrompt();
                p.Body = new qualitysettings();
                p.Title = "Image Quality Settings";
                p.Foreground = new SolidColorBrush(Colors.Black);
                p.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                var list = p.ActionPopUpButtons;
                list[0].BorderBrush = new SolidColorBrush( Colors.Black);
                list[0].Foreground = new SolidColorBrush(Colors.Black);
                p.Show();
            }
            void messagePrompt_Completed(object sender, PopUpEventArgs<object, PopUpResult> e)
            {
                _resolution = SettingsManager.GetSettings<int>("res", 1);
            }

            private async void listPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.AddedItems.Count > 0)
                {
                    try
                    {
                        SystemTray.ProgressIndicator = new ProgressIndicator()
                        {
                            IsVisible = true,
                            Text = "Make collage...",
                            IsIndeterminate = true
                        };
                       // SetProgress(true);
                        SetEnabledButtons(false);
                        ImageCollage.Stretch = Stretch.None;
                        ImageCollage.Source = new BitmapImage(new Uri("/Assets/Instagram_logo.png", UriKind.Relative));
                        List<UserMedia> templist = GenerateUserMediaList(e.AddedItems.OfType<UserMedia>().ToList<UserMedia>());
                        await CreateCollage(templist);
                    }
                    catch(Exception exp)
                    {
                        SetProgress(false);
                        settings.IsEnabled = true;
                        MessageBox.Show("Что-то пошло не так." + (exp.Message.Length > 0 ? ("\r\n" + exp.Message) : String.Empty), "Не удалось сохранить файл", MessageBoxButton.OK);
                    }
                }
                
            }

            private void CheckButton_Click(object sender, EventArgs e)
            {
                this.listPicker2.ItemsSource = medias;
                if (null != this.listPicker2.SelectedItems)
                    this.listPicker2.SelectedItems.Clear();
                this.listPicker2.Open();
            }

            private void SendEmailButton_Click(object sender, EventArgs e)
            {
                try
                {
                    long _size;
                    string res = _resolution == 3 ? "standard" : _resolution == 2 ? "low" : "thumb";
                    string fileName = string.Format("MyCollage_{0}_{1}x{1}_{2}_by_InstaCollage.jpg", _username, _num, res);
                     //ApplicationData.Current.LocalFolder.Path
                    WriteableBitmap wb = (WriteableBitmap)ImageCollage.Source;
                    _size = SaveFileToIsolatedStorage(fileName, wb);
                   /* using (MemoryStream ms = new MemoryStream())
                    {
                        wb.SaveJpeg(ms, wb.PixelWidth, wb.PixelHeight, 0, 80);
                        _size = ms.Length;
                        await SaveFileToIsolatedStorage(fileName, wb);
                    }*/
                    NavigationService.Navigate(new Uri(String.Format("/ChooseEmailAccountTypePage.xaml?FileName={0}&FileSize={1}", fileName, _size), UriKind.Relative));
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Что-то пошло не так." + (exp.Message.Length > 0 ? ("\r\n" + exp.Message) : String.Empty), "Ошибка", MessageBoxButton.OK);
                }
            }

            private static long SaveFileToIsolatedStorage(string fileName, WriteableBitmap image)
            {
                long Size = 0;
                using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isolatedStorage.FileExists(fileName))
                        isolatedStorage.DeleteFile(fileName);
                    using (var stream = isolatedStorage.OpenFile(fileName, FileMode.Create))
                    {
                        Extensions.SaveJpeg(image, stream, image.PixelWidth, image.PixelHeight, 0, 100);
                        Size = stream.Length;
                    }
                }
                return Size;
            }

            private void nickname_TextChanged(object sender, TextChangedEventArgs e)
            {
                IsCanGetData = (nickname.Text.Length > 0);
            }
    }


   

    //Help classes
    public class UserMedia
    {
        public string id { get; set; }
        public string thumbnail_url { get; set; }
        public string low_url { get; set; }
        public string standard_url { get; set; }
        public string caption { get; set; }
        public int likes_count { get; set; }
        public UserMedia()
        { }
        public UserMedia(string id, string thumbnail_url, int likes_count, string standard_url, string low_url, string caption)
         {
             this.id = id;
             this.thumbnail_url = thumbnail_url;
             this.low_url = low_url;
             this.standard_url = standard_url;
             this.likes_count = likes_count;
             this.caption = caption;
         }
    }
    public class InstaResponse
    {
        public InstaMeta meta { get; set; }
        public List<IDictionary<string, object>> data { get; set; }

    }
    public class InstaMeta
    {
        public int code { get; set; }
        public string error_message { get; set; }
    }
    public class InstaMedia 
    {
        public InstaLikes likes { get; set; }
        public InstaImages images { get; set; }
        public string type { get; set; }
        public string id { get; set; }
    }
    public class InstaLikes
    {
        public int count { get; set; }
    }
    public class InstaImages
    {
        public InstaImageData low_resolution { get; set; }
        public InstaImageData thumbnail { get; set; }
        public InstaImageData standard_resolution { get; set; }
    }
    public class InstaImageData
    {
        public string url { get; set; }
        public int width{ get; set; }
        public int height{ get; set; }
    }
    public class InstaCaption
    {
        public string text { get; set; }
    }
}