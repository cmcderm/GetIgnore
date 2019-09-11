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
        public const string cachePath = "~/.getignore.cache";

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
            string output;

            string listingJSON = WebFetch.fetch(apiURL);

            //Console.WriteLine("Listing JSON: {0}", listingJSON);

            // Buffer of listings before putting into queue
            Listing[] listings = Listing.FromJson(listingJSON);

            // Queue of listings to check next
            Queue<Listing> listingsQueue = new Queue<Listing>(listings);
            
            // Dirs to throw back into the queue when it's empty
            Stack<Listing> dirs = new Stack<Listing>();

            // Read through all listings in the queue, push any dirs to stack
            // Do-While because Count check at this point is redundant
            do
            {
                while(listingsQueue.Count > 0)
                {
                    Listing list = listingsQueue.Dequeue();

                    if(list.Type == TypeEnum.Dir)
                    {
                        dirs.Push(list);
                    }
                    else
                    {
                        // This iteration is going to bail as soon as we find a match
                        if(list.Name.ToLower().Equals(ignore.ToLower() + ".gitignore"))
                        {
                            output = $"####### File downloaded by GetIgnore from {list.DownloadUrl} #######" + Environment.NewLine +
                            WebFetch.fetch(list.DownloadUrl) + Environment.NewLine +
                            "#######";
                            return output;
                        }
                    }
                }
                // After queue is empty, if there are dirs to check, fetch put their contents in queue
                // This can lead to the queue to really explode -- Keep an eye on memory!
                // It's important to note that this won't be reached if the .gitignore is found in the root
                while(dirs.Count > 0)
                {
                    // Fetch contents from folder
                    Console.WriteLine("About to expand dir: {0}", dirs.Peek().Path);
                    string dirJSON = WebFetch.fetch(dirs.Pop().Url);
                    listings = Listing.FromJson(dirJSON);
                    foreach(Listing l in listings)
                    {
                        listingsQueue.Enqueue(l);
                    }
                }
              
            } while (listingsQueue.Count > 0);

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

            Action<ListingCache> cacheUpdate = (cache) => {
                // Gets the latest cache from github
                cache.Update(
                    Listing.FromJson(
                        WebFetch.fetch(apiURL)
                    )
                );
            };

            if(File.Exists(cachePath))
            {
                // Load cache from file
                cache = (ListingCache)JsonConvert.DeserializeObject(File.ReadAllText("~/.getignore.cache"));

                // Get info branch info from Github
                Branch master = Branch.FromJson(WebFetch.fetch(branchURL));
            
                // Get the latest commits timestamp
                DateTimeOffset lastCommitDate = master.Commit.Commit.Author.Date;

                // Check the timestamp
                // Could probably get by just checking the last changed time on the file...
                if(DateTimeOffset.Compare(lastCommitDate, cache.TimeStamp) <= 0)
                {
                    cacheUpdate(cache);
                    saveCache(cachePath, cache);
                }
            }
            else
            {
                // Cache file doesn't exist, so create it and (if we're allowed) save it
                // and (if we're allowed) save it to ~/.getignore.cache
                cache = new ListingCache();
                cacheUpdate(cache);
                saveCache(cachePath, cache);
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