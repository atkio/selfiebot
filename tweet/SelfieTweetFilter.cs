using LinqToTwitter;
using System.Collections.Generic;
using System.Linq;

namespace selfiebot
{
    public static class SelfieTweetFilter
    {
        static SelfieBotDB db = new SelfieBotDB();
        static List<string> BlockTexts = db.getBlockTexts();
        static List<string> BandIDs = db.getBandIDs();
        static List<string> NameBlockTexts = db.getNameBlockTexts();

        /// <summary>
        /// 过滤推文
        /// </summary>
        /// <param name="src">推文</param>
        /// <returns></returns>
        public static List<Status> Filter(this List<Status> src)
        {

            return src
             .AsParallel()
             .Where(tw => !BlockTexts.Any(bt => tw.Text.Contains(bt)) && /*推特文字过滤*/
                          !BandIDs.Contains(tw.User.ScreenNameResponse) && /*推特黑名单过滤*/
                          !NameBlockTexts.Any(bt => tw.User.Name.Contains(bt)) && /*推特用户名过滤*/
                          tw.RetweetedStatus.StatusID == 0) /*非转推*/
            .ToList();
        }

        /// <summary>
        /// 过滤推文
        /// </summary>
        /// <param name="src">推文</param>
        /// <returns></returns>
        public static List<Status> Filter(this List<Status> src, List<string> blockedids)
        {
            var iblockedids = new List<string>();
            iblockedids.AddRange(blockedids);
            iblockedids.AddRange(BandIDs);
            return src
             .AsParallel()
             .Where(tw => !BlockTexts.Any(bt => tw.Text.Contains(bt)) && /*推特文字过滤*/
                          !iblockedids.Contains(tw.User.ScreenNameResponse) && /*推特黑名单过滤*/
                          !NameBlockTexts.Any(bt => tw.User.Name.Contains(bt)) && /*推特用户名过滤*/
                          tw.RetweetedStatus.StatusID == 0) /*非转推*/
            .ToList();
        }

        /// <summary>
        /// 从推文中获取图片链接
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static List<WaitRecognizer> GetImageURL(this List<Status> src)
        {
            return src.Distinct()
                   .SelectMany(s => urls(s), (s, url) =>
                      new WaitRecognizer()
                      {
                          TID = s.StatusID.ToString(),
                          UID = s.User.ScreenNameResponse,
                          Tweet = s.Text.Substring(0, 10),
                          PhotoUrl = url
                      })
                   .ToList();
        }

        /// <summary>
        /// 从推文中获取图片链接
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static List<string> urls(Status data)
        {
            var ret = new List<string>();
            if (data.Entities.MediaEntities != null)
            {
                ret.AddRange(data.Entities.MediaEntities.Select(media => media.MediaUrl));
            }

            if (data.ExtendedEntities.MediaEntities != null)
            {
                ret.AddRange(data.ExtendedEntities.MediaEntities.Select(media => media.MediaUrl));
            }

            if (data.Entities.UrlEntities != null)
            {
                ret.AddRange(
                 data.Entities.UrlEntities
                 .Where(urlEntity => (urlEntity.ExpandedUrl.Contains("instagram.com") || urlEntity.ExpandedUrl.Contains("instagr.am")))
                 .Select(urlEntity => urlEntity.ExpandedUrl));
            }
            return ret;
        }


    }
}