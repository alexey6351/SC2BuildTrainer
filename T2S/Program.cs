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
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SC2BuildTrainer
{
    struct BuildOrderItem
    {
        public string supply;
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
            ExecuteBuildOrder(/*Rectangle.FromLTRB(1750, 15, 1860, 50)*/);
        }

#if false
        static bool CopyFromScreen(Rectangle screenRegion, Image image)
        {
            try
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.CopyFromScreen(
                        screenRegion.Left, screenRegion.Top,
                        0, 0,
                        image.Size,
                        CopyPixelOperation.SourceCopy);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
#endif

        static void ExecuteBuildOrder(/*Rectangle screenRegion*/)
        { 
            using (var synthesizer = new SpeechSynthesizer())
            {
                synthesizer.SelectVoice("Microsoft Zira Desktop");
                synthesizer.Volume = 100;  // 0...100
                synthesizer.Rate = 0; // -10 .. 10

                Timer timer = null;
                //object timerAccessLock = new object();
                Stopwatch debugWatch = null;

                BuildOrder buildOrder = Builds.Empty;
                uint currentBuildItem = 0;

#if false
                int supply1 = 0, supply2 = 0;

                var SupplyTimerCallbackProc = new TimerCallback(delegate (object state)
                {
                    using (Bitmap screenCapture = new Bitmap(screenRegion.Width, screenRegion.Height))
                    {
                        if (CopyFromScreen(screenRegion, screenCapture))
                        {
                            using (var scaledCapture = ScaleImage(screenCapture, 2))
                            {
                                using (var ocrEngine = new TesseractEngine(@"C:\Program Files (x86)\Tesseract-OCR\tessdata", "eng"))
                                {
                                    ocrEngine.SetVariable("tessedit_char_whitelist", "0123456789/");
                                    using (var ocrResult = ocrEngine.Process(scaledCapture, Rect.Empty, PageSegMode.SingleLine))
                                    {
                                        string text = ocrResult.GetText().Replace(" ", "").Replace("\n", "");
                                        var match = Regex.Match(text, @"^(\d\d\d?)/(\d\d\d?)$");
                                        if (match.Success)
                                        {
                                            int newSupply1 = Convert.ToInt32(match.Groups[1].Value);
                                            int newSupply2 = Convert.ToInt32(match.Groups[2].Value);

                                            if (newSupply1 != supply1 || newSupply2 != supply2)
                                            {
                                                Console.WriteLine("[{0:D2}:{1:D2}] {2}",
                                                    debugWatch.Elapsed.Minutes,
                                                    debugWatch.Elapsed.Seconds,
                                                    text);

                                                if (newSupply2 != supply2 // changed
                                                    && newSupply2 > newSupply1  // mostly errors
                                                    && newSupply2 - newSupply1 < 8 
                                                    && newSupply2 < 200)
                                                {
                                                    synthesizer.Speak("Supply!");
                                                }

                                                supply1 = newSupply1;
                                                supply2 = newSupply2;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    lock (timerAccessLock)
                    {
                        if (timer != null)
                            timer.Change(1000, -1);
                    }

            });
#endif

                var BuildOrderTimerCallbackProc = new TimerCallback(delegate (object state)
                {
                    if (currentBuildItem < buildOrder.items.Length)
                    {
                        synthesizer.SpeakAsync(String.Format("{0} {1}",
                            buildOrder.items[currentBuildItem].supply,
                            buildOrder.items[currentBuildItem].text));

                        Console.WriteLine("[{0:D2}:{1:D2}] {2}",
                            debugWatch.Elapsed.Minutes,
                            debugWatch.Elapsed.Seconds,
                            buildOrder.items[currentBuildItem].text);

                        ++currentBuildItem;

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
                            timer.Change(1000, 30000);
                        }

                    }
                    else
                    {
                        synthesizer.SpeakAsync("Supply!");
                        return;
                    }

               });

                timer = new Timer(BuildOrderTimerCallbackProc, null, -1, -1);

                using (var keyHook = new GlobalKeyboardHook())
                {
                    bool running = false;
                    int myRace = 0;
                    var races = new int[] { Convert.ToInt32('T'), Convert.ToInt32('Z'), Convert.ToInt32('P'), };

                    var KeyPressCallbackProc = new EventHandler<GlobalKeyboardHookEventArgs>(delegate (object sender, GlobalKeyboardHookEventArgs keypressParams)
                    {
                        if (keypressParams.KeyboardState != GlobalKeyboardHook.KeyboardState.KeyDown)
                            return;

                        var vk = keypressParams.KeyboardData.VirtualCode;

                        if (!running)
                        {
                            if (races.Contains(keypressParams.KeyboardData.VirtualCode))
                            {
                                if (myRace == 0)
                                {
                                    myRace = keypressParams.KeyboardData.VirtualCode;
                                }
                                else
                                {
                                    switch (String.Format("{0}v{1}", (char)myRace, (char)keypressParams.KeyboardData.VirtualCode))
                                    {
                                        case "TvT": buildOrder = Builds.TvT; break;
                                        case "TvP": buildOrder = Builds.TvP; break;
                                        case "TvZ": buildOrder = Builds.TvZ; break;
                                        case "ZvT": buildOrder = Builds.ZvT; break;
                                        case "ZvP": buildOrder = Builds.ZvP; break;
                                        case "ZvZ": buildOrder = Builds.ZvZ; break;
                                        case "PvT": buildOrder = Builds.PvT; break;
                                        case "PvZ": buildOrder = Builds.PvZ; break;
                                        case "PvP": buildOrder = Builds.PvP; break;
                                    }

                                    Console.WriteLine("(Re)starting...");
                                    currentBuildItem = 0;
                                    timer.Change(0, -1);
                                    debugWatch = Stopwatch.StartNew();
                                    running = true;
                                }
                            }
                            else
                            {
                                myRace = 0;
                            }
                        }
                        else if (running && keypressParams.KeyboardData.VirtualCode == 0x79) // VK_F10
                        {
                            timer.Change(-1, -1);
                            debugWatch.Stop();
                            running = false;
                            myRace = 0;
                            Console.WriteLine("Stopped.");
                        }
                    });

                   keyHook.KeyboardPressed += KeyPressCallbackProc;
                   Application.Run();
                }
            }
        }

        static Bitmap ScaleImage(Image image, int factor)
        {
            if (factor == 1)
                return new Bitmap(image);
            else
                return ResizeImage(image, image.Width * factor, image.Height * factor);
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
