using UnityEngine;

namespace FlatFury.MainModule.Scripts.Helpers
{
    /// <summary>
    /// The <see cref="RandomHelper"/> class.
    /// This class contains different methods for the ease of work with random values.
    /// </summary>
    internal static class RandomHelper
    {
        /// <summary>
        /// Gets the random point in the specified bounds.
        /// </summary>
        /// <param name="bounds">bounds.</param>
        /// <returns>random point in bounds</returns>
        public static Vector3 GetRandomPointInBounds(this Bounds bounds)
        {
            Vector3 point = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), UnityEngine.Random.Range(bounds.min.y, bounds.max.y), UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
            return bounds.ClosestPoint(point);
        }
    }
}