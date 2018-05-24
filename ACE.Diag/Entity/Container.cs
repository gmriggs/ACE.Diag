using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace ACE.Diag.Entity
{
    [ProtoContract]
    [ProtoInclude(114, typeof(Creature))]
    public class Container: WorldObject
    {
    }
}
