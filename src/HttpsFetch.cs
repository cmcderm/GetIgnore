using System;
using System.IO;
using System.Net;

namespace GetIgnore
{
    public class WebFetch
    {
        public static string fetchRequest(string url)
        {
            string output = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if(response.StatusCode != HttpStatusCode.OK)
                    {
                        Console.Error.Write("HTTP Status Code {0} while contacting: {0}", response.StatusCode.ToString());
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                                throw new FileNotFoundException($"File not found at {url}");
                            default:
                                Console.Error.Write("Ignoring and moving on...");
                                break;
                        }
                    }
                    
                    using(Stream stream = response.GetResponseStream())
                    using(StreamReader reader = new StreamReader(stream))
                    {
                        output = reader.ReadToEnd();
                    }
                }
            }
            catch(FileNotFoundException ex)
            {
                throw new FileNotFoundException("File not found!", ex);
            }

            return output;
        }

        // These are easy to use but offer no good way to handle HTTP status codes
        public static string fetch(string url)
        {
            WebClient client = new WebClient();
            client.Headers.Set("User-Agent", "GetIgnore");
            string response = client.DownloadString(url);

            return response;
        }

        public static string fetch(Uri uri){
            WebClient client = new WebClient();
            client.Headers.Set("User-Agent", "GetIgnore");
            string response = client.DownloadString(uri);

            return response;
        }
    }
}