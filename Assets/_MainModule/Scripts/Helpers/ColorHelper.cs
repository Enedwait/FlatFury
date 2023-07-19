using UnityEngine;

namespace FlatFury.MainModule.Scripts.Helpers
{
    /// <summary>
    /// The <see cref="ColorHelper"/> class.
    /// This class contains different methods for the ease of work with colors
    /// </summary>
    internal static class ColorHelper
    {
        /// <summary>
        /// Adds hue to the specified color.
        /// </summary>
        /// <param name="color">color</param>
        /// <param name="hue">hue</param>
        /// <returns>new color</returns>
        public static Color AddHue(this Color color, float hue)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            return Color.HSVToRGB(h + hue, s, v);
        }

        /// <summary>
        /// Pulses hue between two values.
        /// </summary>
        /// <param name="color">color</param>
        /// <param name="hue">hue</param>
        /// <param name="minHue">min hue</param>
        /// <param name="maxHue">max hue</param>
        /// <param name="add">add if <value>True</value>; subtract otherwise</param>
        /// <returns>new color</returns>
        public static Color PulseHue(this Color color, float hue, float minHue, float maxHue, ref bool add)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);

            float H = add ? (h + hue) : (h - hue);
            if (H > maxHue)
            {
                H = maxHue - (H - maxHue);
                add = !add;
            }
            else if (H < minHue)
            {
                H = minHue + (minHue - H);
                add = !add;
            }

            return Color.HSVToRGB(H, s, v);
        }
    }
}
