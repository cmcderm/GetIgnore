using System;
using System.Collections.Generic;
using GetIgnore.Github;

namespace GetIgnore
{
    public class GHGetIgnore
    {
        GithubAPI _gh;

        public GHGetIgnore(Options flags = Options.None){
            _gh = new GithubAPI();
        }

        ///<Summary>
        /// Gets the gitignore for each of the specified environments. Skips invalid entries.
        ///</Summary>
        public string Get(IEnumerable<String> ignoreFiles)
        {
            string gitignore = "";
            foreach(string ignore in ignoreFiles)
            {
                try
                {
                    gitignore += _gh.download(ignore);
                }
                catch( Exception ex ) {
                    Console.WriteLine("Error: Could not download the specified .gitignore: {0}", ignore);
                    Console.WriteLine(ex.Message);
                }
            }
            return gitignore;
        }

        public string Search(string[] ignoreFiles)
        {
            throw new NotImplementedException();
        }
    }
}