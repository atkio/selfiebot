using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace selfiebot
{
    /// <summary>
    /// 
    /// </summary>
    public class TweetHelper
    {
        const ulong MINTWITTERID = 204251866668871681;


        static TwitterContext AuthTwitterContext(IAuthorizer authapp)
        {
            authapp.AuthorizeAsync().Wait();
            return new TwitterContext(authapp);
        }


        public static List<string> GetBlockedIDs(SingleUserAuthorizer authuser)
        {
            var twitterCtx = AuthTwitterContext(authuser);
            var blockResponse =
                (from block in twitterCtx.Blocks
                 where block.Type == BlockingType.List
                 select block)
                .SingleOrDefaultAsync()
                .Result;

            if (blockResponse != null && blockResponse.Users != null)
            {
                return blockResponse.Users.Select(user => user.ScreenNameResponse).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// 搜索推文
        /// https://github.com/JoeMayo/LinqToTwitter/wiki/Searching-Twitter
        /// </summary>
        /// <param name="authapp">认证器</param>
        /// <param name="searchtext">搜索文字</param>
        /// <param name="mintid">最小twitterID</param>
        /// <param name="retid">返回最新twitterID</param>
        /// <param name="maxcount">最大搜素推特数</param>
        /// <returns></returns>
        public static List<Status> SearchTweet(ApplicationOnlyAuthorizer authapp, string searchtext, ulong mintid, out ulong retid, int maxcount = 500)
        {

            if (mintid < MINTWITTERID) mintid = MINTWITTERID;
            retid = mintid;


            var twitterCtx = AuthTwitterContext(authapp);
            var rslist = new List<Status>();
            var searchResponse =
              (from search in twitterCtx.Search
               where search.Type == SearchType.Search &&
                     search.Query == searchtext &&
                     search.SinceID == mintid
               select search)
              .SingleOrDefaultAsync()
              .Result;


            if (searchResponse != null && searchResponse.Statuses != null && searchResponse.Statuses.Count > 0)
            {
                retid = searchResponse.Statuses.Max(tw => tw.StatusID);
                rslist.AddRange(searchResponse.Statuses.Where(st => st.StatusID > mintid));
                Console.WriteLine("SearchData   >" + rslist.Count);
            }
            else
            {
                return rslist;
            }

            while (rslist.Count < maxcount && rslist.Count > 0)
            {
                if (rslist.Min(st => st.StatusID) < mintid)
                    break;

                ulong maxid = rslist.Min(st => st.StatusID) - 1;

                Thread.Sleep(10 * 1000);

                searchResponse =
                     (from search in twitterCtx.Search
                      where search.Type == SearchType.Search &&
                            search.Query == searchtext &&
                            search.SinceID == mintid &&
                            search.MaxID == maxid
                      select search)
                      .SingleOrDefaultAsync()
                      .Result;

                if (searchResponse != null && searchResponse.Statuses != null && searchResponse.Statuses.Count > 0)
                {
                    rslist.AddRange(searchResponse.Statuses.Where(st => st.StatusID > mintid));
                    Console.WriteLine("SearchData   >" + rslist.Count);
                }
                else
                {
                    break;
                }
            }

            return rslist;
        }

        /// <summary>
        /// 搜索list
        /// https://github.com/JoeMayo/LinqToTwitter/wiki/Reading-List-Statuses
        /// </summary>
        /// <param name="authapp"></param>
        /// <param name="username"></param>
        /// <param name="listname"></param>
        /// <param name="mintid"></param>
        /// <param name="retid"></param>
        /// <param name="maxStatuses"></param>
        /// <returns></returns>
        public static List<Status> GetList(ApplicationOnlyAuthorizer authapp, string username, string listname, ulong mintid, out ulong retid, int maxStatuses = 500)
        {

            var twitterCtx = AuthTwitterContext(authapp);
            string ownerScreenName = username;
            string slug = listname;
            int lastStatusCount = 0;
            // last tweet processed on previous query
            ulong sinceID = mintid > MINTWITTERID ? mintid : MINTWITTERID;
            ulong maxID;
            int count = 10;
            var statusList = new List<Status>();

            // only count
            var listResponse =
                 (from list in twitterCtx.List
                  where list.Type == ListType.Statuses &&
                        list.OwnerScreenName == ownerScreenName &&
                        list.Slug == slug &&
                        list.Count == count
                  select list)
                .SingleOrDefaultAsync().Result;

            if (listResponse != null && listResponse.Statuses != null)
            {
                List<Status> newStatuses = listResponse.Statuses;

                retid = newStatuses.Max(s => s.StatusID);

                // first tweet processed on current query
                maxID = newStatuses.Min(status => status.StatusID) - 1;
                statusList.AddRange(newStatuses);

                do
                {
                    // now add sinceID and maxID
                    listResponse =
                        (from list in twitterCtx.List
                         where list.Type == ListType.Statuses &&
                               list.OwnerScreenName == ownerScreenName &&
                               list.Slug == slug &&
                               list.Count == count &&
                               list.SinceID == sinceID &&
                               list.MaxID == maxID
                         select list)
                        .SingleOrDefaultAsync().Result;

                    if (listResponse == null)
                        break;

                    if (listResponse.Statuses == null)
                        break;


                    newStatuses = listResponse.Statuses;

                    if (newStatuses.Count < 1)
                        break;

                    // first tweet processed on current query
                    maxID = newStatuses.Min(status => status.StatusID) - 1;
                    statusList.AddRange(newStatuses);

                    lastStatusCount = newStatuses.Count;
                }
                while (lastStatusCount != 0 && statusList.Count < maxStatuses);


                return statusList;
            }
            retid = sinceID;
            return statusList;

        }

        /// <summary>
        /// 搜索本人的TIMELINE
        /// https://github.com/JoeMayo/LinqToTwitter/wiki/Querying-the-Home-Timeline
        /// </summary>
        /// <param name="authuser"></param>
        /// <param name="mintid"></param>
        /// <param name="retid"></param>
        /// <param name="maxcount"></param>
        /// <returns></returns>
        public static List<Status> GetHomeTL(SingleUserAuthorizer authuser, ulong mintid, out ulong retid, int maxcount = 500)
        {

            if (mintid < MINTWITTERID) mintid = MINTWITTERID;
            var twitterCtx = AuthTwitterContext(authuser);

            retid = mintid;

            var rslist =
               (from tweet in twitterCtx.Status
                where tweet.Type == StatusType.Home &&
                   tweet.Count == 200 &&
                   tweet.SinceID == mintid
                select tweet)
               .ToList();


            while (rslist.Count < maxcount && rslist.Count > 0)
            {
                if (rslist.Min(st => st.StatusID) <= mintid)
                    break;

                retid = rslist.Max(st => st.StatusID);

                ulong maxid = rslist.Min(st => st.StatusID) - 1;

                Thread.Sleep(10 * 1000);

                var searchResponse =
                     (from tweet in twitterCtx.Status
                      where tweet.Type == StatusType.Home &&
                         tweet.Count == 200 &&
                         tweet.SinceID == mintid &&
                         tweet.MaxID == maxid
                      select tweet)
                       .ToList();


                if (searchResponse != null && searchResponse.Count > 0)
                {
                    rslist.AddRange(searchResponse);
                }
                else
                {
                    break;
                }

            }


            return rslist;
        }
    }
}
