using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace NDS_Cover_downloader {
    class Program {

        static void Main(string[] args) {
            new Program().NonStaticMain(args);
        }

        private List<string> downloadOrder = new List<string>() {
            "http://art.gametdb.com/ds/coverS/EN/{0}.png",
            "http://art.gametdb.com/ds/coverS/US/{0}.png",
            "http://art.gametdb.com/ds/coverS/JA/{0}.png",
            "http://art.gametdb.com/ds/coverS/AU/{0}.png",
        };

        public void NonStaticMain(string[] args) {
            var scanDir = "";

            if (args.Length == 1) {
                scanDir = args[0];
            }

            if (string.IsNullOrEmpty(scanDir)) {
                while (!Directory.Exists(scanDir)) {
                    Console.WriteLine("Path with NDS files: ");
                    scanDir = Console.ReadLine().Replace("\"", "");
                }
            }

            foreach (var ndsFile in Directory.EnumerateFiles(scanDir, "*.nds")) {
                var binaryReader = new BinaryReader(File.OpenRead(ndsFile));
                var titleName = ReadBytes(binaryReader, 0x000, 12);
                var gameCode = ReadBytes(binaryReader, 0x00C, 4);
                if (gameCode.Equals("####")) continue;

                Console.WriteLine($"Game name: {titleName}");
                Console.WriteLine($"Game code: {gameCode}");
                downloadCover(gameCode);
                Console.WriteLine("==============================");
            }

            Console.WriteLine("Done, press ANY key to close the application...");
            Console.ReadKey();
        }

        private string ReadBytes(BinaryReader br, int seek, int length) {
            byte[] test = new byte[length];
            br.BaseStream.Seek(seek, SeekOrigin.Begin);
            br.Read(test, 0, length);
            return Encoding.UTF8.GetString(test);
        }

        private void downloadCover(string gamecode) {
            if (!Directory.Exists("covers")) Directory.CreateDirectory("covers");

            var hasDownload = false;
            var downloadUrlIndex = -1;

            while (!hasDownload) {
                downloadUrlIndex++;
                if (downloadUrlIndex < downloadOrder.Count) {
                    var boxArt = downloadBoxart(string.Format(downloadOrder[downloadUrlIndex], gamecode));
                    hasDownload = boxArt != null;

                    if (boxArt != null) {
                        ConvertTo16bpp(boxArt).Save($"covers/{gamecode}.bmp", ImageFormat.Bmp);
                        Console.WriteLine("Cover downloaded!");
                    }
                } else {
                    Console.WriteLine("No cover found !");
                    return;
                }
            }
        }

        private Bitmap downloadBoxart(string url) {
            WebRequest request = WebRequest.Create(url);
            try {
                using (WebResponse response = request.GetResponse()) {
                    using (Stream responseStream = response.GetResponseStream()) {
                        return new Bitmap(responseStream);
                    }
                }
            } catch { }
            return null;
        }

        private Bitmap ConvertTo16bpp(Image img) {
            var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format16bppRgb555);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }
    }
}
