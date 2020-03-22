using LinqToTwitter;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace selfiebot
{
    partial class SelfieTweetFunc
    {

        #region 定义

        public SelfieTweetFunc()
        {
            db = new SelfieBotDB();
            var Twitter = db.getAuthorizer();

            authuser = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = Twitter.ConsumerKey,
                    ConsumerSecret = Twitter.ConsumerSecret,
                    AccessToken = Twitter.AccessToken,
                    AccessTokenSecret = Twitter.AccessTokenSecret
                }
            };

            authapp = new ApplicationOnlyAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = Twitter.ConsumerKey,
                    ConsumerSecret = Twitter.ConsumerSecret
                }
            };
        }

        private TwitterContext AuthorizeContext(IAuthorizer auth)
        {
            try
            {
                auth.AuthorizeAsync().Wait(5 * 1000);
                return new TwitterContext(auth);
            }
            catch (Exception e)
            {
                throw new Exception("TwitterAPI　failed." + e.Message);
            }
        }

        /// <summary>
        /// 带用户的认证，本人timeline和转推用
        /// </summary>
        private SingleUserAuthorizer authuser;

        /// <summary>
        /// 不带用户的认证，搜文字和list用
        /// </summary>
        private ApplicationOnlyAuthorizer authapp;
        private SelfieBotDB db;
        #endregion


        /// <summary>
        /// 根据定义的关键字查找
        /// </summary>
        public void SearchTweets()
        {
            ulong maxid;
            var bids = TweetHelper.GetBlockedIDs(authuser);

            foreach (var kv in db.getSearchKey())
            {
                Console.WriteLine("SearchTweets:" + kv.Value + " >" + kv.Key);
                var result = TweetHelper.SearchTweet(authapp, kv.Key, kv.Value, out maxid).Filter(bids);

                Console.WriteLine("SearchTweets newid  >" + maxid);
                db.updateSearchKey(kv.Key, maxid);

                if (result.Count > 0)
                {
                    var todownload = result.GetImageURL().ToList();
                    ImageDownloader.Download(todownload);
                }

            }
        }

        /// <summary>
        /// 查找本人timeline
        /// </summary>
        public void SearchTimeline()
        {

            ulong HTLMaxid = db.getHTLMaxid();
            ulong newid;
            Console.WriteLine("searchTimeline HTLMaxid:" + HTLMaxid);
            var result = TweetHelper.GetHomeTL(authuser, HTLMaxid, out newid).Filter();
            Console.WriteLine("searchTimeline newid:" + newid);
            db.updateHTLMaxid(newid);

            if (result.Count > 0)
            {
                var todownload = result.GetImageURL().ToList();
                ImageDownloader.Download(todownload);
            }
        }

        /// <summary>
        /// 获取list
        /// </summary>
        public void searchList()
        {

            foreach (var listd in db.getLTLMaxid())
            {
                ulong newid;
                var result = TweetHelper.GetList(authapp, listd.UID, listd.LIST, ulong.Parse(listd.SINCEID), out newid, 2000).Filter();
                listd.SINCEID = newid.ToString();
                db.updateLTLMaxid(listd);
                if (result.Count > 0)
                {

                    var todownload = SelfieTweetFilter.GetImageURL(result).ToList();
                    ImageDownloader.Download(todownload);
                }
            }


        }

        /// <summary>
        /// 获取Favs
        /// </summary>
        public void SearchFavs()
        {
            var bids = TweetHelper.GetBlockedIDs(authuser);
            foreach (var kv in db.getFavorites())
            {
                ulong newid;
                var result = TweetHelper.GetUserFavorites(authapp, kv.Key, kv.Value, out newid).Filter(bids);
                db.setFavorites(kv.Key, newid);
                if (result.Count > 0)
                {
                    var todownload = SelfieTweetFilter.GetImageURL(result).ToList();
                    ImageDownloader.Download(todownload);
                }
            }


        }

        /// <summary>
        /// 清除重复文字的推
        /// </summary>
        public void ClearGarbTweet()
        {
            db.CheckWaitRecognizer();
        }

        /// <summary>
        /// 检查已经识别的图片
        /// </summary>
        public void ChkRecognized()
        {

            //色情推
            var pornfiles = Directory.GetFiles(Config.PhotoPornPath).Select(f => Path.GetFileName(f)).ToList();
            db.PornTweet(pornfiles);
            new DirectoryInfo(Config.PhotoPornPath).GetFiles().ToList().ForEach(f => del(f));

            //SEXY推
            var sxfiles = Directory.GetFiles(Config.PhotoSEXYPath).Select(f => Path.GetFileName(f)).ToList();
            db.RTTweet(sxfiles, "1");
            new DirectoryInfo(Config.PhotoSEXYPath).GetFiles().ToList().ForEach(f => del(f));

            //转推
            var rtfiles = Directory.GetFiles(Config.PhotoRTPath).Select(f => Path.GetFileName(f)).ToList();
            db.RTTweet(rtfiles, "0");
            new DirectoryInfo(Config.PhotoRTPath).GetFiles().ToList().ForEach(f => del(f));

            //删除
            var delfiles = Directory.GetFiles(Config.PhotoDeletePath).Select(f => Path.GetFileName(f)).ToList();
            db.DELTweet(delfiles);
            new DirectoryInfo(Config.PhotoDeletePath).GetFiles().ToList().ForEach(f => del(f));

        }

        private static void del(FileInfo f)
        {
            try
            {
                f.Delete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #region 转推方法
        /// <summary>
        /// 发推
        /// </summary>
        /// <param name="st"></param>
        public void post(string st)
        {

            var twitterContext = new TwitterContext(authuser);
            var tweet = twitterContext.TweetAsync(st).Result;
        }

        /// <summary>
        /// 转推
        /// </summary>
        /// <param name="tweetID"></param>
        /// <returns></returns>
        public void reTweet(string tweetID, bool fav = false)
        {

            var twitterContext = new TwitterContext(authuser);
            try
            {
                ulong tid = ulong.Parse(tweetID);
                var retweet = twitterContext.RetweetAsync(tid).Result;
                if (fav)
                    retweet = twitterContext.CreateFavoriteAsync(tid).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("reTweet:" + e.Message);
            }

        }

        public void RTAll()
        {
            //根据推特API要求，3个小时300转推，保证3个小时运行一次
            foreach (WaitRetweet rt in db.getWaitRetweet().Take(300))
            {
                reTweet(rt.TID, rt.RANK == "1");
                db.removeRetweet(rt);
            }
        }
        #endregion
    }
}
