using UnityEngine;

namespace FlatFury.MainModule.Scripts.Helpers
{
    /// <summary>
    /// The <see cref="MathHelper"/> class.
    /// This class contains different methods for the ease of work with Math.
    /// </summary>
    internal static class MathHelper
    {
        /// <summary>
        /// Returns the angle in degrees whose Tan is y/x for 2D vector coordinates. 
        /// </summary>
        /// <param name="a">2D vector</param>
        /// <returns>the angle in degrees</returns>
        public static float Atan2(this Vector2 a) => Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg;

        /// <summary>
        /// Returns the max range between two float values.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>||a|+|b||</returns>
        public static float MaxRange(this float a, float b) => Mathf.Abs(Mathf.Abs(a) + Mathf.Abs(b));
    }
}
