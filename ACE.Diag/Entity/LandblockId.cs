using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace ACE.Diag.Entity
{
    [ProtoContract]
    public class LandblockId: IEquatable<LandblockId>
    {
        [ProtoMember(1)]
        public uint Raw { get; }

        public LandblockId() { }

        public LandblockId(uint raw)
        {
            Raw = raw;
        }

        public LandblockId(byte x, byte y)
        {
            Raw = (uint)x << 24 | (uint)y << 16;
        }

        public LandblockId East => new LandblockId(Convert.ToByte(LandblockX + 1), LandblockY);

        public LandblockId West => new LandblockId(Convert.ToByte(LandblockX - 1), LandblockY);

        public LandblockId North => new LandblockId(LandblockX, Convert.ToByte(LandblockY + 1));

        public LandblockId South => new LandblockId(LandblockX, Convert.ToByte(LandblockY - 1));

        public LandblockId NorthEast => new LandblockId(Convert.ToByte(LandblockX + 1), Convert.ToByte(LandblockY + 1));

        public LandblockId NorthWest => new LandblockId(Convert.ToByte(LandblockX - 1), Convert.ToByte(LandblockY + 1));

        public LandblockId SouthEast => new LandblockId(Convert.ToByte(LandblockX + 1), Convert.ToByte(LandblockY - 1));

        public LandblockId SouthWest => new LandblockId(Convert.ToByte(LandblockX - 1), Convert.ToByte(LandblockY - 1));

        public ushort Landblock => (ushort)((Raw >> 16) & 0xFFFF);

        public byte LandblockX => (byte)((Raw >> 24) & 0xFF);

        public byte LandblockY => (byte)((Raw >> 16) & 0xFF);

        public bool Equals(LandblockId landblock)
        {
            return Raw.Equals(landblock.Raw);
        }
    }
}
