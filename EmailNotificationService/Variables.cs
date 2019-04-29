using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailNotificationService
{
    public static class Variables
    {
        #region Members

        public readonly static int NOTIFICATION_PERIOD_SECONDS = GetAppSetting<int>("NOTIFICATION_PERIOD_SECONDS", 30);

        #endregion
        #region Methods
        
        public static T GetAppSetting<T>(string settingName, T defaultValue)
        {
            try
            {
                object value =System.Configuration.ConfigurationSettings.AppSettings[settingName];
                if (value == null)
                    return defaultValue;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch { return defaultValue; }
        }
        #endregion
    }
}
