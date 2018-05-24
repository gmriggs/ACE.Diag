using System;
using ProtoBuf;

namespace ACE.Diag.Entity
{
    public enum GuidType
    {
        Undef,
        Player,
        Static,
        Dynamic,
    }

    [ProtoContract]
    public class ObjectGuid: IComparable<ObjectGuid>
    {
        public static uint PlayerMin { get; } = 0x50000001;
        public static uint PlayerMax { get; } = 0x5FFFFFFF;

        public static uint StaticObjectMin { get; } = 0x70000000;
        public static uint StaticObjectMax { get; } = 0x7FFFFFFF;

        public static uint DynamicMin { get; } = 0x80000000;
        public static uint DynamicMax { get; } = 0xFFFFFFFE;

        [ProtoMember(1)]
        public uint Full { get; }
        public uint Low => Full & 0xFFFFFF;
        public uint High => (Full >> 24);

        [ProtoMember(2)]
        public GuidType Type { get; }

        public ObjectGuid() { }

        public ObjectGuid(uint full)
        {
            Full = full;

            if (Full >= PlayerMin && Full <= PlayerMax)
                Type = GuidType.Player;
            else if (Full >= StaticObjectMin && Full <= StaticObjectMax)
                Type = GuidType.Static;
            else if (Full >= DynamicMin && Full <= DynamicMax)
                Type = GuidType.Dynamic;
            else
                Type = GuidType.Undef;
        }

        public bool IsPlayer()
        {
            return Type == GuidType.Player;
        }

        public static bool operator ==(ObjectGuid g1, ObjectGuid g2)
        {
            return g1.Full == g2.Full;
        }

        public static bool operator !=(ObjectGuid g1, ObjectGuid g2)
        {
            return g1.Full != g2.Full;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectGuid && (ObjectGuid)obj == this;
        }

        public int CompareTo(ObjectGuid guid)
        {
            return Full.CompareTo(guid.Full);
        }

        public override int GetHashCode()
        {
            return Full.GetHashCode();
        }
    }
}
