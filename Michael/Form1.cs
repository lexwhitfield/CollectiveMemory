using CollectiveMemoryHelper;
using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tweetinvi;
using Tweetinvi.Core.Credentials;
using Tweetinvi.Core.Parameters;

namespace Michael
{
    public partial class Form1 : Form
    {
        public static List<string> punctuation;
        public static List<string> numbers;
        public static List<string> terminators;

        protected ContextMenu contextMenu;

        public static Dictionary<TokenType, string> tokenTypes;
        public static Dictionary<string, TokenType> tokenTypesReversed;

        public static Corpus currentCorpus;

        public static string cs;

        public Form1()
        {
            InitializeComponent();
            InitializeMichael();
            BuildContextMenu();

            // credentials = new TwitterCredentials(Form1.ConsumerKey, Form1.ConsumerSecret, Form1.AccessToken, Form1.AccessTokenSecret);
            Form1.cs = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;


        }

        private void InitializeMichael()
        {
            tokenTypes = new Dictionary<TokenType, string>();
            tokenTypes.Add(TokenType.ADJECTIVE, "AJ");
            tokenTypes.Add(TokenType.ADVERB, "AV");
            tokenTypes.Add(TokenType.ARTICLE, "AR");
            tokenTypes.Add(TokenType.CONJUNCTION, "CJ");
            tokenTypes.Add(TokenType.INTERJECTION, "IJ");
            tokenTypes.Add(TokenType.NOUN, "N");
            tokenTypes.Add(TokenType.PREPOSITION, "PR");
            tokenTypes.Add(TokenType.PRONOUN, "PN");
            tokenTypes.Add(TokenType.PUNCTUATION, "#");
            tokenTypes.Add(TokenType.UNKNOWN, "?");
            tokenTypes.Add(TokenType.VERB, "V");

            tokenTypesReversed = new Dictionary<string, TokenType>();
            tokenTypesReversed.Add("AJ", TokenType.ADJECTIVE);
            tokenTypesReversed.Add("AV", TokenType.ADVERB);
            tokenTypesReversed.Add("AR", TokenType.ARTICLE);
            tokenTypesReversed.Add("CJ", TokenType.CONJUNCTION);
            tokenTypesReversed.Add("IJ", TokenType.INTERJECTION);
            tokenTypesReversed.Add("N", TokenType.NOUN);
            tokenTypesReversed.Add("PR", TokenType.PREPOSITION);
            tokenTypesReversed.Add("PN", TokenType.PRONOUN);
            tokenTypesReversed.Add("#", TokenType.PUNCTUATION);
            tokenTypesReversed.Add("?", TokenType.UNKNOWN);
            tokenTypesReversed.Add("V", TokenType.VERB);

            punctuation = new List<string>(){
                "\"", "£", "$", "%", "&", "=", "*", "(", ")", "{", "}", "[", "]", "@",
                "~", "#", "<", ">", "/", "-", "?", "!", "'", ":", ";", ".", ","
            };

            numbers = new List<string>(){
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
            };

            terminators = new List<string>(){
                ".", "?", "!"
            };

            currentCorpus = new Corpus();
        }

        private void BuildContextMenu()
        {
            contextMenu = new ContextMenu();

            MenuItem type = new MenuItem("Type");

            foreach (string s in Form1.tokenTypes.Values)
            {
                MenuItem item = new MenuItem(s);
                item.RadioCheck = true;
                item.Click += new EventHandler(item_Click);
                type.MenuItems.Add(item);
            }

            contextMenu.MenuItems.Add(type);
        }

        void item_Click(object sender, EventArgs e)
        {
            //get the id of the label clicked
            object o = sender;

            //get menuitem clicked
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //get friends

            //try
            //{
            //    Auth.SetUserCredentials(Form1.ConsumerKey, Form1.ConsumerSecret, Form1.AccessToken, Form1.AccessTokenSecret);

            //    var loggedUser = User.GetLoggedUser(credentials);

            //    var follows = loggedUser.GetFriends(250);

            //    int count = follows.Count();

            //    foreach (var followee in follows)
            //    {
            //        SqlHelper.ExecuteNonQuery(cs, "AddTweetSource",
            //            followee.Name,
            //            followee.ScreenName,
            //            followee.Id,
            //            followee.Location,
            //            followee.Url,
            //            followee.FollowersCount,
            //            followee.CreatedAt.ToShortDateString(),
            //            followee.Description);
            //    }
            //}
            //catch (AggregateException ex)
            //{
            //    Console.Write(ex.Message);
            //}
        }

        private Color SelectColor(TokenType t)
        {
            switch (t)
            {
                case TokenType.ADJECTIVE:
                    return Color.FromArgb(150, 215, 90);
                case TokenType.ADVERB:
                    return Color.FromArgb(230, 135, 100);
                case TokenType.ARTICLE:
                    return Color.FromArgb(150, 145, 225);
                case TokenType.CONJUNCTION:
                    return Color.FromArgb(240, 215, 105);
                case TokenType.INTERJECTION:
                    return Color.FromArgb(145, 130, 80);
                case TokenType.NOUN:
                    return Color.FromArgb(60, 115, 185);
                case TokenType.PREPOSITION:
                    return Color.FromArgb(225, 160, 175);
                case TokenType.PRONOUN:
                    return Color.FromArgb(140, 205, 245);
                case TokenType.PUNCTUATION:
                    return Color.FromArgb(150, 150, 150);
                case TokenType.UNKNOWN:
                    return Color.White;
                case TokenType.VERB:
                    return Color.FromArgb(195, 70, 60);
            }

            return Color.White;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ThreadStart childref = new ThreadStart(GetTweets);
            Thread childThread = new Thread(childref);
            childThread.Start();

            //var rate = RateLimit.GetCredentialsRateLimits(credentials, false);
            ///this.Text = rate.StatusesUserTimelineLimit.Remaining.ToString();
        }

        public static void GetTweets()
        {
            int totalTweets = 0;
            int tweeterIndex = 1;

            Auth.SetUserCredentials(TwitterSettings.ConsumerKey, TwitterSettings.ConsumerSecret, TwitterSettings.AccessToken, TwitterSettings.AccessTokenSecret);

            //get all tweetsources
            CollectiveMemoryHelper.Logger.WriteLine("Michael", "GETTING TWEETSOURCES", true, true);
            DataTable tweeters = SqlHelper.ExecuteDataset(cs, CommandType.Text, "SELECT ID, ScreenName FROM TweetSource").Tables[0];

            int totalTweeters = tweeters.Rows.Count;

            //foreach tweetsource get tweets
            foreach (DataRow tweeter in tweeters.Rows)
            {
                //Console.Write("Processing: #" + tweeterIndex + " : " + tweeter["ScreenName"].ToString());

                CollectiveMemoryHelper.Logger.WriteLine("Michael", "Processing: #" + tweeterIndex + "/" + totalTweeters + " : " + tweeter["ScreenName"].ToString(), true, false);

                long tweeterID = Convert.ToInt64(tweeter["ID"]);

                //get id of last tweet
                object lastTweetID = SqlHelper.ExecuteScalar(cs, CommandType.Text, "SELECT Top 1 TweetID FROM Tweet WHERE TweetSourceID = " + tweeterID + " ORDER BY Created DESC");

                var user = User.GetUserFromScreenName(tweeter["ScreenName"].ToString());

                var userTimelineParameters = new UserTimelineParameters();
                userTimelineParameters.ExcludeReplies = true;
                userTimelineParameters.IncludeRTS = false;

                if (lastTweetID != null && lastTweetID != DBNull.Value)
                    userTimelineParameters.SinceId = Convert.ToInt64(lastTweetID);
                else
                    userTimelineParameters.MaximumNumberOfTweetsToRetrieve = 200;

                var tweets = Timeline.GetUserTimeline(user.UserIdentifier, userTimelineParameters);

                if (tweets != null)
                {
                    int tweetsSaved = 0;

                    foreach (var tweet in tweets)
                    {
                        // don't care is no articles are linked or is a retweet
                        if (!tweet.IsRetweet && tweet.Urls.Count > 0)
                        {
                            //tweet
                            string datetime = tweet.CreatedAt.ToString();
                            SqlHelper.ExecuteNonQuery(cs, "AddTweet", tweeterID, tweet.Id, tweet.Text, datetime);
                            tweetsSaved++;

                            //hashtags
                            if (tweet.Hashtags.Count > 0)
                            {
                                foreach (var hashtag in tweet.Hashtags)
                                {
                                    SqlHelper.ExecuteNonQuery(cs, "AddTweetHashtag", tweet.Id, hashtag.Text);
                                }
                            }

                            //mentions
                            if (tweet.UserMentions.Count > 0)
                            {
                                foreach (var mentions in tweet.UserMentions)
                                {
                                    SqlHelper.ExecuteNonQuery(cs, "AddTweetMention", tweet.Id, mentions.ScreenName, mentions.Id);
                                }
                            }

                            //urls
                            foreach (var url in tweet.Urls)
                            {
                                SqlHelper.ExecuteNonQuery(cs, "AddTweetUrl", tweet.Id, url.ExpandedURL);
                            }
                        }
                    }

                    CollectiveMemoryHelper.Logger.WriteLine("Michael", tweetsSaved + " tweets saved", false, true);
                    totalTweets += tweetsSaved;
                }
                else
                {
                    CollectiveMemoryHelper.Logger.WriteLine("Michael", " Tweets returned NULL",false, true);
                }

                tweeterIndex++;
                Thread.Sleep(1000);
            }

            CollectiveMemoryHelper.Logger.WriteLine("Michael", "Total Tweets: " + totalTweets,true, true);
            CollectiveMemoryHelper.Logger.WriteLine("Michael", "TWEET HARVEST COMPLETE",true, true);

        }
    }
}
