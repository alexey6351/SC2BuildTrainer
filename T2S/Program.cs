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
            ExecuteBuildOrder(Rectangle.FromLTRB(1750, 15, 1860, 50));
        }

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

        static void ExecuteBuildOrder(Rectangle screenRegion)
        { 
            using (var synthesizer = new SpeechSynthesizer())
            {
                synthesizer.SelectVoice("Microsoft Zira Desktop");
                synthesizer.Volume = 100;  // 0...100
                synthesizer.Rate = 0; // -10 .. 10

                Timer timer = null;
                object timerAccessLock = new object();
                Stopwatch debugWatch = null;

                BuildOrder buildOrder = Builds.Empty;
                uint currentBuildItem = 0;
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
                        timer = new Timer(SupplyTimerCallbackProc, null, 0, -1);
                    }
                });

                using (var keyHook = new GlobalKeyboardHook())
                {
                    bool running = false;

                    var KeyPressCallbackProc = new EventHandler<GlobalKeyboardHookEventArgs>(delegate (object sender, GlobalKeyboardHookEventArgs e)
                    {
                        var vk = e.KeyboardData.VirtualCode;

                        if (!running && 
                             ( e.KeyboardData.VirtualCode == 'T'
                             || e.KeyboardData.VirtualCode == 'Z'
                             || e.KeyboardData.VirtualCode == 'P'))
                        {
                            Console.WriteLine("(Re)starting...");
                            buildOrder = 
                                e.KeyboardData.VirtualCode == 'T' ? Builds.Terran
                                    : e.KeyboardData.VirtualCode == 'Z' ? Builds.Zerg
                                    : e.KeyboardData.VirtualCode == 'P' ? Builds.Protoss
                                    : Builds.Empty;

                            currentBuildItem = 0;
                            timer = new Timer(BuildOrderTimerCallbackProc, null, 0, -1);
                            debugWatch = Stopwatch.StartNew();
                            running = true;
                        }
                        else if (running && e.KeyboardData.VirtualCode == 0x79) // VK_F10
                        {
                            lock (timerAccessLock)
                            {
                                timer.Dispose();
                                timer = null;
                            }
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
