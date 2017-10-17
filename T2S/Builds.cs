using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SC2BuildTrainer
{
    class Builds
    {
        public static BuildOrder TvP => ParseBuildOrder(_Terran);
        public static BuildOrder TvZ => ParseBuildOrder(_Terran);
        public static BuildOrder TvT => ParseBuildOrder(_TvT);

        public static BuildOrder ZvT => ParseBuildOrder(_ZvX);
        public static BuildOrder ZvP => ParseBuildOrder(_ZvX);
        public static BuildOrder ZvZ => ParseBuildOrder(_ZvZ);

        public static BuildOrder PvT => ParseBuildOrder(_PvX);
        public static BuildOrder PvZ => ParseBuildOrder(_PvX);
        public static BuildOrder PvP => ParseBuildOrder(_PvP);

        public static BuildOrder Empty = new BuildOrder
        {
            description = "",

            items = new BuildOrderItem[]
            {
            }
        };

        private static string _TvT = @"
#http://lotv.spawningtool.com/build/55330/
          0:00    SCV
  14	  0:18	  Supply Depot	  
  15	  0:29	  Refinery	  1st
  16	  0:44	  Barracks	  
  19	  1:31	  Orbital Command	  
  19	  1:32	  Reaper	  
  20	  1:45	  Command Center	  
  20	  1:58	  Factory	  
  21	  2:06	  Barracks Reactor	  
  21	  2:13	  Supply Depot	  
  22	  2:17	  Refinery	  2nd
  24	  2:44	  Cyclone	  
  24	  2:45	  Marine x2	  Keep doing it
  30	  2:57	  Orbital Command	  
  30	  3:03	  Starport	  
  32	  3:17	  Factory Tech Lab	  
  36	  3:36	  Siege Tank	  1st
  41	  3:47	  Viking	  
  44	  3:49	  Supply Depot	  
  47	  4:17	  Action	  Swap starport into tech lab (Factory)
  48	  4:22	  Factory Tech Lab	  
  50	  4:24	  Raven	  1st
  50	  4:27	  Command Center	  
  50	  4:28	  Supply Depot	  
  53	  4:38	  Refinery x2	  3rd, 4th
  54	  4:41	  Supply Depot	  
  54	  4:49	  Siege Tank	  2nd
  65	  5:18	  Raven	  
  67	  5:25	  Barracks x2	  
  69	  5:30	  Siege Tank	  3rd
  74	  5:41	  Orbital Command	  
  61	  6:02	  Siege Tank	  4th
  69	  6:24	  Barracks Tech Lab	  
  67	  6:28	  Starport Reactor	  
  67	  6:30	  Stimpack	  
  67	  6:31	  Orbital Command	  
  70	  6:34	  Siege Tank	  
  70	  6:35	  Engineering Bay x2	  
  75	  6:45	  Combat Shields	  
  80	  6:57	  Barracks x2	  
  83	  7:03	  Terran Infantry Armor Level 1, Terran Infantry Weapons Level 1	  
  91	  7:18	  Refinery	  5th
";

        private static string _Terran = @"
            #http://lotv.spawningtool.com/build/57734/
                  0:00    SCV
            14	  0:17	  Supply Depot	  
            15	  0:29	  Refinery	  
            16	  0:45	  Barracks	  
            19	  1:32	  Barracks Reactor	  
            19	  1:33	  Orbital Command	  
            19	  1:42	  Command Center	  
            19	  1:52	  Factory	  
            20	  2:05	  Supply Depot	  
            23	  2:17	  Refinery	  
            26	  2:39	  Starport	  
            27	  2:40	  Factory Tech Lab	  
            30	  2:58	  Orbital Command	  
            38	  3:27	  Supply Depot	  
            41	  3:32	  Liberator	  
            46	  3:49	  Supply Depot	  
            55	  4:12	  Supply Depot	  
            57	  4:24	  Barracks x2	  
            57	  4:26	  Starport Reactor	  
            58	  4:30	  Refinery x2	  
            58	  4:32	  Engineering Bay	  
            58	  4:35	  Supply Depot	  
            67	  4:59	  +1 Attack
            67	  5:00	  Supply Depot	  
            71	  5:15	  Barracks x2	  
            71	  5:18	  Barracks Tech Lab x2	  
            73	  5:27	  Supply Depot	  
            76	  5:37	  Stimpack, Combat Shields	  
            80	  5:50	  Supply Depot x2	  
            87	  6:06	  Barracks Reactor x2	  
            95	  6:17	  Supply Depot x2	  
            102	  6:40	  Supply Depot x2	  
            117	  7:05	  Supply Depot x2	  
            134	  7:30	  Supply Depot x2
                  7:35    Move Out
        ";

        private static string _PvP = @"
#http://lotv.spawningtool.com/build/55536/
          0:00	  Probe
  14	  0:17	  Pylon	  
  15	  0:38	  Gateway	  
  16	  0:44	  Assimilator	  
  17	  0:53	  Assimilator	  
  19	  1:11	  Gateway	  
  20	  1:25	  Cybernetics Core	  
  21	  1:39	  Pylon	  
  23	  2:02	  Stalker x2, Warp Gate (Chrono Boost)	  
  27	  2:11	  Mothership Core	  
  29	  2:16	  Pylon	  
  29	  2:32	  Stalker x2	  
  36	  3:16	  Nexus	  
  37	  3:23	  Pylon	  
  37	  3:32	  Robotics Facility	  
  39	  3:50	  Sentry x2	  
  45	  4:20	  Observer (Chrono Boost)	  
  47	  4:27	  Twilight Council	  
  49	  4:40	  Immortal (Chrono Boost)	  
  49	  4:41	  Zealot x2	  
  59	  4:49	  Pylon	  
  61	  5:03	  Charge (Chrono Boost)	  
  62	  5:12	  Gateway x2	  
  63	  5:21	  Assimilator x2	  
  64	  5:25	  Immortal	  
  72	  5:52	  Zealot x2	  
  77	  5:59	  Immortal	  
  83	  6:23	  Nexus	  
  85	  6:33	  Immortal	  
  91	  7:01	  Gateway x3	  
  91	  7:06	  Gateway	  
  91	  7:07	  Immortal	  
  99	  7:30	  Forge x2	  
  101	  7:38	  Assimilator x2	  
  101	  7:42	  Templar Archives	  
  104	  7:53	  Protoss Ground Weapons Level 1 (Chrono Boost)	  
  104	  7:54	  Protoss Ground Armor Level 1 (Chrono Boost)	  
  106	  8:10	  Immortal (Chrono Boost)	  
  110	  8:22	  High Templar x8	  
  110	  8:26	  Archon x4	  
  126	  8:44	  Warp Prism (Chrono Boost)	  
  126	  8:48	  Photon Cannon x2	  
  128	  8:55	  Photon Cannon x2	  
  128	  9:00	  Photon Cannon x2	  
  128	  9:09	  Zealot x6	  
  118	  9:25	  Immortal (Chrono Boost)	  
  122	  9:36	  Stargate x2	  
  122	  9:46	  Nexus	  
  122	  9:55	  Assimilator
        ";

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
        private static string _ZvZ = @"
#http://lotv.spawningtool.com/build/52775/
          0:00	  Drone
  13	  0:13	  Overlord	  
  17	  0:46	  Spawning Pool	  
  18	  1:12	  Hatchery	  
  18	  1:15	  Extractor	  
  18	  1:33	  Queen	  
  18	  1:34	  Zergling x2	  
  21	  1:41	  Overlord	  
  23	  2:08	  Queen	  
  25	  2:20	  Roach Warren	  
  24	  2:36	  Queen	  
  34	  2:54	  Overlord x2	  
  34	  3:13	  Roach x3	  
  34	  3:19	  Roach x4	  
  46	  3:25	  Roach x2	  
  52	  3:35	  Overlord	  
  58	  3:59	  Metabolic Boost	  
  58	  4:30	  Extractor	  
  57	  4:33	  Roach	  
  67	  5:01	  Hatchery	  
  76	  5:23	  Zergling x12	  
  82	  5:30	  Zergling x6	  
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