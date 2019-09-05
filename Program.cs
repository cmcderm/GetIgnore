using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.CommandLineUtils;

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

            Console.WriteLine(string.Format("Version {0}", _config.GetSection("Config")["Version"]));

            var app = new CommandLineApplication();
            app.Name = "GetIgnore";
            app.Description = @".NET Core app for quickly downloading .gitignore files.
            Uses gitignore.io as an API";

            app.HelpOption("-?|-h|--help");

            app.VersionOption("-v|--version", () => {
                return string.Format("Version {0}", _config.GetSection("Config")["Version"]);
            });
        }
    }
}
