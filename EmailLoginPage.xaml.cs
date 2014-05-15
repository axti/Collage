using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace InstaCollage
{
    public partial class EmailLoginPage : BasePhonePage
    {
        private string _emailAddress;
        private string _accountHost;
        private string _accounPort;
        private bool _accountSsl;

        private string _emailPassword;

        private bool _showPassword;

        private bool _saveInfo;

        public EmailLoginPage()
        {
            InitializeComponent();
            EmailAddress = string.Empty;
            EmailPassword = string.Empty;
            AccountHost = string.Empty;
            AccountPort = string.Empty;
            AccountType = default(EmailAccountType);
            ShowPassword = false;
            SaveInfo = true;
            AccountSSL = false;
            DataContext = this;
        }

        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }
            set
            {
                _emailAddress = value;
                NotifyPropertyChanged("EmailAddress");
                NotifyPropertyChanged("IsCanNavigateNext");
            }
        }

        public string AccountHost
        {
            get
            {
                return _accountHost;
            }
            set
            {
                _accountHost = value;
                NotifyPropertyChanged("AccountHost");
                NotifyPropertyChanged("IsCanNavigateNext");
            }
        }

        public string AccountPort
        {
            get
            {
                return _accounPort;
            }
            set
            {
                _accounPort = value;
                NotifyPropertyChanged("AccountPort");
                NotifyPropertyChanged("IsCanNavigateNext");
            }
        }

        public bool AccountSSL
        {
            get
            {
                return _accountSsl;
            }
            set
            {
                _accountSsl = value;
                NotifyPropertyChanged("AccountSSL");
            }
        }

        public bool IsCanNavigateNext
        {
            get
            {
                return (AccountType != EmailAccountType.Custom) ? (!string.IsNullOrEmpty(EmailAddress) && EmailAddress.Contains("@") && !string.IsNullOrEmpty(EmailPassword)) : (!string.IsNullOrEmpty(EmailAddress) && EmailAddress.Contains("@") && !string.IsNullOrEmpty(EmailPassword) && !string.IsNullOrEmpty(AccountHost) && !string.IsNullOrEmpty(AccountPort));
               // return !string.IsNullOrEmpty(EmailAddress) && EmailAddress.Contains("@") && !string.IsNullOrEmpty(EmailPassword);
            }
        }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public EmailAccountType AccountType { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get a dictionary of URI parameters and values.
            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;
            foreach (var p in queryStrings)
            {
                if (p.Key == "FileName")
                    FileName = p.Value;
                if (p.Key == "FileSize")
                    FileSize= p.Value;
                if (p.Key == "AccountType")
                    AccountType = (EmailAccountType)Enum.Parse(typeof(EmailAccountType), p.Value);
                
            }
            if (AccountType == EmailAccountType.Custom)
                CustomGrid.Visibility = System.Windows.Visibility.Visible;
            base.OnNavigatedTo(e);
        }

        public string EmailPassword
        {
            get
            {
                return _emailPassword;
            }
            set
            {
                _emailPassword = value;
                NotifyPropertyChanged("EmailPassword");
                NotifyPropertyChanged("IsCanNavigateNext");
            }
        }
        public bool ShowPassword
        {
            get
            {
                return _showPassword;
            }
            set
            {
                _showPassword = value;
                NotifyPropertyChanged("ShowPassword");
            }
        }

        public bool SaveInfo
        {
            get
            {
                return _saveInfo;
            }
            set
            {
                _saveInfo = value;
                NotifyPropertyChanged("SaveInfo");
            }
        }

        private void NavigateNext(object sender, RoutedEventArgs e)
        {
            if (AccountType == EmailAccountType.Custom)
            {
                NavigationService.Navigate(new Uri(String.Format("/ComposeEmailPage.xaml?EmailAddress={0}&EmailPassword={1}&FileName={2}&FileSize={3}&AccountType={4}&AccountHost={5}&AccountPort={6}&AccountSSL={7}", EmailAddress, EmailPassword, FileName, FileSize, AccountType, AccountHost, AccountPort, AccountSSL), UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri(String.Format("/ComposeEmailPage.xaml?EmailAddress={0}&EmailPassword={1}&FileName={2}&FileSize={3}&AccountType={4}", EmailAddress, EmailPassword, FileName, FileSize, AccountType), UriKind.Relative));
            }
        }

        private void SaveEmailAccountInfoIfNeeded()
        {
            if (!SaveInfo)
            {
                return;
            }
            AddOrUpdateEmailAccount(EmailAddress, EmailPassword);
        }

        private void AddOrUpdateEmailAccount(string EmailAddress, string EmailPassword)
        {
            return;
        }
    }
}