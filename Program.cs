using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.CommandLineUtils;

// Author: Connor McDermott

namespace GetIgnore
{

    
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot _config = builder.Build();

            var app = new CommandLineApplication();
            app.Name = "GetIgnore";
            app.Description = @".NET Core app for quickly downloading .gitignore files.
            Fetches .gitignore files from github.com/github/gitignore
            Writes to .gitignore (./.gitignore) in the current directory unless otherwise specified.";

            app.HelpOption("-?|-h|--help");

            app.VersionOption("-V|--version", () => {
                return string.Format("Version {0}", _config.GetSection("Config")["Version"]);
            });

            var output = app.Option("-o|--output <outputFile>", 
                       "Define the file to output to instead of .gitignore",
                       CommandOptionType.SingleValue);

            var ignoreFiles = app.Argument("ignoreFiles",
                                      "Languages or environments to fetch .gitignore for",
                                      true //Multiple Values
            );

            var verbose = app.Option("-v|--verbose",
                                     "Be more talkative than normal.",
                                     CommandOptionType.NoValue);
            var preview = app.Option("-p|--preview",
                                     "Preview the .gitignore in stdout before saving.",
                                     CommandOptionType.NoValue);

            app.OnExecute(() => {
                //Collect flags into one place

                Options option = Options.None;
                if(verbose.HasValue()){
                    option &= Options.Verbose;
                }
                if(preview.HasValue()){
                    option &= Options.Preview;
                }
                Console.WriteLine("Beep Borp");
                GHGetIgnore getter = new GHGetIgnore(option);
                //string gitignore = getter.Get(ignoreFiles.Values);

                return 0;
            });

            app.Command("search", (command) => {
                Console.WriteLine("Zip Zorp");

                command.OnExecute(() => {
                    return 0;
                });
            });

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
            }
            
        }
    }
}
