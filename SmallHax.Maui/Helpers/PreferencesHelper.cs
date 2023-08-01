using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallHax.Maui.Helpers
{
    public static class PreferencesHelper
    {
        public static T GetEnum<T>(string key, T defaultValue) where T : struct
        {
            var stringValue = Preferences.Get(key, defaultValue.ToString());
            var value = Enum.Parse<T>(stringValue);
            return value;
        }
    }
}
