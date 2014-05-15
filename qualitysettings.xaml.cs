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
    public partial class qualitysettings : UserControl
    {
        int _resolution;
        public qualitysettings()
        {
            InitializeComponent();
            _resolution = SettingsManager.GetSettings<int>("res", 1);
            switch (_resolution)
            {
                case 3:
                    rb3.IsChecked = true;
                    break;
                case 2:
                    rb2.IsChecked = true;
                    break;
                case 1:
                default:
                    rb1.IsChecked = true;
                    break;
            }
        }

        private void rb1_Checked(object sender, RoutedEventArgs e)
        {
            SettingsManager.SaveSettings<int>("res", 1);
        }
        private void rb2_Checked(object sender, RoutedEventArgs e)
        {
            SettingsManager.SaveSettings<int>("res", 2);
        }
        private void rb3_Checked(object sender, RoutedEventArgs e)
        {
            SettingsManager.SaveSettings<int>("res", 3);
        }

    }
}
