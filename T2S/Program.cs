using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Threading;
using System.Diagnostics;

using System.Windows.Forms;

using Timer = System.Threading.Timer;

namespace SC2
{
    class Program
    {
        class BuildOrderItem
        {
            public string timing;
            public string text;
        };

        struct BuildOrder
        {
            public string description;
            public BuildOrderItem[] items;
        }

        static void Main(string[] args)
        {

            BuildOrder TvP = new BuildOrder
            {
                description = "https://www.youtube.com/watch?v=KK4Trit-MvE",

                items = new BuildOrderItem[]
                {
                    new BuildOrderItem { timing = "00:00:00", text = "SCV" },
                    new BuildOrderItem { timing = "00:00:18", text = "Supply Depot" },
                    new BuildOrderItem { timing = "00:00:29", text = "Refinery" },
                    new BuildOrderItem { timing = "00:00:45", text = "Barracks" },
                    new BuildOrderItem { timing = "00:01:32", text = "Marine" },
                    new BuildOrderItem { timing = "00:01:33", text = "Orbital Command" },
                    new BuildOrderItem { timing = "00:01:44", text = "Command Center" },
                    new BuildOrderItem { timing = "00:01:51", text = "Reactor" },
                    new BuildOrderItem { timing = "00:01:57", text = "Mule" },
                    new BuildOrderItem { timing = "00:02:00", text = "Factory" },
                    new BuildOrderItem { timing = "00:02:10", text = "Supply Depot" },
                    new BuildOrderItem { timing = "00:02:24", text = "Refinery" },
                    new BuildOrderItem { timing = "00:02:43", text = "Starport" },
                    new BuildOrderItem { timing = "00:02:45", text = "Widow Mine" },
                    new BuildOrderItem { timing = "00:02:58", text = "Orbital Command" },
                    new BuildOrderItem { timing = "00:03:17", text = "Widow Mine" },
                    new BuildOrderItem { timing = "00:03:23", text = "Viking" },
                    new BuildOrderItem { timing = "00:03:29", text = "Barracks" },
                    new BuildOrderItem { timing = "00:03:38", text = "Barracks" },
                    new BuildOrderItem { timing = "00:03:44", text = "Supply Depot" },
                    new BuildOrderItem { timing = "00:03:45", text = "Tech Lab" },
                    new BuildOrderItem { timing = "00:03:46", text = "Tech Lab" },
                    new BuildOrderItem { timing = "00:03:49", text = "Engineering Bay" },
                    new BuildOrderItem { timing = "00:04:08", text = "Switch" },
                    new BuildOrderItem { timing = "00:04:14", text = "+1 Attack" },
                    new BuildOrderItem { timing = "00:04:16", text = "Reactor" },
                    new BuildOrderItem { timing = "00:04:17", text = "Reactor" },
                    new BuildOrderItem { timing = "00:04:24", text = "Stimpack" },
                    new BuildOrderItem { timing = "00:04:31", text = "Combat Shield" },
                    new BuildOrderItem { timing = "00:04:56", text = "Medivac" },
                    new BuildOrderItem { timing = "00:04:57", text = "Medivac" },
                    new BuildOrderItem { timing = "00:05:01", text = "Widow Mine" },
                    new BuildOrderItem { timing = "00:05:02", text = "Widow Mine" },
                    new BuildOrderItem { timing = "00:05:45", text = "Supply Depot" },
                    new BuildOrderItem { timing = "00:05:46", text = "Move out" },
                    new BuildOrderItem { timing = "00:05:53", text = "Medivac" },
                    new BuildOrderItem { timing = "00:05:54", text = "Medivac" },
                    new BuildOrderItem { timing = "00:05:55", text = "Command Center" },
                    new BuildOrderItem { timing = "00:06:08", text = "Refinery" },
                    new BuildOrderItem { timing = "00:06:09", text = "Refinery" },
                    new BuildOrderItem { timing = "00:06:10", text = "Barracks" },
                    new BuildOrderItem { timing = "00:06:12", text = "Barracks" },
                    //new BuildOrderItem { timing = "00:0:", text = "Concussive Shells" },
                    new BuildOrderItem { timing = "00:06:22", text = "+1 Armor" },
                    new BuildOrderItem { timing = "00:06:53", text = "Supply Depot" },
                    new BuildOrderItem { timing = "00:06:56", text = "Supply Depot" },
                    new BuildOrderItem { timing = "00:07:00", text = "Reactor" },
                    new BuildOrderItem { timing = "00:07:01", text = "Reactor" },
                }
            };

            using (var synthesizer = new SpeechSynthesizer())
            {
                synthesizer.SelectVoice("Microsoft Zira Desktop");
                synthesizer.Volume = 100;  // 0...100
                synthesizer.Rate = 0;

                Timer timer = null;
                uint currentBuildItem = 0;
                Stopwatch debugWatch = null;

                var TimerCallbackProc = new TimerCallback(delegate (object state)
                {
                    Console.WriteLine("[{0:D2}:{1:D2}] {2}", 
                        debugWatch.Elapsed.Minutes, 
                        debugWatch.Elapsed.Seconds, 
                        TvP.items[currentBuildItem].text);

                    synthesizer.SpeakAsync(TvP.items[currentBuildItem].text);

                    ++currentBuildItem;
                    if (currentBuildItem < TvP.items.Length)
                    {
                        timer.Change(
                            TimeSpan.Parse(TvP.items[currentBuildItem].timing) - TimeSpan.Parse(TvP.items[currentBuildItem - 1].timing), 
                            Timeout.InfiniteTimeSpan);
                    }
                });


                using (var keyHook = new GlobalKeyboardHook())
                {
                    bool running = false;

                    var KeyPressCallbackProc = new EventHandler<GlobalKeyboardHookEventArgs>(delegate (object sender, GlobalKeyboardHookEventArgs e)
                    {
                        if (!running && e.KeyboardData.VirtualCode == 'S')
                        {
                            Console.WriteLine("(Re)starting...");
                            currentBuildItem = 0;
                            timer = new Timer(TimerCallbackProc, null, TimeSpan.Parse(TvP.items[currentBuildItem].timing), Timeout.InfiniteTimeSpan);
                            debugWatch = Stopwatch.StartNew();
                            running = true;
                        }
                        else if (running && e.KeyboardData.VirtualCode == 0x79) // VK_F10
                        {
                            Console.WriteLine("Stopped.");
                            timer.Dispose();
                            debugWatch.Stop();
                            running = false;
                        }
                        else
                        {
                            return;
                        }
                    });

                   keyHook.KeyboardPressed += KeyPressCallbackProc;
                   Application.Run();
                }
            }
        }
    }
}
