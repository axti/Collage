using Microsoft.Phone.Controls;
using System;
using System.ComponentModel;
using System.Windows;


namespace InstaCollage
{
    public abstract class BasePhonePage : PhoneApplicationPage, INotifyPropertyChanged
    {
        public BasePhonePage() { 
        }
        protected void Save() {  }
        protected void Cancel() {  }
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raise the PropertyChanged event and pass along the property that changed
        /// </summary>
        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                });
            }
        }
    }
}
