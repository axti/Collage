using System;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Animation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Collections.Generic;
using System.Windows;
using System.IO.IsolatedStorage;
using Windows.Storage;
using Venetasoft.WP.Net;
using Venetasoft.WP7;
using System.Windows.Data;

namespace InstaCollage
{
    public partial class ComposeEmailPage : BasePhonePage
    {
        public ComposeEmailPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            Loaded += (sender, args) => emailAddress.Focus();
            _event = new ManualResetEvent(false);
            IsSending = false;
            DataContext = this;
            _emailAddressChooserTask = new EmailAddressChooserTask();
            _emailAddressChooserTask.Completed += EmailAddressChooserOnCompleted;
        }

        
        
        private readonly ManualResetEvent _event;

        private string _lastSendResult;

        private string _emailTo;

        private string _emailSubject;

        private string _emailBody;

        private string _attachmentFileName;

        private long _attachmentFileSize;

        private bool _isSending;

        private int _sendingProgressValue;

        private readonly EmailAddressChooserTask _emailAddressChooserTask;

        public string EmailTo
        {
            get
            {
                return _emailTo;
            }
            set
            {
                if (value == _emailTo)
                    return;
                if (null != sendAppBarIconButton)
                    sendAppBarIconButton.IsEnabled = IsCanSend;
                _emailTo = value;
                NotifyPropertyChanged("EmailTo");
            }
        }

        public string EmailSubject
        {
            get
            {
                return _emailSubject;
            }
            set
            {
                if (value == _emailSubject)
                    return;

                _emailSubject = value;
                NotifyPropertyChanged("EmailSubject");
            }
        }

        public string EmailBody
        {
            get
            {
                return _emailBody;
            }
            set
            {
                if (value == _emailBody)
                    return;

                _emailBody = value;
                NotifyPropertyChanged("EmailBody");
            }
        }

        public bool IsSending
        {
            get
            {
                return _isSending;
            }
            private set
            {
                if (value == _isSending)
                    return;

                _isSending = value;
                NotifyPropertyChanged("IsSending");
            }
        }
        public int SendingProgressValue
        {
            get
            {
                return _sendingProgressValue;
            }
            private set
            {
                if (value == _sendingProgressValue)
                    return;

                _sendingProgressValue = value;
                NotifyPropertyChanged("SendingProgressValue");
            }
        }

        public EmailAccountType AccountType { get; set; }
        public string AccountHost{ get; set; }
        public int AccountPort { get; set; }
        public bool AccountSSL { get; set; }

        public string AttachmentFileName
        {
            get
            {
                return _attachmentFileName;
            }
            set
            {
                if (string.Equals(value, _attachmentFileName, StringComparison.CurrentCulture))
                    return;

                _attachmentFileName = value;
                NotifyPropertyChanged("AttachmentFileName");
            }
        }
        private ApplicationBarIconButton sendAppBarIconButton;

        public string AttachmentFileType { get { return "JPG";} }
        
        public string EmailAddress { get; set; }

        public string EmailPassword { get; set; }

        public long AttachmentFileSize
        {
            get
            {
                return _attachmentFileSize;
            }
            set
            {
                if (value == _attachmentFileSize)
                    return;

                _attachmentFileSize = value;
                NotifyPropertyChanged("AttachmentFileSize");
            }
        }

        public void AddContacts()
        {
            _emailAddressChooserTask.Show();
        }

        private void EmailAddressChooserOnCompleted(object sender, EmailResult emailResult)
        {
            EmailTo = emailResult.Email;
        }

        private void BuildLocalizedApplicationBar()
        {

            var applicationBar = new ApplicationBar();
            applicationBar.IsVisible = true;
            applicationBar.IsMenuEnabled = true;
            applicationBar.Mode = ApplicationBarMode.Default;
            applicationBar.BackgroundColor = Color.FromArgb(255, 255, 164, 0);
            applicationBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);

            sendAppBarIconButton =
                new ApplicationBarIconButton(new Uri("/Assets/Icons/sendemail.png", UriKind.Relative));
            sendAppBarIconButton.Click += SendAppBarIconButtonClicked;
            sendAppBarIconButton.Text = "отправить";
            sendAppBarIconButton.IsEnabled = false;
            applicationBar.Buttons.Add(sendAppBarIconButton);
            ApplicationBar = applicationBar;
        }

        public bool IsCanSend
        {
            get
            {
                return !string.IsNullOrEmpty(EmailAddress) && EmailAddress.Contains("@");
            }
        }

        private void SendAppBarIconButtonClicked(object sender, EventArgs eventArgs)
        {
            // remove focus from all textboxes by focusing on page
            Focus();
            SendEmail();

        }
        private void _emailAddress_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                emailSubject.Focus();
        }

        private void _emailSubject_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                emailBody.Focus();
        }

        private void ImageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddContacts();
        }

        public async void SendEmail()
        {
            if (IsSending)
                return;

            if (EmailTo.Contains(";"))
            {
                MessageBox.Show("Вы не можете отправлять письма более чем одному получателю.", "Внимание", MessageBoxButton.OK);
                return;
            }

            IsSending = true;
            _animateEmailSendingStoryboard.Begin();
            SendingProgressValue = 0;
            try
            {
                 var sendingResult = await SendFileByEmail(
                     AttachmentFileName,
                     AttachmentFileSize,
                     EmailAddress,
                     EmailPassword,
                     EmailTo,
                     EmailSubject,
                     EmailBody);
                
                if (!string.IsNullOrEmpty(sendingResult))
                    MessageBox.Show(sendingResult, "Произошла ошибка :'-(", MessageBoxButton.OK);
            }
            finally
            {
                IsSending = false;
                _animateEmailSentStoryboard.Begin();
            }

            // Wait until animation finish
            await Task.Factory.StartNew(() => Thread.Sleep(TimeSpan.FromMilliseconds(600)));
            NavigationService.Navigate(new Uri("/MainPage.xaml?ClearBackStack=true", UriKind.Relative));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string FileName = string.Empty;
            long FileSize = 0;
            // Get a dictionary of URI parameters and values.
            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;
            foreach (var p in queryStrings)
            {
                if (p.Key == "FileName")
                    FileName = p.Value;
                if (p.Key == "FileSize")
                    FileSize = long.Parse(p.Value);
                if (p.Key == "EmailAddress")
                    EmailAddress = p.Value;
                if (p.Key == "EmailPassword")
                    EmailPassword = p.Value;
                if (p.Key == "AccountType")
                    AccountType = (EmailAccountType)Enum.Parse(typeof(EmailAccountType), p.Value);
                if (p.Key == "AccountHost")
                    AccountHost = p.Value;
                if (p.Key == "AccountPort")
                    AccountPort = int.Parse(p.Value);
                if (p.Key == "AccountSSL")
                    AccountSSL = bool.Parse(p.Value);
            }
            UpdateAttachmentUIInfo(FileName, FileSize);

            base.OnNavigatedTo(e);
        }

        private void UpdateAttachmentUIInfo(string name, long size)
        {
            if (size >= 25000000)
            {
                //_showMessageService.ShowWarn("The file you are trying to send exceeds the 25 MB attachment limit. But don't worry, you can use SkyDrive instead.");
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }

            if (string.IsNullOrEmpty(name))
            {
                //_showMessageService.ShowError(AppResources.MessageBox_SomethingWentWrong);
            }
            else
            {
                AttachmentFileName = name;
                AttachmentFileSize = size;
            }
        }

        public async Task<string> SendFileByEmail(
            string filename, long filesize,
            string emailAddress,
            string password,
            string mailTo,
            string subject,
            string body)
        {
            _lastSendResult = string.Empty;
            _event.Reset();
            await Task.Factory.StartNew(
                () =>
                {
                    string fullFilePath;
                    using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        fullFilePath = GetFullFilePath(filename);
                    }

                    if (fullFilePath == null)
                    {
                        _lastSendResult = "Looks like we lost file during our way here :( Please, make sure file exists and try again.";
                        return;
                    }

                    try
                    {
                        var email = new MailMessage();
                        try
                        {
                            //email.LicenceKey = MainMessageLicenseKey;
                            email.Email = emailAddress;
                            email.Password = password;
                            switch (AccountType)
                            {
                                case EmailAccountType.Gmail:
                                    email.AccountType = MailMessage.accountType.Gmail;
                                    break;
                                case EmailAccountType.Microsoft:
                                    email.AccountType = MailMessage.accountType.MicrosoftAccount;
                                    break;
                                case EmailAccountType.Yandex:
                                    email.AccountType = MailMessage.accountType.Custom;
                                    email.SetCustomSMTPServer("smtp.yandex.ru", 587, false);
                                    break;
                                case EmailAccountType.Mailru:
                                    email.AccountType = MailMessage.accountType.Custom;
                                    email.SetCustomSMTPServer("smtp.mail.ru", 465, true);
                                    break;
                                case EmailAccountType.Custom:
                                    email.AccountType = MailMessage.accountType.Custom;
                                    email.SetCustomSMTPServer(AccountHost, AccountPort, AccountSSL);
                                    break;
                                default:
                                    email.AccountType = MailMessage.accountType.Unknown;
                                    break;
                            }
                            
                            email.To =  mailTo;
                            email.Subject = string.IsNullOrEmpty(subject) ? string.Empty : subject;
                            email.Body = string.IsNullOrEmpty(body) ? string.Empty : body;
                            email.AddAttachment(fullFilePath);
                            //email.SetCustomSMTPServer();
                            email.Error += EmailOnError;
                            email.Progress += EmailOnProgress;
                            email.MailSent += MailSent;
                            email.Send();
                            _event.WaitOne();
                        }
                        finally
                        {
                            DeleteTemporaryFile(fullFilePath);
                            email.Error -= EmailOnError;
                            email.Progress -= EmailOnProgress;
                            email.MailSent -= MailSent;
                            _lastSendResult = email.LastError;
                        }
                    }
                    catch (Exception ex)
                    {
                        _lastSendResult = "Извините, но что-то пошло не так :(";
                    }
                });

            return _lastSendResult;
        }

        private void DeleteTemporaryFile(string tempFileName)
        {
            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isolatedStorage.FileExists(tempFileName))
                    isolatedStorage.DeleteFile(tempFileName);
            }
        }

        private void MailSent(object sender, ValueEventArgs<bool> valueEventArgs)
        {
            _event.Set();
        }

        private void EmailOnProgress(object sender, ValueEventArgs<int> valueEventArgs)
        {
            SendingProgressValue = valueEventArgs.Value;
        }
        private void EmailOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            _event.Set();
        }

        private string GetFullFilePath(string FileName)
        {
            return string.Format(
                "{0}\\{1}", ApplicationData.Current.LocalFolder.Path, FileName);
        }


    }
}

