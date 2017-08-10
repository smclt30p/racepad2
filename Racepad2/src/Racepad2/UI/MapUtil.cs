using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Racepad2.UI {
    class MapUtil {

        public static string MAP_SERVICE_TOKEN =
            "4qiurkdgKlTD5Ba1qkOu~7va9tRX0jj2kZEnXqyt8Iw~AmfqIUMnMk6zor_" +
            "i4yKPKpXJeT3J0FxOsy8Z6BcGKRJ6yZwKRfZsj-ak87UiVnzT";

        public static Color GetColorFromSlope(double percentage) {

            if (percentage < -20) return GetSolidColorBrush("0000FF");
            if (percentage > 20) return GetSolidColorBrush("FF0000");

            if (percentage >= -20 && percentage <= -15) return GetSolidColorBrush("00fffF");
            if (percentage >= -15 && percentage <= -10) return GetSolidColorBrush("0060ff");
            if (percentage >= -10 && percentage <= -5) return GetSolidColorBrush("00ff12");
            if (percentage >= -5 && percentage <= 0) return GetSolidColorBrush("00ff12");
            if (percentage >= 0 && percentage <= 5) return GetSolidColorBrush("fffc00");
            if (percentage >= 5 && percentage <= 10) return GetSolidColorBrush("fffc00");
            if (percentage >= 10 && percentage <= 15) return GetSolidColorBrush("ff9600");
            if (percentage >= 15 && percentage <= 20) return GetSolidColorBrush("ff00f0");

            return Colors.Purple;
        }

        /* Thanks to Joel Joseph for this method! */
        public static Color GetSolidColorBrush(string hex) {
            hex = hex.Replace("#", string.Empty);
            byte a = 255;
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush.Color;
        }


    }
}
