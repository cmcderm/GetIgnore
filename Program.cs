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
                       CommandOptionType.SingleValue
            );

            var ignoreFiles = app.Argument("ignoreFiles",
                                      "Languages or environments to fetch .gitignore for",
                                      true //Multiple Values
            );

            // Long option name (e.g. --verbose) MUST match the corresponding value in Enum Options
            var verbose = app.Option("-v|--verbose",
                                     "Be more talkative than normal.",
                                     CommandOptionType.NoValue
            );
            var preview = app.Option("-p|--preview",
                                     "Preview the .gitignore in stdout before saving.",
                                     CommandOptionType.NoValue
            );
            var force = app.Option("-f|--force",
                                   "Ignore prompts (yes to all)",
                                   CommandOptionType.NoValue
            );

            app.OnExecute(() => {
                //Collect flags into one place

                Options flags = GetFlags(app.GetOptions());

                GHGetIgnore getter = new GHGetIgnore(flags);
                string gitignore = getter.Get(ignoreFiles.Values);
                if(flags.HasFlag(Options.Preview)){
                    Console.WriteLine(gitignore);
                }
                return 0;
            });

            app.Command("search", (command) => {
                command.OnExecute(() => {
                    Console.Error.WriteLine("Error: Search is not yet implemented");

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

        // Wrote this in a way that it doesn't need to be changed whenever an option is added
        // Using the CommandOptions given, return Options flags for easier use
        private static Options GetFlags(IEnumerable<CommandOption> optionArgs)
        {
            Options flags = Options.None;
            
            // For every potential flag
            foreach(CommandOption opt in optionArgs)
            {
                // If the flag is used (Has Value doesn't refer to whether or not an argument was given)
                if(opt.HasValue())
                {
                    foreach(Options f in Enum.GetValues(typeof(Options))){
                        if(opt.LongName.ToLower() == f.ToString().ToLower())
                        {
                            if(!flags.HasFlag(f)){
                                flags &= f;
                            }
                        }
                    }
                }
            }

            return flags;
        }
    }
}
