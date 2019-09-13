using System;
using System.Collections.Generic;
using System.Text;
using GetIgnore.Github;

namespace GetIgnore
{
    public class GHGetIgnore
    {
        GithubAPI _gh;

        Options flags;

        public GHGetIgnore(String CachePath, Options Flags = Options.None){
            flags = Flags;
            _gh = new GithubAPI(CachePath, flags);
        }

        ///<Summary>
        /// Gets the gitignore for each of the specified environments. Skips invalid entries.
        ///</Summary>
        public string Get(IEnumerable<String> ignoreFiles)
        {
            StringBuilder gitignore = new StringBuilder();
            foreach(string ignore in ignoreFiles)
            {
                try
                {
                    gitignore.Append(_gh.download(ignore) + Environment.NewLine);
                }
                catch(System.IO.FileNotFoundException)
                {
                    Console.WriteLine("Error: The specified .gitignore ({0}) could not be found", ignore);
                }
                catch(Exception ex) 
                {
                    Console.WriteLine("Error: Could not download the specified .gitignore: {0}", ignore);
                    Console.WriteLine(ex.Message);
                    throw new Exception("Error downloading .gitignore", ex);
                }
            }
            return gitignore.ToString();
        }

        public ICollection<String> Search(String search)
        {
            //ICollection<String> searchResults;
            try
            {
                return _gh.search(search);
            }
            catch( Exception ex )
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                throw new Exception($"Could not complete search: {search}", ex);
            }
        }
    }
}