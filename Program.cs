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
                    services.AddSingleton<IShortageRepository>(sp => new JsonShortageRepository("shortages.json"));
                    services.AddSingleton<IShortageService, ShortageService>();
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

            var service = host.Services.GetRequiredService<IShortageService>();

            // Setting up the root command
            var root = new RootCommand("Visma Resource Shortage CLI");
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            #region register Command

            var registerCmd = new Command("register", "Register a new application")
            {
                new Option<string>("--title")
                {
                    Description = "Application title",
                    IsRequired  = true
                },
                new Option<Room>("--room")
                {
                    Description = "Room",
                    IsRequired  = true
                },
                new Option<Category>("--category")
                {
                    Description = "Category",
                    IsRequired  = true
                },
                CreatePriorityOption()
            };

            registerCmd.Handler = CommandHandler.Create<string, Room, Category, int>((title, room, category, priority) =>
            {
                try
                {
                    var s = new Shortage
                    {
                        Title = title,
                        Name = username,
                        Room = room,
                        Category = category,
                        Priority = priority,
                        CreatedOn = DateTime.UtcNow
                    };
                    service.Register(s, username);
                    Console.WriteLine("OK: application registered");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Attention: {ex.Message}");
                    Log.Error(ex, "When trying to register an application Title={Title}, Room={Room}", title, room);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An unexpected error occurred while registering.");
                    Log.Error(ex, "Unexpected error in register Cmd.Handler");
                }
            });
            root.AddCommand(registerCmd);
            #endregion

            #region ---list Command
            // Options --from and --to moved to correct positional parseArgument
            var fromOpt = new Option<DateTime?>(
                aliases: new[] { "--from" },
                parseArgument: result =>
                {
                    var token = result.Tokens.SingleOrDefault()?.Value;
                    try
                    {
                        if (DateTime.TryParseExact(token ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                        {
                            return d;
                        }
                        else
                        {
                            throw new ArgumentException($"Incorrect date format: «{token}». Expected yyyy-MM-dd.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error parsing parameter --from: {RawValue}", token);
                        Console.WriteLine("Error: Date format for --from parameter must be yyyy-MM-dd.");
                        return null;
                    }
                })
            {
                Description = "Date from (yyyy-MM-dd)"
            };



            var toOpt = new Option<DateTime?>(
                aliases: new[] { "--to" },
                parseArgument: result =>
                {
                    var token = result.Tokens.SingleOrDefault()?.Value;
                    try
                    {
                        if (DateTime.TryParseExact(token ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                        {
                            return d;
                        }
                        else
                        {
                            throw new ArgumentException($"Incorrect date format: «{token}». Expected yyyy-MM-dd.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error parsing parameter --to: {RawValue}", token);
                        Console.WriteLine("Error: Date format for --to option must be yyyy-MM-dd.");
                        return null;
                    }
                })
            {
                Description = "Date to (yyyy-MM-dd)"
            };

            var listCmd = new Command("list", "Show applications")
            {
                new Option<string?>("--title")
                {
                    Description = "Filter by title"
                },
                fromOpt,
                toOpt,
                new Option<Category?>("--category")
                {
                    Description = "Filter by category"
                },
                new Option<Room?>("--room")
                {
                    Description = "Filter by room"
                }
            };
            listCmd.Handler = CommandHandler.Create<string?, DateTime?, DateTime?, Category?, Room?>(
                (title, from, to, category, room) =>
                {
                    var all = service.GetAll(username, isAdmin);
                    var list = service.Filter(all, title, from, to, category, room);
                    if (!list.Any())
                    {
                        Console.WriteLine("Nothing found.");
                        return;
                    }
                    foreach (var x in list)
                    {
                        Console.WriteLine(
                            $"{x.Title} | {x.Room} | {x.Category} | Pri:{x.Priority} | " +
                            $"By:{x.Name} | {x.CreatedOn:yyyy-MM-dd}");
                    }
                });
            root.AddCommand(listCmd);
            #endregion

            #region delete Command
            var deleteCmd = new Command("delete", "Delete request")
            {
                new Option<string>("--title")
                {
                    Description = "Application title",
                    IsRequired  = true
                },
                new Option<Room>("--room")
                {
                    Description = "Room",
                    IsRequired  = true
                }
            };
            deleteCmd.Handler = CommandHandler.Create<string, Room>((title, room) =>
            {
                service.Delete(title, room, username, isAdmin);
                Console.WriteLine("OK: request deleted");
            });
            root.AddCommand(deleteCmd);
            #endregion


            // --- exit
            var exitCmd = new Command("exit", "Exit the application");
            exitCmd.Handler = CommandHandler.Create(() => Environment.Exit(0));
            root.AddCommand(exitCmd);

            // If there are arguments, execute once and exit
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

        private static Option<int> CreatePriorityOption()
        {
            var priorityOption = new Option<int>("--priority")
            {
                Description = "Priority (1-10)",
                IsRequired = true
            };

            priorityOption.AddValidator(result =>
            {
                var value = result.GetValueOrDefault<int>();
                if (value < 1 || value > 10)
                {
                    result.ErrorMessage = "The priority must be in the range from 1 to 10.";
                }
            });

            return priorityOption;
        }
    }
}

