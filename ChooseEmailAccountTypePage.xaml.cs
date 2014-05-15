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
    public partial class ChooseEmailAccountTypePage : BasePhonePage
    {
        public ChooseEmailAccountTypePage()
        {
            InitializeComponent();
            DataContext = this;
            _accountTypes.SelectedIndex = -1;
        }

        public string FileName { get; set; }
        public string FileSize { get; set; }


        public IEnumerable<EmailAccountType> AccountTypes
        {
            get
            {
                return Enum.GetValues(typeof(EmailAccountType)).OfType<EmailAccountType>();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _accountTypes.SelectedIndex = -1;
            // Get a dictionary of URI parameters and values.
            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;
            foreach (var p in queryStrings)
            {
                if (p.Key == "FileName")
                    FileName = p.Value;
                if (p.Key == "FileSize")
                    FileSize = p.Value;
            }

            base.OnNavigatedTo(e);
        }


        private void _accountTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                NavigationService.Navigate(new Uri(String.Format("/EmailLoginPage.xaml?FileName={0}&FileSize={1}&AccountType={2}", FileName, FileSize, e.AddedItems[0]), UriKind.Relative));
            }
        }
    }
}