using System.Numerics;
using ProtoBuf;

namespace ACE.Diag.Network.Serialization
{ 
    [ProtoContract]
    public class QuaternionSurrogate
    {
        [ProtoMember(1)]
        public float x;
        [ProtoMember(2)]
        public float y;
        [ProtoMember(3)]
        public float z;
        [ProtoMember(4)]
        public float w;

        public static implicit operator QuaternionSurrogate(Quaternion q)
        {
            return new QuaternionSurrogate { x = q.X, y = q.Y, z = q.Z, w = q.W };
        }

        public static implicit operator Quaternion(QuaternionSurrogate q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
    }
}
