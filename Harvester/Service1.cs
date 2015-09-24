using CollectiveMemoryHelper;
using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using Tweetinvi;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Core.Parameters;

namespace Harvester
{
    public partial class Service1 : ServiceBase
    {
        public static string cs;
        public static List<string> IgnoredHashtags; //to eliminate crap like big brother and americas next top model
        public static List<string> IgnoredUrls; //mostly to eliminate image and video links, we only care about text
        public static List<string> UntrimedHosts;

        public Service1()
        {
            InitializeComponent();

            //Init();
            cs = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            Init();
        }

        protected override void OnStart(string[] args)
        {
            CollectiveMemoryHelper.Logger.WriteLine("Harvester", "STARTING SERVICE", true, true);
            CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Starting Tweet Harvest", true, true);
            HarvestTweets();
        }

        protected override void OnStop()
        {
            CollectiveMemoryHelper.Logger.WriteLine("Harvester", "STOPPING SERVICE", true, true);
        }

        public void Init()
        {
            cs = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;

            //build hashtag and url lists
            DataTable dtHashtags = SqlHelper.ExecuteDataset(cs, CommandType.Text, "SELECT DISTINCT Hashtag FROM Hashtag WHERE HashtagStatusID = 1").Tables[0];

            IgnoredHashtags = new List<string>();
            foreach (DataRow dr in dtHashtags.Rows)
            {
                IgnoredHashtags.Add(dr["Hashtag"].ToString());
            }

            DataTable dtHosts = SqlHelper.ExecuteDataset(cs, CommandType.Text, "SELECT DISTINCT HostUrl FROM Host WHERE Ignore = 1").Tables[0];

            IgnoredUrls = new List<string>();
            foreach (DataRow dr in dtHosts.Rows)
            {
                IgnoredUrls.Add(dr["HostUrl"].ToString());
            }

            DataTable dtUntrimmed = SqlHelper.ExecuteDataset(cs, CommandType.Text, "SELECT DISTINCT HostUrl FROM Host WHERE RemoveQueryString = 0").Tables[0];

            UntrimedHosts = new List<string>();
            foreach (DataRow dr in dtUntrimmed.Rows)
            {
                UntrimedHosts.Add(dr["HostUrl"].ToString());
            }
        }

        public void HarvestTweets()
        {
            int totalTweets = 0;
            int tweeterIndex = 1;

            Auth.SetUserCredentials(TwitterSettings.ConsumerKey, TwitterSettings.ConsumerSecret, TwitterSettings.AccessToken, TwitterSettings.AccessTokenSecret);

            try
            {
                DataTable tweeters = SqlHelper.ExecuteDataset(cs, "GetTweetSourcesForHarvesting").Tables[0];

                int totalTweeters = tweeters.Rows.Count;
                CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Harvesting from " + totalTweeters + " sources", true, true);

                foreach (DataRow tweeter in tweeters.Rows)
                {
                    CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Processing: #" + tweeterIndex + "/" + totalTweeters + " : " + tweeter["ScreenName"].ToString(), true, true);

                    long tweeterID = Convert.ToInt64(tweeter["ID"]);

                    //get id of last tweet
                    object lastTweetID = SqlHelper.ExecuteScalar(cs, CommandType.Text, "SELECT Top 1 TweetID FROM Tweet WHERE TweetSourceID = " + tweeterID + " ORDER BY Created DESC");

                    var user = User.GetUserFromScreenName(tweeter["ScreenName"].ToString());

                    var userTimelineParameters = new UserTimelineParameters();
                    userTimelineParameters.ExcludeReplies = true;
                    userTimelineParameters.IncludeRTS = false;
                    //no replies or retweets

                    if (lastTweetID != null && lastTweetID != DBNull.Value)
                        userTimelineParameters.SinceId = Convert.ToInt64(lastTweetID);
                    else
                        userTimelineParameters.MaximumNumberOfTweetsToRetrieve = 200; //get 200 recent tweets for new tweet sources

                    var tweets = Timeline.GetUserTimeline(user.UserIdentifier, userTimelineParameters);

                    if (tweets != null)
                    {
                        int tweetsSaved = 0, hashtagCount = 0, mentionCount = 0, urlCount = 0;
                        int tweetCounter = 1;

                        CollectiveMemoryHelper.Logger.WriteLine("Harvester", ((List<ITweet>)tweets).Count + " possible tweets", true, true);

                        foreach (var tweet in tweets)
                        {
                            // don't care if no articles are linked or is a retweet
                            if (!tweet.IsRetweet && tweet.Urls.Count > 0)
                            {
                                bool worthwhileTweet = true;
                                //urls
                                Dictionary<string, string> urls = new Dictionary<string, string>();

                                foreach (var url in tweet.Urls)
                                {
                                    Uri uri = new Uri(url.URL);
                                    HttpWebRequest www = (HttpWebRequest)WebRequest.CreateDefault(uri);
                                    www.Proxy = null;
                                    www.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
                                    www.AllowAutoRedirect = true;
                                    www.Credentials = CredentialCache.DefaultNetworkCredentials;
                                    www.Method = "GET";
                                    www.Accept = "text/html";
                                    www.Timeout = 60000;
                                    //www.CookieContainer = new CookieContainer();

                                    //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };

                                    try
                                    {
                                        using (WebResponse resp = www.GetResponse())
                                        {
                                            string hosturl = resp.ResponseUri.Host.Replace("www.", "");

                                            if (IgnoredUrls.Contains(hosturl))
                                                worthwhileTweet = false;

                                            foreach (var hashtag in tweet.Hashtags)
                                            {
                                                if (IgnoredHashtags.Contains(hashtag.Text))
                                                    worthwhileTweet = false;
                                            }

                                            if (worthwhileTweet && !urls.ContainsKey(resp.ResponseUri.OriginalString))
                                            {
                                                if (UntrimedHosts.Contains(hosturl))
                                                    urls.Add(resp.ResponseUri.OriginalString, hosturl);
                                                else
                                                    urls.Add(resp.ResponseUri.OriginalString.Split('?')[0], hosturl);
                                            }
                                        }
                                    }
                                    catch (WebException e)
                                    {
                                        worthwhileTweet = false;
                                        CollectiveMemoryHelper.Logger.WriteLine("Harvester", "ERROR: " + e.Message, true, true);
                                        CollectiveMemoryHelper.Logger.WriteLine("Harvester", "STATUS:" + e.Status.ToString(), true, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        worthwhileTweet = false;
                                        CollectiveMemoryHelper.Logger.WriteLine("Harvester", "ERROR: " + ex.Message, true, true);
                                    }
                                }

                                if (worthwhileTweet)
                                {
                                    //tweet
                                    string datetime = tweet.CreatedAt.ToString();
                                    SqlHelper.ExecuteNonQuery(cs, "AddTweet", tweeterID, tweet.Id, tweet.Text, datetime);
                                    tweetsSaved++;

                                    foreach (KeyValuePair<string, string> url in urls)
                                    {
                                        SqlHelper.ExecuteNonQuery(cs, "AddTweetUrl", tweet.Id, url.Key, url.Value);
                                        urlCount++;
                                    }

                                    //hashtags
                                    foreach (var hashtag in tweet.Hashtags)
                                    {
                                        SqlHelper.ExecuteNonQuery(cs, "AddTweetHashtag", tweet.Id, hashtag.Text);
                                        hashtagCount++;
                                    }

                                    //mentions
                                    foreach (var mention in tweet.UserMentions)
                                    {
                                        SqlHelper.ExecuteNonQuery(cs, "AddTweetMention", tweet.Id, mention.ScreenName, mention.Id);
                                        mentionCount++;
                                    }

                                    //CollectiveMemoryHelper.Logger.WriteLine("Harvester", tweetCounter + " : Tweet Saved", true, true);
                                    tweetCounter++;
                                }
                            }
                        }

                        SqlHelper.ExecuteNonQuery(cs, "AddTweetHarvestLog", tweeterID, tweetsSaved, hashtagCount, mentionCount, urlCount);
                        CollectiveMemoryHelper.Logger.WriteLine("Harvester", tweetsSaved + " tweets saved", true, true);
                        totalTweets += tweetsSaved;
                    }
                    else
                    {
                        CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Tweets returned NULL", true, true);
                    }

                    tweeterIndex++;

                    //var ratelimits = RateLimit.GetCurrentCredentialsRateLimits(false);
                    //CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Rate Limit: " + ratelimits.StatusesUserTimelineLimit, true, true);
                }

                CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Total Tweets: " + totalTweets, true, true);
                CollectiveMemoryHelper.Logger.WriteLine("Harvester", "TWEET HARVEST COMPLETE", true, true);
            }
            catch (Exception e)
            {
                CollectiveMemoryHelper.Logger.WriteLine("Harvester", e.ToString(), true, true);
            }
        }

        #region TIMERS

        private void TweetHarvestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Starting Tweet Harvest", true, true);
            HarvestTweets();
        }

        private void StatisticGeneratorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Starting Stats Generation", true, true);

            //if its monday and it hasn't already run do stats for last week

            //if yesterdays stats haven't been generated yet do that

            CollectiveMemoryHelper.Logger.WriteLine("Harvester", "Stats Generation Complete", true, true);
        }

        #endregion
    }
}
