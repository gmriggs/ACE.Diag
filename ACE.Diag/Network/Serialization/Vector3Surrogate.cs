using System.Numerics;
using ProtoBuf;

namespace ACE.Diag.Network.Serialization
{
    [ProtoContract]
    public class Vector3Surrogate
    {
        [ProtoMember(1)]
        public float x;
        [ProtoMember(2)]
        public float y;
        [ProtoMember(3)]
        public float z;

        public static implicit operator Vector3Surrogate(Vector3 v)
        {
            return new Vector3Surrogate { x = v.X, y = v.Y, z = v.Z };
        }

        public static implicit operator Vector3(Vector3Surrogate v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
    }
}
