using System.IO;

namespace selfiebot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                initFolder();

                SelfieTweetFunc func = new SelfieTweetFunc();

                //检查已经识别的
                func.ChkRecognized();

                //转推
                func.RTAll();

                //查找关键字
                func.SearchTweets();

                //查找HomeTL
                func.SearchTimeline();

                //查找Favs
                func.SearchFavs();

                //清除垃圾推
                func.ClearGarbTweet();
            }
            finally
            {

            }

        }

        static void initFolder()
        {
            if (!Directory.Exists(Config.PhotoTempPath))
            {
                Directory.CreateDirectory(Config.PhotoTempPath);
            }

            if (!Directory.Exists(Config.PhotoPornPath))
            {
                Directory.CreateDirectory(Config.PhotoPornPath);
            }

            if (!Directory.Exists(Config.PhotoRTPath))
            {
                Directory.CreateDirectory(Config.PhotoRTPath);
            }

            if (!Directory.Exists(Config.PhotoDeletePath))
            {
                Directory.CreateDirectory(Config.PhotoDeletePath);
            }

            if (!Directory.Exists(Config.PhotoSEXYPath))
            {
                Directory.CreateDirectory(Config.PhotoSEXYPath);
            }
        }

    }
}
