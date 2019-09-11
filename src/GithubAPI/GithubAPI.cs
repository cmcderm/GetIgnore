using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

using QuickType;

namespace GetIgnore.Github
{
    /// <Summary>
    /// A class to search the github .gitignore repo for the requested file -- Uses the github web API as you might guess.
    /// </Summary>

    /// <Notes>
    /// Some kinda caching would be great, can even quickly check the time of last commit so it's never outdated
    /// This would make sense to merge up into GithubGetIgnore because the line of separation definitely got blurred along the way.
    /// </Notes>
    public class GithubAPI
    {
        // Map of filenames (e.g. visualstudio.gitignore) to the URL to the raw download

        private const string apiURL = "https://api.github.com/repos/github/gitignore/contents/";
        private const string branchURL = "https://api.github.com/repos/github/gitignore/branches/master";
        private const string rawURL = "https://raw.githubusercontent.com/";
        public readonly string cachePath;

        public GithubAPI(){
            if(Environment.OSVersion.Platform == PlatformID.Unix)
            {
                cachePath = Environment.GetEnvironmentVariable("HOME");
            }
            else if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                cachePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.getignore.cache";
            }
            else
            {
                cachePath = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            }
        }

        /// <summary>
        /// Search for and download the specified gitignore, fails if not found
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns>The downloaded .gitignore contents as a string</returns>
        /// <exception cref="FileNotFoundException">File was not found in the repository</exception>
        /// <Example>
        /// Input: "Visual Studio Code" -- Downloads visualstudiocode.gitignore and returns contents
        /// </Example>
        public string download(string ignore)
        {
            ListingCache cache = getCache();

            if(cache.Data.ContainsKey(ignore))
            {
                return WebFetch.fetch(cache.Data[ignore]);
            }
            // Else: use search to find the closest matches and ask what the user wants

            throw new System.IO.FileNotFoundException("Specified .gitignore was not found in the Repository.");
        }

        public string search(string ignore)
        {
            return "Didn't find anything, Captain!";
            // I don't think he looked very hard
        }

        /// <summary>
        /// Call before using the cache dictionary to ensure it is up to date
        /// </summary>
        /// <returns></returns>
        public ListingCache getCache()
        {
            ListingCache cache;

            DirectoryInfo pathInfo = new DirectoryInfo(cachePath);

            Action<ListingCache> cacheUpdate = (c) => {
                // Gets the latest cache from github
                c.Update(
                    Listing.FromJson(
                        WebFetch.fetch(apiURL)
                    )
                );
            };

            if(File.Exists(pathInfo.FullName))
            {
                // Load cache from file
                cache = (ListingCache)JsonConvert.DeserializeObject(File.ReadAllText(pathInfo.FullName));

                // Get info branch info from Github
                Branch master = Branch.FromJson(WebFetch.fetch(branchURL));
            
                // Get the latest commits timestamp
                DateTimeOffset lastCommitDate = master.Commit.Commit.Author.Date;

                // Check the timestamp
                // Could probably get by just checking the last changed time on the file...
                if(DateTimeOffset.Compare(lastCommitDate, cache.TimeStamp) <= 0)
                {
                    cacheUpdate(cache);
                    saveCache(pathInfo.FullName, cache);
                }
            }
            else
            {
                // Cache file doesn't exist, so create it and (if we're allowed) save it
                // and (if we're allowed) save it to ~/.getignore.cache
                cache = new ListingCache();
                cacheUpdate(cache);
                saveCache(pathInfo.FullName, cache);
            }

            return cache;
        }

        private void saveCache(String path, ListingCache cache)
        {
            String cacheJSON = JsonConvert.SerializeObject(cache);
            File.WriteAllText(path, cacheJSON);
        }
    }
}