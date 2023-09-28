using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Fic.XTB.FlowExecutionHistory.Helpers
{
    public static class ColorHelper
    {
        public static List<Color> GetAllColors(int count)
        {
            var uniqueColors = new List<Color>();

            var goldenRatioConjugate = 0.618033988749895;

            for (var i = 0; i < count; i++)
            {
                var hue = (i * goldenRatioConjugate) % 1;
                var color = FromHsl(hue, 0.5, 0.6);
                uniqueColors.Add(color);
            }

            return uniqueColors;
        }

        private static Color FromHsl(double hue, double saturation, double lightness)
        {
            var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
            var x = c * (1 - Math.Abs((hue * 6) % 2 - 1));
            var m = lightness - c / 2;

            double r, g, b;

            if (hue < 1.0 / 6)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (hue < 2.0 / 6)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (hue < 3.0 / 6)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (hue < 4.0 / 6)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (hue < 5.0 / 6)
            {
                r = x;
                g = 0;
                b = c;
            }
            else
            {
                r = c;
                g = 0;
                b = x;
            }

            return Color.FromArgb((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
        }
    }
}
