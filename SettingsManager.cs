using System;
using System.IO.IsolatedStorage;


namespace InstaCollage
{
    public static class SettingsManager
    {
        readonly static IsolatedStorageSettings _settings = IsolatedStorageSettings.ApplicationSettings;

        public static T GetSettings<T>(string propertie)
        {
            T temp = default(T);
            if (_settings.Contains(propertie))
            {
                temp = (T)_settings[propertie];
            }
            else
            {
                _settings.Add(propertie, default(T));
                _settings.Save();
            }
            return temp;
        }

        public static T GetSettings<T>(string propertie, T defaultvalue)
        {
            T temp = defaultvalue;
            if (_settings.Contains(propertie))
            {
                temp = (T)_settings[propertie];
            }
            else
            {
                _settings.Add(propertie, defaultvalue);
                _settings.Save();
            }
            return temp;
        }

        public static void SaveSettings<T>(string propertie, T value)
        {
            try
            {
                _settings[propertie] = value;
                _settings.Save();
            }
            catch (Exception e)
            {
                
            }

        }
    }
}
