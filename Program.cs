using System;
using System.CommandLine; 
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using VismaTask1.Commands;
using Serilog;
using VismaTask1.Models;
using VismaTask1.Repositories;
using VismaTask1.Services;


namespace VismaTask1
{
    class Program
    {
        static int Main(string[] args)
        {

            var host = Host.CreateDefaultBuilder()
                .UseSerilog((context, services, configuration) =>
                {
                    configuration.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IShortageService, ShortageService>();
                    services.AddSingleton<IShortageRepository>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<JsonShortageRepository>>();
                        return new JsonShortageRepository("shortages.json", logger);
                    });
                })
                .Build();

            Console.Write("Enter your name: ");
            var username = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Username cannot be empty.");
                return 1;
            }
            bool isAdmin = username.Equals("admin", StringComparison.OrdinalIgnoreCase);

            var root = new RootCommand("Visma Resource Shortage CLI");
            var service = host.Services.GetRequiredService<IShortageService>();

            #region register Command
            root.AddCommand(new RegisterCommand(
                service,
                host.Services.GetRequiredService<ILogger<RegisterCommand>>(),
                username,
                isAdmin));
            #endregion 

            #region list Command
            root.AddCommand(new ListCommand(
                service,
                host.Services.GetRequiredService<ILogger<ListCommand>>(),
                username,
                isAdmin));
            #endregion

            #region delete Command
            root.AddCommand(new DeleteCommand(
                service,
                host.Services.GetRequiredService<ILogger<DeleteCommand>>(),
                username,
                isAdmin));
            #endregion

            #region exit Command
            var exitCmd = new Command("exit", "Exit the application");
            exitCmd.Handler = CommandHandler.Create(() => Environment.Exit(0));
            root.AddCommand(exitCmd);
            #endregion

            // CLI entry point
            if (args.Length > 0)
            {
                return root.InvokeAsync(args).Result;
            }
                
            // Otherwise - interactive REPL
            Console.WriteLine("Visma CLI: Type --help for a list of commands, exit to exit.");
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _ = root.Invoke(parts);
            }
        }

    }
}

