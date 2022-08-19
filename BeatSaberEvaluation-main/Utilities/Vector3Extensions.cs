using UnityEngine;

namespace SaberTailor.Utilities
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Returns a copy of the Vector3
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector3 Clone(this Vector3 vector3)
        {
            Vector3 result = new Vector3(vector3.x, vector3.y, vector3.z);
            return result;
        }

        /// <summary>
        /// Rescale and return a new Vector3
        /// </summary>
        /// <param name="vector3">Base Vector3</param>
        /// <param name="x">X multiplier</param>
        /// <param name="y">Y multiplier</param>
        /// <param name="z">Z multiplier</param>
        /// <returns>New, rescaled Vector3</returns>
        public static Vector3 Rescale(this Vector3 vector3, float x, float y, float z)
        {
            Vector3 result = new Vector3()
            {
                x = vector3.x * x,
                y = vector3.y * y,
                z = vector3.z * z
            };

            return result;
        }
    }
}
