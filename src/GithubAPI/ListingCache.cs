using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QuickType;

namespace GetIgnore.Github
{
    public class ListingCache
    {
        public DateTimeOffset TimeStamp{get;}
        public Dictionary<String, Uri> Data{get;}

        public ListingCache(){
            Data = new Dictionary<String, Uri>(StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Takes in the root listing of the gitignore, and "recursively" caches all files into a Dictionary
        /// </summary>
        /// <param name="listings"></param>
        public void Update(Listing[] listings)
        {
            Console.WriteLine("Writing cache...");
            // Queue of listings to check next
            Queue<Listing> listingsQueue = new Queue<Listing>(listings);
            
            // Dirs to throw back into the queue when it's empty
            Stack<Listing> dirs = new Stack<Listing>();

            String ignorePattern = @"(.*)\.gitignore";

            // Note that it's not actually recurisve
            // I find recursion difficult to maintain/debug
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
                        // Only find .gitignore files while also stripping it from the name
                        Match match = Regex.Match(list.Name, ignorePattern);
                        if(match.Success)
                        {
                            Data.Add(match.Groups[1].Value, list.DownloadUrl);
                        }
                    }
                }

                while(dirs.Count > 0)
                {
                    // Fetch contents from dir
                    //Console.Write("Fetching from dir: {0}", dirs.Peek().Name);
                    //Console.ReadLine();
                    string dirJSON = WebFetch.fetch(dirs.Pop().Url);
                    listings = Listing.FromJson(dirJSON);
                    foreach(Listing l in listings)
                    {
                        listingsQueue.Enqueue(l);
                    }
                }

            } while(listingsQueue.Count > 0);
        }
    }
}