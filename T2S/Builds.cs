using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SC2BuildTrainer
{
    class Builds
    {
        public static BuildOrder Empty = new BuildOrder
        {
            description = "",

            items = new BuildOrderItem[]
            {
            }
        };

        public static string _Terran = @"
            #https://www.youtube.com/watch?v=KK4Trit-MvE
            00:00 SCV
            00:18 Supply Depot
            00:29 Refinery
            00:45 Barracks
            01:32 Marine
            01:33 Orbital Command
            01:44 Command Center
            01:51 Reactor
            01:57 Mule
            02:00 Factory
            02:10 Supply Depot
            02:24 Refinery
            02:43 Starport
            02:45 Widow Mine
            02:58 Orbital Command
            03:17 Widow Mine
            03:23 Viking
            03:29 Barracks
            03:38 Barracks
            03:44 Supply Depot
            03:45 Tech Lab
            03:46 Tech Lab
            03:49 Engineering Bay
            04:08 Switch
            04:14 +1 Attack
            04:16 Reactor
            04:17 Reactor
            04:24 Stimpack
            04:30 Supply Depot
            04:34 Combat Shield
            04:56 Medivac
            04:57 Medivac
            05:01 Widow Mine
            05:02 Widow Mine
            05:04 Supply Depot
            05:17 Supply Depot
            05:45 Supply Depot
            05:46 Move out
            05:53 Medivac
            05:54 Medivac
            05:55 Command Center
            06:08 Refinery
            06:09 Refinery
            06:10 Barracks
            06:12 Barracks
            06:22 +1 Armor
            06:53 Supply Depot
            06:56 Supply Depot
            07:00 Reactor
            07:01 Reactor
        ";

        public static BuildOrder Terran => ParseBuildOrder(_Terran);
        public static BuildOrder Zerg => ParseBuildOrder(_ZvX);
        public static BuildOrder Protoss => ParseBuildOrder(_PvX);

        private static string _PvX = @"
            0:00	  Probe
            0:18	  Pylon	  
            0:37	  Gateway	  
            0:41	  Assimilator	  
            1:21	  Nexus	  
            1:31	  Cybernetics Core	  
            1:40	  Assimilator	  
            1:52	  Pylon	  
            2:07	  Adept	  
            2:17	  Stargate	  
            2:23	  Warp Gate	  
            2:49	  Adept	  
            2:56	  Oracle	  
            3:04	  Assimilator	  
            3:07	  Assimilator	  
            3:20	  Adept	  
            3:37	  Pylon	  
            3:56	  Mothership Core (Chrono Boost)	  
            4:04	  Pylon	  
            4:07	  Forge	  
            4:15	  Fleet Beacon	  
            4:22	  Stargate	  
            4:30	  Adept	  
            4:41	  Pylon	  
            4:48	  Photon Cannon	  
            4:58	  Carrier	  
            5:01	  Photon Cannon	  
            5:02	  Protoss Air Weapons Level 1	  
            5:14	  Pylon	  
            5:22	  Carrier	  
            5:39	  Graviton Catapult	  
            5:43	  Pylon	  
            6:12	  Carrier	  
            6:13	  Twilight Council	  
            6:22	  Nexus	  
            6:28	  Pylon	  
            6:36	  Carrier	  
            6:49	  Adept	  
            6:53	  Resonating Glaives	  
            7:00	  Gateway	  
            7:06	  Gateway	  
            7:14	  Gateway	  
            7:23	  Pylon	  
            7:24	  Pylon	  
            7:26	  Adept	  
            7:33	  Pylon	  
            7:38	  Carrier	  
            8:03	  Assimilator	  
            8:05	  Pylon	  
            8:06	  Assimilator	  
            8:07	  Carrier	  
            8:25	  Templar Archives	  
            8:28	  Pylon	  
            8:48	  Gateway	  
            8:50	  Gateway	  
            8:51	  Gateway	  
            8:56	  Adept	  
            9:01	  Oracle	  
            9:05	  Adept x2	  
            9:06	  Adept	  
            9:08	  Psionic Storm	  
            9:18	  High Templar	  
            9:23	  Gateway	  
            9:27	  Gateway	  
            9:30	  High Templar x3	  
            9:54	  High Templar x2	  
            9:55	  High Templar        
        ";

        private static string _ZvX = @"
            #http://lotv.spawningtool.com/build/56494/
            0:00	  Drone	  
            0:14	  Overlord	  
            0:53	  Hatchery	  
            1:09	  Spawning Pool	  
            1:43	  Overlord	  
            1:58	  Queen	  
            2:01	  Zergling x4	  
            2:04	  Queen	  
            2:27	  Overlord	  
            2:34	  Queen	  
            2:40	  Queen	  
            2:47	  Spine Crawler	  
            3:18	  Overlord	  
            3:21	  Extractor x4	  
            3:35	  Overlord	  
            3:58	  Lair	  
            4:06	  Evolution Chamber x2	  
            4:09	  Roach Warren	  
            4:33	  Zerg Missile Weapons Level 1	  
            4:35	  Zerg Ground Armor Level 1	  
            4:45	  Hatchery	  
            4:51	  Roach x7	  
            4:57	  Roach	  
            5:00	  Glial Reconstitution	  
            5:03	  Overseer	  
            5:09	  Roach x2	  
            6:00	  Spore Crawler x2	  
            6:02	  Overseer	  
            6:23	  Ravager x4	  
            6:29	  Spore Crawler	  
            7:03	  Extractor x2
        ";



        private static BuildOrder ParseBuildOrder(string buildOrderText)
        {
            var buildOrder = new BuildOrder();

            using (var buildOrderStringStream = new StringReader(buildOrderText))
            {
                var buildOrderItems = new List<BuildOrderItem> { };

                string line;
                while ((line = buildOrderStringStream.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    line = line.Trim();

                    if (line.StartsWith("#"))
                    {
                        buildOrder.description = line.Remove(0, 1);
                        continue;
                    }

                    var match = Regex.Match(line, @"^(\d+):(\d+)\s+(.*)$");
                    if (!match.Success)
                        throw new ApplicationException();

                    var item = new BuildOrderItem
                    {
                        timing = String.Format("00:{0:D2}:{1:D2}", 
                            Convert.ToInt32(match.Groups[1].Value),
                            Convert.ToInt32(match.Groups[2].Value)),
                        text = match.Groups[3].Value
                    };

                    buildOrderItems.Add(item);
                }

                buildOrder.items = buildOrderItems.ToArray();
            }

            return buildOrder;
        }
    }
}