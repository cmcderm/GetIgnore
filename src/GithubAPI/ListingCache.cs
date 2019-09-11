using System;
using System.Collections.Generic;

using QuickType;

namespace GetIgnore.Github
{
    public class ListingCache
    {
        public DateTimeOffset TimeStamp{get;}
        public Dictionary<String, Uri> Data{get;}

        public ListingCache(){
            Data = new Dictionary<String, Uri>();
        }

        public void Update(Listing[] newListings)
        {
            foreach(Listing list in newListings)
            {
                Data.Add(list.Name, list.DownloadUrl);
            }
        }
    }
}