// Generated by https://quicktype.io
//
// To change quicktype's target language, run command:
//
//   "Set quicktype target language"

namespace QuickType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Branch
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("commit")]
        public BranchCommit Commit { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("protected")]
        public bool Protected { get; set; }

        [JsonProperty("protection")]
        public Protection Protection { get; set; }

        [JsonProperty("protection_url")]
        public Uri ProtectionUrl { get; set; }
    }

    public partial class BranchCommit
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("commit")]
        public CommitCommit Commit { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("comments_url")]
        public Uri CommentsUrl { get; set; }

        [JsonProperty("author")]
        public PurpleAuthor Author { get; set; }

        [JsonProperty("committer")]
        public PurpleAuthor Committer { get; set; }

        [JsonProperty("parents")]
        public Parent[] Parents { get; set; }
    }

    public partial class PurpleAuthor
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("avatar_url")]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("gravatar_id")]
        public string GravatarId { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("followers_url")]
        public Uri FollowersUrl { get; set; }

        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; }

        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; }

        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; }

        [JsonProperty("subscriptions_url")]
        public Uri SubscriptionsUrl { get; set; }

        [JsonProperty("organizations_url")]
        public Uri OrganizationsUrl { get; set; }

        [JsonProperty("repos_url")]
        public Uri ReposUrl { get; set; }

        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }

        [JsonProperty("received_events_url")]
        public Uri ReceivedEventsUrl { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public partial class CommitCommit
    {
        [JsonProperty("author")]
        public FluffyAuthor Author { get; set; }

        [JsonProperty("committer")]
        public FluffyAuthor Committer { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("tree")]
        public Tree Tree { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("comment_count")]
        public long CommentCount { get; set; }

        [JsonProperty("verification")]
        public Verification Verification { get; set; }
    }

    public partial class FluffyAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }
    }

    public partial class Tree
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class Verification
    {
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("signature")]
        public object Signature { get; set; }

        [JsonProperty("payload")]
        public object Payload { get; set; }
    }

    public partial class Parent
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }
    }

    /* Already defined in Listing.cs
    public partial class Links
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("html")]
        public Uri Html { get; set; }
    }
    */

    public partial class Protection
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("required_status_checks")]
        public RequiredStatusChecks RequiredStatusChecks { get; set; }
    }

    public partial class RequiredStatusChecks
    {
        [JsonProperty("enforcement_level")]
        public string EnforcementLevel { get; set; }

        [JsonProperty("contexts")]
        public object[] Contexts { get; set; }
    }

    public partial class Branch
    {
        public static Branch FromJson(string json) => JsonConvert.DeserializeObject<Branch>(json, QuickType.Converter.Settings);
    }

    public static partial class Serialize
    {
        public static string ToJson(this Branch self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
    }

    /*
    internal static partial class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
    */
}