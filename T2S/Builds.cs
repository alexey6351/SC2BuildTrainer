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
            4:04	  Pylon	  
            4:07	  Forge	  
            4:15	  Fleet Beacon	  
            4:22	  Stargate	  
            4:30	  Adept	  
            4:41	  Pylon	  
            4:48	  Photon Cannon	  
            4:58	  Carrier	  
            5:01	  Photon Cannon	  
            5:02	  Air Weapons 1	  
            5:14	  Pylon	  
            5:22	  Carrier	  
            5:39	  Graviton Catapult	  
            5:43	  Pylon	  
            6:12	  Carrier	  
            6:13	  Twilight Council	  
            6:22	  Nexus	  
            6:28	  Pylon	  
            6:36	  Carrier	  
            6:49	  Zealots	  
            6:53	  Robo
            7:00	  Charge
            7:06	  Gateway	  
            7:14	  Gateway	  
            7:23	  Pylon	  
            7:24	  Pylon	  
            7:26	  Zealots	  
            7:33	  Pylon	  
            7:38	  Carrier	  
            8:03	  Assimilator	  
            8:05	  Pylon	  
            8:06	  Assimilator	  
            8:07	  Carrier	  
            8:25	  Stargate	  
            8:28	  Pylon	  
            8:48	  Gateway	  
            8:50	  Gateway	  
            8:51	  Gateway	  
            8:56	  Zealots	  
            9:01	  Observer
        ";

        private static string _ZvX = @"
            #http://lotv.spawningtool.com/build/56494/
                  0:00	  Drone
            13	  0:12	  Overlord	  
            17	  0:45	  Spawning Pool	  
            17	  1:09	  Hatchery	  
            16	  1:15	  Extractor	  
            18	  1:31	  Queen	  
            20	  1:34	  Zergling x4	  
            22	  1:49	  Overlord	  
            22	  2:06	  Queen	  
            26	  2:17	  Metabolic Boost	  
            30	  2:40	  Overlord	  
            44	  3:27	  Overlord
            44	  3:28	  Overlord
            44	  3:29	  Hatchery	  
            43	  3:41	  Roach Warren	  
            52	  3:55	  Queen	  
            54	  4:21	  Roach x8	  
            70	  4:32	  Roach	  
            70	  4:33	  Queen	  
            70	  4:38	  Roach	  
            85	  5:03	  Evolution Chamber x2	  
            85	  5:10	  Extractor x4
            86	  5:23	  Hatchery	  
            89	  5:31	  +1 Ranged Attack
            90	  5:40	  Extractor	  
            90	  5:42	  +1 Armor
            92	  5:44	  Lair	  
            135	  6:43	  Hydralisk Den	  
            135	  6:44	  Glial Reconstitution	  
            150	  7:15	  Muscular Augments, Hydralisk x7	  
            164	  7:23	  Hydralisk x3	  
            180	  7:52	  Extractor	  
            191	  8:06	  Extractor	  
            200	  8:25	  Infestation Pit	  
            199	  9:03	  Spire 
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

                    var match = Regex.Match(line, @"^(?:(?<supply>\d+)\s+)?(?<minutes>\d+):(?<seconds>\d+)\s+(?<text>.*)$");
                    if (!match.Success)
                        throw new ApplicationException();

                    var item = new BuildOrderItem
                    {
                        supply = match.Groups["supply"].Value,
                        timing = String.Format("00:{0:D2}:{1:D2}", 
                            Convert.ToInt32(match.Groups["minutes"].Value),
                            Convert.ToInt32(match.Groups["seconds"].Value)),
                        text = match.Groups["text"].Value
                    };

                    buildOrderItems.Add(item);
                }

                buildOrder.items = buildOrderItems.ToArray();
            }

            return buildOrder;
        }
    }
}