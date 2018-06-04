using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace ACE.Diag.Entity
{
    [ProtoContract]
    [ProtoInclude(100, typeof(Admin))]
    [ProtoInclude(101, typeof(AdvocateFane))]
    [ProtoInclude(102, typeof(AdvocateItem))]
    [ProtoInclude(103, typeof(Ammunition))]
    [ProtoInclude(105, typeof(Bindstone))]
    [ProtoInclude(106, typeof(Book))]
    [ProtoInclude(107, typeof(Caster))]
    [ProtoInclude(108, typeof(Chest))]
    [ProtoInclude(109, typeof(Clothing))]
    [ProtoInclude(110, typeof(Coin))]
    [ProtoInclude(111, typeof(Container))]
    [ProtoInclude(112, typeof(Corpse))]
    [ProtoInclude(113, typeof(Cow))]
    [ProtoInclude(115, typeof(Door))]
    [ProtoInclude(116, typeof(Food))]
    [ProtoInclude(117, typeof(Game))]
    [ProtoInclude(118, typeof(GamePiece))]
    [ProtoInclude(119, typeof(Gem))]
    [ProtoInclude(120, typeof(GenericObject))]
    [ProtoInclude(121, typeof(Healer))]
    [ProtoInclude(122, typeof(Key))]
    [ProtoInclude(123, typeof(Lifestone))]
    [ProtoInclude(124, typeof(Lock))]
    [ProtoInclude(125, typeof(Lockpick))]
    [ProtoInclude(126, typeof(MeleeWeapon))]
    [ProtoInclude(127, typeof(Missile))]
    [ProtoInclude(128, typeof(MissileLauncher))]
    [ProtoInclude(130, typeof(PKModifier))]
    [ProtoInclude(132, typeof(Portal))]
    [ProtoInclude(133, typeof(Scroll))]
    [ProtoInclude(134, typeof(Sentinel))]
    [ProtoInclude(137, typeof(SpellComponent))]
    [ProtoInclude(138, typeof(SpellProjectile))]
    [ProtoInclude(139, typeof(Stackable))]
    [ProtoInclude(140, typeof(Switch))]
    public class WorldObject
    {
        [ProtoMember(1)]
        public ObjectGuid Guid;

        [ProtoMember(2)]
        public Position Location;

        [ProtoMember(3)]
        public bool? Missile;

        [ProtoMember(4)]
        public float Radius;

        public WorldObject()
        {
            Guid = new ObjectGuid();
            Location = new Position();
        }
    }
}
