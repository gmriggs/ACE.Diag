using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using ProtoBuf;

namespace ACE.Diag.Entity
{
    [ProtoContract]
    public class Position
    {
        [ProtoMember(1)]
        public LandblockId landblockId;

        [ProtoMember(2)]
        public Vector3 Pos;

        [ProtoMember(3)]
        public Quaternion Rotation;
    }
}
