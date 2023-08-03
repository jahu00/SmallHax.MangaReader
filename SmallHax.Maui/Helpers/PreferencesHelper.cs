using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallHax.Maui.Helpers
{
    public static class PreferencesHelper
    {
        public const string TrueValue = "true";
        public const string FalseValue = "false";
        public static T GetEnum<T>(string key, T defaultValue) where T : struct
        {
            var stringValue = Preferences.Get(key, defaultValue.ToString());
            var value = Enum.Parse<T>(stringValue);
            return value;
        }

        /// <summary>
        /// On Android Preferences.Get cannot be used directly for bool.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetBool(string key, bool defaultValue)
        {
            var stringValue = Preferences.Get(key, BoolToString(defaultValue));
            return StringToBool(stringValue);
        }

        public static string BoolToString(bool value)
        {
            return value ? TrueValue : FalseValue;
        }

        public static bool StringToBool(string stringValue)
        {
            if (stringValue != null && (stringValue.Equals(TrueValue, StringComparison.InvariantCultureIgnoreCase) || stringValue == "1"))
            {
                return true;
            }
            return false;
        }
    }
}
