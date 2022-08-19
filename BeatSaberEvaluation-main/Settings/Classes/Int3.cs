using UnityEngine;

namespace SaberTailor.Settings.Classes
{
    public class Int3
    {
        public int x;
        public int y;
        public int z;

        public static Int3 zero => new Int3(0, 0, 0);

        public Int3() { }

        public Int3(Int3 int3) : this(int3.x, int3.y, int3.z) { }

        public Int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Converts the PluginConfig.StoreableIntVector3 to a UnityEngine.Vector3 format
        /// </summary>
        public static Vector3 ToVector3(Int3 int3) => new Vector3()
        {
            x = int3.x,
            y = int3.y,
            z = int3.z
        };

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(Int3 left, Int3 right)
        {
            return left.x == right.x
                && left.y == right.y
                && left.z == right.z;
        }

        public static bool operator !=(Int3 left, Int3 right)
        {
            return left.x != right.x
                || left.y != right.y
                || left.z != right.z;
        }
    }
}
