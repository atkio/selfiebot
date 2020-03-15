using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace selfiebot
{
    public class ImageDownloader
    {
        private const string JPGEND = ".jpg";
        private const string IMGEND = "og:image";

        public static void Download(List<WaitRecognizer> defs)
        {

            var db = new SelfieBotDB();
            var stored = db.getAllWaitRecognizer().Select(nr => nr.TID).Distinct();
            foreach (var def in defs.Where(d => !stored.Contains(d.TID)))
            {
                if (dl(def))
                    db.addWaitRecognizer(def);
            }
        }

        private static bool dl(WaitRecognizer def)
        {
            if (def.PhotoUrl.Contains("profile_images") || def.PhotoUrl.Contains("emoji"))
                return false;


            if (def.PhotoUrl.Contains("instagram.com") || def.PhotoUrl.Contains("instagr.am"))
            {
                try
                {
                    string str1 = new WebClient().DownloadString(def.PhotoUrl);
                    int num1 = str1.IndexOf(IMGEND) + 11;
                    int num2 = str1.IndexOf(JPGEND, num1 + IMGEND.Length) + 4;
                    def.PhotoUrl = str1.Substring(num1 + IMGEND.Length, num2 - num1 - IMGEND.Length);

                    return savefile(def);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            def.PhotoUrl = def.PhotoUrl + ":orig";

            return savefile(def);
        }

        public static bool savefile(WaitRecognizer def)
        {

            try
            {
                Uri address = new Uri(def.PhotoUrl);
                string localpath = address.LocalPath.EndsWith(":orig") ?
                    address.LocalPath.Substring(0, address.LocalPath.Length - 5) :
                    address.LocalPath;
                def.FILENAME = Path.GetFileName(localpath);
                def.PhotoLocalPath = Path.Combine(Config.PhotoTempPath, def.FILENAME);

                PoolAndDownloadFile(new Uri(def.PhotoUrl), def.PhotoLocalPath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        static void PoolAndDownloadFile(Uri uri, string filePath)
        {


            WebClient webClient = new WebClient();
            byte[] downloadedBytes = webClient.DownloadData(uri);
            int count = 0;
            while (downloadedBytes.Length == 0)
            {
                if (count > 4) throw new Exception("can not download:" + uri);
                Thread.Sleep(2000);
                downloadedBytes = webClient.DownloadData(uri);
                count++;
            }
            Stream file = File.Open(filePath, FileMode.Create);
            file.Write(downloadedBytes, 0, downloadedBytes.Length);
            file.Close();
        }
    }
}
