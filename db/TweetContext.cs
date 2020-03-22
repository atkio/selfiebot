using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace selfiebot
{

    public class TweetContext : DbContext
    {
        public DbSet<WaitRecognizer> WaitRecognizer { get; set; }
        public DbSet<WaitRetweet> WaitRetweet { get; set; }

        public DbSet<HomeTimeLineMAXID> HomeTimeLineMAXID { get; set; }

        public DbSet<ListTimeLineMAXID> ListTimeLineMAXID { get; set; }

        public DbSet<SearchKeys> SearchKeys { get; set; }

        public DbSet<WatchUsers> WatchUsers { get; set; }

        public DbSet<BandIDs> BandIDs { get; set; }

        public DbSet<BlockText> BlockText { get; set; }

        public DbSet<BlockName> BlockName { get; set; }

        public DbSet<TwitterAPI> TwitterAPI { get; set; }

        public DbSet<WatchFavorites> WatchFavorites { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=selfie.db");
    }


    #region Table定义

    //推特API认证
    public class TwitterAPI
    {
        [Key]
        public string AccessToken { get; set; }

        public string AccessTokenSecret { get; set; }

        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }
    }

    //等待识别推特表
    public class WaitRecognizer
    {

        public string UID { get; set; }

        public string TID { get; set; }

        public string Tweet { get; set; }

        public string PhotoLocalPath { get; set; }

        public string PhotoUrl { get; set; }

        [Key]
        public string FILENAME { get; set; }
    }

    //等待转推表
    public class WaitRetweet
    {

        [Key]
        public string TID { get; set; }

        public string UID { get; set; }

        public string RANK { get; set; }
    }

    //HomeTL的最新ID
    public class HomeTimeLineMAXID
    {
        [Key]
        public string SINCEID { get; set; }
    }

    //List的最新ID
    public class ListTimeLineMAXID
    {
        [Key]
        public string UID { get; set; }

        public string LIST { get; set; }

        public string SINCEID { get; set; }
    }

    //查询关键字
    public class SearchKeys
    {
        [Key]
        public string KEYWORDS { get; set; } //"我 自己" OR "化妆" OR "自拍" OR "卸妆" OR "素颜" OR "我 头发" OR "眼镜" OR "黑了" OR "胖了" OR "刘海" OR "我 丑" OR "我 拍" OR "selfie 了"

        public string SINCEID { get; set; }
    }

    //查询用户名
    public class WatchUsers
    {
        [Key]
        public string UID { get; set; }

        public string SINCEID { get; set; }
    }

    //Block用户ID
    public class BandIDs
    {
        [Key]
        public string ID { get; set; }
    }

    //Block关键字
    public class BlockText
    {
        [Key]
        public string TEXT { get; set; }
    }

    //Block用户名
    public class BlockName
    {
        [Key]
        public string NAME { get; set; }
    }

    public class WatchFavorites
    {
        [Key]
        public string UID { get; set; }

        public string SINCEID { get; set; }
    }

    #endregion
}

