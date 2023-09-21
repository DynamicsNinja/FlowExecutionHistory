using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Fic.XTB.FlowExecutionHistory.Helpers
{
    public static class ColorHelper
    {
        public static List<Color> GetAllColors()
        {
            var colorProperties = typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public);
            var colors = new List<Color>();

            foreach (var property in colorProperties)
            {
                if (property.PropertyType != typeof(Color)) { continue; }

                var color = (Color)property.GetValue(null, null);
                colors.Add(color);
            }

            return colors.ToList();
        }
    }
}
