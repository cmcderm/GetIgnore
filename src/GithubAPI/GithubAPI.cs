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

        public readonly string cachePath;
        public Options flags;

        private const string apiURL = "https://api.github.com/repos/github/gitignore/contents/";
        private const string branchURL = "https://api.github.com/repos/github/gitignore/branches/master";
        private const string rawURL = "https://raw.githubusercontent.com/";
        private ListingCache listCache;
        private ListingCache cache{
            get{
                if(listCache == null)
                {
                    listCache = getCache();
                }
                return listCache;
            }
        }


        public GithubAPI(string CachePath, Options Flags = Options.None){
            cachePath = CachePath;
            flags = Flags;
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
            if(cache.Data.ContainsKey(ignore))
            {
                if(flags.HasFlag(Options.Verbose))
                {
                    Console.WriteLine($"Downloading .gitignore for {ignore}");
                }
                return WebFetch.fetch(cache.Data[ignore]);
            }
            else
            {
                Console.WriteLine($"Exact match to {ignore} not found. ");
                IList<String> searchResults = search(ignore);
                if(searchResults.Count > 1){
                    int choice = UserInputReader.EnumerateChoices(
                        $"There are {searchResults.Count} .gitignore files similar to your choice.",
                        "Enter a selection:",
                        searchResults
                    );

                    if(choice > -1)
                    {
                        // In the case that search adds extra lines for information, clean it up
                        string choiceName = new StringReader(searchResults[choice]).ReadLine();

                        if(flags.HasFlag(Options.Verbose))
                        {
                            Console.WriteLine($"Downloading .gitignore for {choiceName}");
                        }
                        
                        return WebFetch.fetch(cache.Data[choiceName]);
                    }
                }
                else if(searchResults.Count == 1)
                {
                    if(UserInputReader.GetConfirmation($".gitignore {ignore} not found. Did you mean {searchResults[0]}?", false))
                    {
                        string choiceName = new StringReader(searchResults[0]).ReadLine();

                        if(flags.HasFlag(Options.Verbose))
                        {
                            Console.WriteLine($"Downloading .gitignore for {choiceName}");
                        }
                        
                        return WebFetch.fetch(cache.Data[choiceName]);
                    }
                }
            }

            throw new System.IO.FileNotFoundException("Specified .gitignore was not found in the Repository.");
        }

        // Verbose here just represents a way to cancel out the verbosity
        // Not a clean solution by any means /shrug
        public IList<String> search(string ignore)
        {
            List<String> searchResults = new List<String>();
            
            foreach(string listing in cache.Data.Keys)
            {
                if(listing.ToLower().Contains(ignore.ToLower()))
                {
                    if(flags.HasFlag(Options.Verbose))
                    {
                        searchResults.Add($"{listing}{Environment.NewLine}\t{cache.Data[listing]}");
                    }
                    else
                    {
                        searchResults.Add($"{listing}");
                    }
                }
            }
            //TODO: Look into Levenshtein distance for sorting the search results by relevance

            return searchResults;
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
                if(flags.HasFlag(Options.Verbose))
                {
                    Console.WriteLine("Updating cache in memory from Github...");
                }

                c.Update(
                    Listing.FromJson(
                        WebFetch.fetch(apiURL)
                    )
                );

                if(flags.HasFlag(Options.Verbose))
                {
                    Console.WriteLine("Cache updated!");
                }
            };

            if(File.Exists(pathInfo.FullName))
            {
                // Load cache from file
                cache = JsonConvert.DeserializeObject<ListingCache>(File.ReadAllText(pathInfo.FullName));
                cache.flags = flags;

                // Get info branch info from Github
                Branch master = Branch.FromJson(WebFetch.fetch(branchURL));

                if(flags.HasFlag(Options.Verbose))
                {
                    Console.WriteLine($"Cache successfully loaded from {pathInfo.FullName}");
                }
            
                // Get the latest commits timestamp
                DateTimeOffset lastCommitDate = master.Commit.Commit.Author.Date;

                // Check the timestamp
                // Could probably get by just checking the last changed time on the file instead of from cache timestamp?
                if(DateTimeOffset.Compare(lastCommitDate, cache.TimeStamp) <= 0)
                {
                    if(flags.HasFlag(Options.Verbose))
                    {
                        Console.WriteLine($"Cache is outdated.");
                    }
                    cacheUpdate(cache);
                    saveCache(pathInfo.FullName, cache);
                }
                else if(flags.HasFlag(Options.Verbose))
                {
                    Console.WriteLine($"Cache up to date with Github, no updates needed.");
                }
            }
            else
            {
                // Cache file doesn't exist, so create it and (if we're allowed) save it
                // and (if we're allowed) save it to ~/.getignore.cache
                if(flags.HasFlag(Options.Verbose))
                {
                    Console.WriteLine($"No cache found, creating in memory...");
                }
                cache = new ListingCache(flags);
                cacheUpdate(cache);
                saveCache(pathInfo.FullName, cache);
            }

            return cache;
        }

        private void saveCache(String path, ListingCache cache)
        {
            if(!flags.HasFlag(Options.Nocache))
            {
                if(flags.HasFlag(Options.Verbose))
                {
                    Console.WriteLine($"Saving cache to {path}");
                }
                String cacheJSON = JsonConvert.SerializeObject(cache);
                File.WriteAllText(path, cacheJSON);
            }
        }
    }
}