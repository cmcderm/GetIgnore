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

            String cachePath;
            if(Environment.OSVersion.Platform == PlatformID.Unix)
            {
                cachePath = Environment.GetEnvironmentVariable("HOME") + "/.getignore.cache";
            }
            else if(Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                cachePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.getignore.cache";
            }
            else
            {
                cachePath = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "/.getignore.cache";
            }

            var output = app.Option("-o|--output <outputFile>", 
                       "Define the file to output to instead of .gitignore",
                       CommandOptionType.SingleValue
            );

            var ignoreFiles = app.Argument("ignoreFiles",
                                      "The .gitignore environments that you want.",
                                      true //Multiple Values
            );

            // Long option name (e.g. --verbose) MUST match the corresponding value in Enum Options
            var verbose = app.Option("-v|--verbose",
                                     "Be more talkative than normal.",
                                     CommandOptionType.NoValue
            );
            var preview = app.Option("-p|--preview",
                                     "Preview the .gitignore in stdout before saving. Adds a confirmation before saving.",
                                     CommandOptionType.NoValue
            );
            var force = app.Option("-f|--force",
                                   "Ignore prompts (yes to all)",
                                   CommandOptionType.NoValue
            );
            var append = app.Option("-a|--append",
                                    "Append .gitignore contents instead of overwriting existing.",
                                    CommandOptionType.NoValue
            );
            var nocache = app.Option("-c|--nocache",
                                     "Discard cache after running. Saves you a whole ~20.4KiB.",
                                     CommandOptionType.NoValue);
            var no = app.Option("-n|--no",
                                "Because I'm a lazy developer. Decline all prompts automagically.",
                                CommandOptionType.NoValue
            );
            
            app.OnExecute(() => {
                //Collect flags into one place
                Options flags = GetFlags(app.GetOptions());

                if(ignoreFiles.Values.Count == 0)
                {
                    Console.WriteLine("No arguments specified.");
                    Console.Write("Enter the environments you need (separated by spaces): ");
                    String[] envs = Console.ReadLine().Split(' ');
                    foreach(string s in envs)
                    {
                        ignoreFiles.Values.Add(s);
                    }
                }

                GHGetIgnore getter = new GHGetIgnore(cachePath, flags);
                string gitignore = getter.Get(ignoreFiles.Values);
                
                // Resolve File Name
                string outputFile = @".\.gitignore";
                if(output.HasValue())
                {
                    Console.WriteLine($"Writing .gitignore to {output.Value()}...");
                    outputFile = output.Value();
                }

                // If preview is selected, show and prompt the user to confirm
                bool saveFile = true;
                if(flags.HasFlag(Options.Preview))
                {
                    Console.WriteLine(gitignore);
                    if(!flags.HasFlag(Options.No))
                    {
                        saveFile = UserInputReader.GetConfirmation("Save file to " + outputFile + "?");
                    }
                    else
                    {
                        saveFile = false;
                    }
                }

                // Write File
                if(saveFile)
                {
                    try
                    {
                        DirectoryInfo dir = new DirectoryInfo(outputFile);
                        if(flags.HasFlag(Options.Append))
                        {
                            File.AppendAllText(dir.FullName, gitignore);
                        }
                        else
                        {
                            File.WriteAllText(dir.FullName, gitignore);
                        }
                    }
                    catch ( Exception ex )
                    {
                        Console.Error.WriteLine("Unable to write to file: {0}", ex.Message);
                    }
                    
                }
                
                return 0;
            });

            app.Command("search", (command) => {
                var ignoreSearch = command.Argument("ignoreSearch",
                                      "The .gitignore environments that you want to search for.");

                command.OnExecute(() => {
                    Options flags = GetFlags(command.GetOptions());

                    GHGetIgnore get = new GHGetIgnore(cachePath, flags);
                    ICollection<string> searchResults = get.Search(ignoreSearch.Value);

                    foreach(String result in searchResults)
                    {
                        Console.WriteLine(result);
                    }
                    Console.Write($"{Environment.NewLine}{searchResults.Count} search results found");

                    return 0;
                });
            });

            app.Command("killcache", (command) => {
                var kcForce = command.Option("-f|--force",
                                         "Skips prompt before deleting.",
                                         CommandOptionType.NoValue
                );

                command.OnExecute(() => {
                    Options flags = GetFlags(command.GetOptions());
                    if(!flags.HasFlag(Options.Force))
                    {
                        if(UserInputReader.GetConfirmation("You are about to delete your cache, Continue?", false)){return 0;}
                    }
                    if(!File.Exists(cachePath))
                    {
                        Console.WriteLine($"Cache not found at {cachePath}.");
                        return 0;
                    }
                    else
                    {
                        File.Delete(cachePath);
                    }
                    return 0;
                });
            });

            app.Command("delete", (command) => {
                command.Description = "Deletes .gitignore in current folder only.";

                var kcForce = command.Option("-f|--force",
                                         "Skips prompt before deleting.",
                                         CommandOptionType.NoValue
                );
                
                command.OnExecute(() => {
                    Options flags = GetFlags(command.GetOptions());
                    bool delete = true;
                    
                    var info = new DirectoryInfo(@".\.gitignore");

                    if(!flags.HasFlag(Options.Force))
                    {
                        delete = UserInputReader.GetConfirmation("Really delete .gitignore at {info.FullName}?", false);
                    }
                    if(delete)
                    {
                        File.Delete(info.FullName);
                    }
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
        // Downside is that the name in Enum Options need to match the long name of the flag
        // Using the CommandOptions given, return Options flags for easier use
        private static Options GetFlags(IEnumerable<CommandOption> optionArgs)
        {
            Options flags = Options.None;
            
            // For every potential flag
            foreach(CommandOption opt in optionArgs)
            {
                // Uncomment the following to debug flags
                //Console.WriteLine($"{opt.LongName}: {opt.HasValue()}");

                // If the flag is used (Has Value doesn't refer to whether or not an argument was given)
                if(opt.HasValue())
                {
                    // Check against all potential flags (as given in Options.cs)
                    foreach(Options f in Enum.GetValues(typeof(Options))){
                        // Compare names -- If there's a better way to match an option to the enum value I'd love to know
                        if(opt.LongName.ToLower() == f.ToString().ToLower())
                        {
                            if(!flags.HasFlag(f)){
                                //Console.Write($"\t Option {opt.LongName} applied");
                                flags |= f;
                                break;
                            }
                        }
                    }
                }
                //Console.WriteLine();
            }

            return flags;
        }
    }
}
