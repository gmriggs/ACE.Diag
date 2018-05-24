using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System;
using ACE.Diag.Entity;
using ACE.Diag.Network;

namespace ACE.Diag
{
    public class Program
    {
        public static Dictionary<ObjectGuid, WorldObject> WorldObjects;

        public static void Main(string[] args)
        {
            // create a dummy gamestate
            CreateState();

            var server = new Server();

            // run until exit
            do
            {
                Thread.Sleep(5000);
            }
            while (true);
        }

        public static void CreateState()
        {
            WorldObjects = new Dictionary<ObjectGuid, WorldObject>();

            var landblock = new LandblockId(0x7D64FFFF);

            var player = new Player();
            player.Guid = new ObjectGuid(1);
            //player.Name = "gmriggs";
            //player.Radius = 5.0f;
            player.Location.landblockId = landblock;
            player.Location.Pos = new Vector3(66, 130, 12);
            player.Location.Rotation = Quaternion.Identity;

            WorldObjects.Add(player.Guid, player);
        }
    }
}
