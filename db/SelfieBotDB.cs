using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace selfiebot
{
    public class SelfieBotDB
    {
        public List<string> getBlockTexts()
        {
            using (var db = new TweetContext())
            {
                return db.BlockText.Select(bt => bt.TEXT).ToList();
            }

        }

        public List<string> getNameBlockTexts()
        {
            using (var db = new TweetContext())
            {
                return db.BlockName.Select(bt => bt.NAME).ToList();
            }
        }



        public Dictionary<string, ulong> getUserList()
        {
            using (var db = new TweetContext())
            {
                return db.WatchUsers.ToDictionary(bt => bt.UID, bt => ulong.Parse(bt.SINCEID));
            }
        }

        public List<string> getBandIDs()
        {
            using (var db = new TweetContext())
            {
                return db.BandIDs.Select(bi => bi.ID).ToList();
            }
        }

        public void addBandIDs(List<string> tID)
        {
            using (var db = new TweetContext())
            {
                var clist = db.BandIDs.Select(bi => bi.ID).ToList();
                var newvals = tID.Except(clist).Select(t => new BandIDs() { ID = t }).ToList();
                db.BandIDs.AddRange(newvals);
                db.SaveChanges();
            }
        }


        public Dictionary<string, ulong> getSearchKey()
        {
            using (var db = new TweetContext())
            {
                return db.SearchKeys.ToDictionary(data => data.KEYWORDS, data => ulong.Parse(data.SINCEID));
            }
        }

        public void updateSearchKey(string key, ulong v)
        {
            using (var db = new TweetContext())
            {
                var datas = db.SearchKeys.Where(d => d.KEYWORDS == key).ToList();
                if (datas.Count > 0)
                {
                    datas.First().SINCEID = v.ToString();
                }
                else
                {
                    db.SearchKeys.Add(new SearchKeys()
                    {
                        KEYWORDS = key,
                        SINCEID = v.ToString()
                    });
                }
                db.SaveChanges();
            }
        }


        public List<WaitRetweet> getWaitRetweet()
        {
            using (var db = new TweetContext())
            {
                return db.WaitRetweet.ToList();
            }
        }

        public void removeRetweet(WaitRetweet rmrt)
        {
            using (var db = new TweetContext())
            {
                db.WaitRetweet.Remove(rmrt);
                db.SaveChanges();
            }
        }



        public void updateUserList(string key, ulong maxid)
        {
            using (var db = new TweetContext())
            {
                var udata = db.WatchUsers.Where(bt => bt.UID == key).ToList();
                foreach (var d in udata)
                {
                    d.SINCEID = maxid.ToString();
                }
                db.SaveChanges();
            }
        }

        public void addToRetweet(string tID, string rank = "0")
        {
            using (var db = new TweetContext())
            {
                try
                {
                    db.WaitRetweet.Add(new WaitRetweet() { TID = tID, RANK = rank, UID = tID });
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        public ulong getHTLMaxid()
        {
            using (var db = new TweetContext())
            {
                var datas = db.HomeTimeLineMAXID.Select(d => d.SINCEID).ToList();

                if (datas.Count < 1)
                    return 3200;
                else
                    return ulong.Parse(datas.First());
            }
        }

        public void updateHTLMaxid(ulong newid)
        {
            using (var db = new TweetContext())
            {

                var datas = db.HomeTimeLineMAXID.ToList();
                if (datas.Count < 1)
                {
                    db.HomeTimeLineMAXID.Add(new HomeTimeLineMAXID() { SINCEID = newid.ToString() });
                }
                else
                {
                    var oldlist = db.HomeTimeLineMAXID.ToList();
                    db.HomeTimeLineMAXID.RemoveRange(oldlist);
                    db.HomeTimeLineMAXID.Add(new HomeTimeLineMAXID() { SINCEID = newid.ToString() });
                }
                db.SaveChanges();
            }

        }
        public List<WaitRecognizer> getAllWaitRecognizer()
        {
            using (var db = new TweetContext())
            {
                return db.WaitRecognizer.ToList();
            }
        }

        public void CheckWaitRecognizer()
        {
            using (var db = new TweetContext())
            {

                var grabids = db.WaitRecognizer
                    .Where(r => !String.IsNullOrWhiteSpace(r.Tweet))
                    .Where(r => r.Tweet.Length > 5)
                    .ToList()
                    .Select(w => new
                    {
                        UID = w.UID,
                        TID = w.TID,
                        TW = w.Tweet
                    })
                    .GroupBy(r => r.TW)
                    .Where(grp => grp.Select(r => r.UID).Distinct().Count() > 1)
                    .SelectMany(grp => grp)
                    .Select(u => u.UID).Distinct()
                    .ToList();

                db.BandIDs.AddRange(grabids.Select(id => new BandIDs() { ID = id }));

                var rmdatas = db.WaitRecognizer
                .Where(w => grabids.Contains(w.UID))
                .ToList();

                db.WaitRecognizer.RemoveRange(rmdatas);

                db.SaveChanges();


            }
        }

        internal List<WaitRecognizer> getAllWaitRecognizerWithTID(String TID)
        {
            using (var db = new TweetContext())
            {
                return
                    db.WaitRecognizer
                    .Where(nr => nr.TID == TID)
                     .ToList();
            }
        }

        public void addWaitRecognizer(WaitRecognizer ul)
        {
            try
            {
                using (var db = new TweetContext())
                {
                    if (!db.WaitRecognizer.Any(w => w.FILENAME == ul.FILENAME))
                    {
                        db.WaitRecognizer.Add(ul);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void removeWaitRecognizer(List<string> filenames)
        {
            using (var db = new TweetContext())
            {
                var rm = db.WaitRecognizer.Where(nr => filenames.Contains(nr.FILENAME)).ToList();
                db.WaitRecognizer.RemoveRange(rm);
                db.SaveChanges();
            }
        }

        public void removeWaitRecognizer(List<WaitRecognizer> rm)
        {
            using (var db = new TweetContext())
            {
                db.WaitRecognizer.RemoveRange(rm);
                db.SaveChanges();
            }
        }
        public List<ListTimeLineMAXID> getLTLMaxid()
        {
            using (var db = new TweetContext())
            {
                return db.ListTimeLineMAXID.ToList();
            }
        }

        public void updateLTLMaxid(ListTimeLineMAXID data)
        {
            using (var db = new TweetContext())
            {

                var datas = db.ListTimeLineMAXID.Where(d => d.UID == data.UID && d.LIST == data.LIST).ToList();

                if (datas.Count > 0)
                {
                    datas.First().SINCEID = data.SINCEID;
                }
                else
                {
                    db.ListTimeLineMAXID.Add(data);
                }
                db.SaveChanges();

            }
        }

        public TwitterAPI getAuthorizer()
        {
            using (var db = new TweetContext())
            {
                return db.TwitterAPI.First();
            }
        }

        public void PornTweet(List<string> pornfiles)
        {
            using (var db = new TweetContext())
            {
                var PornTweets = db.WaitRecognizer
                .Where(w => pornfiles.Contains(w.FILENAME))
                .Select(w => w.TID)
                .Distinct()
                .ToList();

                var delRcg = db.WaitRecognizer
                 .Where(w => PornTweets.Contains(w.TID))
                 .ToList();

                db.WaitRecognizer.RemoveRange(delRcg);
                db.SaveChanges();
            }
        }

        public void RTTweet(List<string> rtfiles, string rank)
        {
            using (var db = new TweetContext())
            {
                var rtrcg = db.WaitRecognizer
                    .Where(w => rtfiles.Contains(w.FILENAME))
                    .ToList();

                if (rtrcg.Count() > 0)
                {
                    var existedTID = db.WaitRetweet.Select(w => w.TID).ToList();
                    var rtrcg2 = rtrcg
                    .Select(w => new
                    {
                        TID = w.TID,
                        UID = w.UID
                    })
                    .GroupBy(w => w.TID)
                    .Select(w => new WaitRetweet()
                    {
                        TID = w.Key,
                        UID = w.First().UID,
                        RANK = rank
                    })
                    .Where(w => !existedTID.Contains(w.TID))
                    .ToList();

                    db.WaitRetweet.AddRange(rtrcg2);
                }
                db.WaitRecognizer.RemoveRange(rtrcg);
                db.SaveChanges();

            }
        }


        public void DELTweet(List<string> delfiles)
        {
            using (var db = new TweetContext())
            {
                var delRcg2 = db.WaitRecognizer
                 .Where(w => delfiles.Contains(w.FILENAME))
                 .ToList();
                db.WaitRecognizer.RemoveRange(delRcg2);
                db.SaveChanges();
            }
        }
    }

}
