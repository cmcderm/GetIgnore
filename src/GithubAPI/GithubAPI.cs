using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

using QuickType;

namespace GetIgnore.Github
{
    /// <Summary>
    /// A class to search the github .gitignore repo for the requested file -- Uses the github web API as you might guess
    /// </Summary>
    /// <Notes>
    /// Whitespace isn't allowed, trims everything. "Visual Studio Code" becomes visualstudiocode
    /// This is fine because nothing in the gitignore has spaces and I'm ignoring case
    /// Some kinda caching would be great, can even quickly check the time of last commit so it's never outdated
    /// </Notes>
    public class GithubAPI
    {
        const string apiURL = "https://api.github.com/repos/github/gitignore/contents/";
        const string rawURL = "https://raw.githubusercontent.com/";

        /// <summary>
        /// Search for and download the specified gitignore, fails if not found
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns>The downloaded .gitignore contents</returns>
        /// <exception cref="FileNotFoundException">File was not found in the repository</exception>
        public string download(string ignore){
            string output;

            string listingJSON = WebFetch.fetch(apiURL);

            //Console.WriteLine("Listing JSON: {0}", listingJSON);

            // Buffer of listings before putting into queue
            Listing[] listings = Listing.FromJson(listingJSON);

            // Queue of listings to check next
            Queue<Listing> listingsQueue = new Queue<Listing>(listings);
            
            // Dirs to throw back into the queue when it's empty
            Stack<Listing> dirs = new Stack<Listing>();

            String tabs = "";
            // Read through all listings in the queue, push any dirs to stack
            // Do-While because Count check at this point is redundant
            do
            {
                while(listingsQueue.Count > 0)
                {
                    Listing list = listingsQueue.Dequeue();
                    Console.WriteLine("{0}{1}", tabs, list.Name);
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
                            $"{WebFetch.fetch(list.DownloadUrl)}" +
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
                    string dirJSON = WebFetch.fetch(dirs.Pop().Url);
                    listings = Listing.FromJson(dirJSON);
                    foreach(Listing l in listings)
                    {
                        listingsQueue.Enqueue(l);
                    }
                }
                tabs += "\t";
              
            } while (listingsQueue.Count > 0);

            throw new System.IO.FileNotFoundException("Specified .gitignore was not found in the Repository.");
        }

        public string search(string ignore){
            return "Didn't find anything, Captain!";
            // I don't think he looked very hard
        }
    }
}