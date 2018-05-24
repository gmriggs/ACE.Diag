using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace ACE.Diag.Entity
{
    [ProtoContract]
    [ProtoInclude(131, typeof(Player))]
    [ProtoInclude(141, typeof(Vendor))]
    public class Creature: Container
    {
    }
}
