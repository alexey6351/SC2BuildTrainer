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
using System.Drawing;
using Tesseract;

namespace SC2BuildTrainer
{
    struct BuildOrderItem
    {
        public string timing;
        public string text;
    };

    struct BuildOrder
    {
        public string description;
        public BuildOrderItem[] items;
    }

    class BuildTrainer
    {
        static void Main(string[] args)
        {
            ExecuteBuildOrder(Builds.Empty);
        }

        static void ExecuteBuildOrder(BuildOrder buildOrder)
        { 
            var ocrEngine = new TesseractEngine(@"C:\Program Files (x86)\Tesseract-OCR\tessdata", "eng");
            ocrEngine.SetVariable("tessedit_char_whitelist", "0123456789");

            using (var synthesizer = new SpeechSynthesizer())
            {
                synthesizer.SelectVoice("Microsoft Zira Desktop");
                synthesizer.Volume = 100;  // 0...100
                synthesizer.Rate = 0;

                Timer timer = null;
                uint currentBuildItem = 0;
                Stopwatch debugWatch = null;

                var SupplyTimerCallbackProc = new TimerCallback(delegate (object state)
                {
                    // 1920 x 1200
                    using (Bitmap sc2SupplyScreenCapture = new Bitmap(1830 - 1750, 45 - 20))
                    {
                        using (Graphics g = Graphics.FromImage(sc2SupplyScreenCapture))
                            g.CopyFromScreen(
                                1750, 20,
                                0, 0,
                                sc2SupplyScreenCapture.Size,
                                CopyPixelOperation.SourceCopy);

                        using (var ocrResult = ocrEngine.Process(sc2SupplyScreenCapture, Rect.Empty, PageSegMode.SingleLine))
                        {
                            string text = ocrResult.GetText().Replace("\n", "");

                            synthesizer.SpeakAsync(text);

                            Console.WriteLine("[{0:D2}:{1:D2}] {2}",
                                debugWatch.Elapsed.Minutes,
                                debugWatch.Elapsed.Seconds,
                                text);
                        }
                    }
                });

                var BuildOrderTimerCallbackProc = new TimerCallback(delegate (object state)
                {
                    if (currentBuildItem < buildOrder.items.Length)
                    {
                        synthesizer.SpeakAsync(buildOrder.items[currentBuildItem].text);

                        Console.WriteLine("[{0:D2}:{1:D2}] {2}",
                            debugWatch.Elapsed.Minutes,
                            debugWatch.Elapsed.Seconds,
                            buildOrder.items[currentBuildItem].text);

                        ++currentBuildItem;
                    }

                    if (currentBuildItem < buildOrder.items.Length)
                    {
                        var advance = TimeSpan.FromSeconds(currentBuildItem == 0 ? 1 : 0);

                        timer.Change(
                            TimeSpan.Parse(buildOrder.items[currentBuildItem].timing) 
                                - TimeSpan.Parse(buildOrder.items[currentBuildItem - 1].timing) 
                                - advance, 
                            Timeout.InfiniteTimeSpan);
                    }
                    else
                    {
                        timer.Dispose();
                        timer = new Timer(SupplyTimerCallbackProc, null, 0, 1000);
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
                            timer = new Timer(BuildOrderTimerCallbackProc, null, 0, -1);
                            debugWatch = Stopwatch.StartNew();
                            running = true;
                        }
                        else if (running && e.KeyboardData.VirtualCode == 0x79) // VK_F10
                        {
                            timer.Dispose();
                            debugWatch.Stop();
                            running = false;
                            Console.WriteLine("Stopped.");
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
