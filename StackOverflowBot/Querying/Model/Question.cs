using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace StackOverflowBot.Querying.Model
{
    public class Question
    {

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("is_answered")]
        public bool IsAnswered { get; set; }

        [JsonProperty("view_count")]
        public int ViewCount { get; set; }

        [JsonProperty("answer_count")]
        public int AnswerCount { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("last_activity_date")]
        public int LastActivityDate { get; set; }

        [JsonProperty("creation_date")]
        public int CreationDate { get; set; }

        [JsonProperty("question_id")]
        public int QuestionId { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(@"<h1><a href=""");
            builder.Append(this.Link);
            builder.Append(@""">");
            builder.Append(this.Title);
            builder.Append("</a></h1>");
            builder.Append("\n\n\r\n");
            builder.Append(@"<h3 style=""color: grey"">");
            if (this.AnswerCount > 0)
                builder.Append($"Answered | ");
            builder.Append(@"Asked by <a href=""");
            builder.Append(this.Owner.Link);
            builder.Append(@""">");
            builder.Append(this.Owner.DisplayName);
            builder.Append("</a> (");
            builder.Append(this.Owner.Reputation);
            builder.Append(" reputation) ");
            builder.Append(GetPrettyDate(DateTimeOffset.FromUnixTimeSeconds(this.CreationDate).DateTime));
            builder.Append(".</h3>");
            builder.Append("\n\n\r\n");
            builder.Append(this.Body);
            return builder.ToString();
        }

        static string GetPrettyDate(DateTime d)
        {
            TimeSpan s = DateTime.Now.Subtract(d);
            int dayDiff = (int)s.TotalDays;
            int secDiff = (int)s.TotalSeconds;

            if (dayDiff < 0 || dayDiff >= 31)
                return null;

            if (dayDiff == 0)
            {
                if (secDiff < 60)
                    return "just now";
                if (secDiff < 120)
                    return "1 minute ago";
                if (secDiff < 3600)
                    return string.Format("{0} minutes ago", Math.Floor((double)secDiff / 60));
                if (secDiff < 7200)
                    return "1 hour ago";
                if (secDiff < 86400)
                    return string.Format("{0} hours ago", Math.Floor((double)secDiff / 3600));
            }

            if (dayDiff == 1)
                return "yesterday";
            if (dayDiff < 7)
                return string.Format("{0} days ago", dayDiff);
            if (dayDiff < 31)
                return string.Format("{0} weeks ago", Math.Ceiling((double)dayDiff / 7));

            return null;
        }

    }

}
