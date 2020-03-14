namespace selfiebot
{
    class Program
    {
        static void Main(string[] args)
        {
            SelfieTweetFunc func = new SelfieTweetFunc();

            //检查已经识别的
            func.ChkRecognized();

            //转推
            func.RTAll();

            //查找关键字
            func.SearchTweets();

            //查找HomeTL
            func.SearchTimeline();

            //清除垃圾推
            func.ClearGarbTweet();

        }
    }
}
