using System;
using System.CommandLine; 
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Globalization;

using Microsoft.Extensions.Logging;

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
            // Ввод имени и определение роли
            Console.Write("Введите ваше имя: ");
            var username = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Имя пользователя не может быть пустым.");
                return 1;
            }
            bool isAdmin = username.Equals("admin", StringComparison.OrdinalIgnoreCase);

            // Репозиторий и сервис
            var repo = new JsonShortageRepository("shortages.json");
            var service = new ShortageService(repo);
            Log.Logger = new LoggerConfiguration().WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day).CreateLogger();

            // Настройка корневой команды
            var root = new RootCommand("Visma Resource Shortage CLI");
            var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(dispose: true));
            var logger = loggerFactory.CreateLogger<Program>();

            // --- register
            var registerCmd = new Command("register", "Зарегистрировать новую заявку")
            {
                new Option<string>("--title")
                {
                    Description = "Заголовок заявки",
                    IsRequired  = true
                },
                new Option<Room>("--room")
                {
                    Description = "Комната",
                    IsRequired  = true
                },
                new Option<Category>("--category")
                {
                    Description = "Категория",
                    IsRequired  = true
                },
                new Option<int>("--priority")
                {
                    Description = "Приоритет (1-10)",
                    IsRequired  = true
                }
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
                    Console.WriteLine("OK: заявка зарегистрирована");
                }
                catch (InvalidOperationException ex)
                {
                    // Пользователю — понятный текст
                    Console.WriteLine($"Внимание: {ex.Message}");
                    // Логируем полный стек
                    Log.Error(ex, "При попытке зарегистрировать заявку Title={Title}, Room={Room}", title, room);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла непредвиденная ошибка при регистрации.");
                    Log.Error(ex, "Непредвиденная ошибка в registerCmd.Handler");
                }
            });
            root.AddCommand(registerCmd);

            // --- list
            // Опции --from и --to вынесены для корректного позиционного parseArgument
            var fromOpt = new Option<DateTime?>(
                aliases: new[] { "--from" },
                parseArgument: result =>
                {
                    var token = result.Tokens.SingleOrDefault()?.Value;
                    try
                    {
                        if (DateTime.TryParseExact(token ?? string.Empty, "yyyy-MM-dd",CultureInfo.InvariantCulture, DateTimeStyles.None,out var d))
                        {
                            return d;
                        }
                        else
                        {
                            throw new ArgumentException($"Некорректный формат даты: «{token}». Ожидается yyyy-MM-dd.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Логируем полную трассировку
                        Log.Error(ex, "Ошибка при разборе параметра --from: {RawValue}", token);

                        // Пользователю — короткое сообщение
                        Console.WriteLine("Ошибка: формат даты для параметра --from должен быть yyyy-MM-dd.");

                        // Возвращаем null, чтобы фильтра не применялось
                        return null;
                    }
                })
                 {
                    Description = "Дата от (yyyy-MM-dd)"
                 };



            var toOpt = new Option<DateTime?>(
                aliases: new[] { "--to" },
                parseArgument: result =>
                {
                    var token = result.Tokens.SingleOrDefault()?.Value;
                    try
                    {
                        if (DateTime.TryParseExact(token ?? string.Empty, "yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None, out var d))
                        {
                            return d;
                        }
                        else
                        {
                            throw new ArgumentException($"Некорректный формат даты: «{token}». Ожидается yyyy-MM-dd.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Логируем подробности в файл
                        Log.Error(ex, "Ошибка при разборе параметра --to: {RawValue}", token);

                        // Выводим пользователю понятное сообщение
                        Console.WriteLine("Ошибка: формат даты для параметра --to должен быть yyyy-MM-dd.");

                        // Возвращаем null, чтобы фильтр не применился
                        return null;
                    }
                })
                 {
                    Description = "Дата до (yyyy-MM-dd)"
                 };

            var listCmd = new Command("list", "Показать заявки")
            {
                new Option<string?>("--title")
                {
                    Description = "Фильтр по заголовку"
                },
                fromOpt,
                toOpt,
                new Option<Category?>("--category")
                {
                    Description = "Фильтр по категории"
                },
                new Option<Room?>("--room")
                {
                    Description = "Фильтр по комнате"
                }
            };
            listCmd.Handler = CommandHandler.Create<string?, DateTime?, DateTime?, Category?, Room?>(
                (title, from, to, category, room) =>
                {
                    var all = service.GetAll(username, isAdmin);
                    var list = service.Filter(all, title, from, to, category, room);
                    if (!list.Any())
                    {
                        Console.WriteLine("Ничего не найдено.");
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

            // --- delete
            var deleteCmd = new Command("delete", "Удалить заявку")
            {
                new Option<string>("--title")
                {
                    Description = "Заголовок заявки",
                    IsRequired  = true
                },
                new Option<Room>("--room")
                {
                    Description = "Комната",
                    IsRequired  = true
                }
            };
            deleteCmd.Handler = CommandHandler.Create<string, Room>((title, room) =>
            {
                service.Delete(title, room, username, isAdmin);
                Console.WriteLine("OK: заявка удалена");
            });
            root.AddCommand(deleteCmd);

            // --- exit
            var exitCmd = new Command("exit", "Выход из приложения");
            exitCmd.Handler = CommandHandler.Create(() => Environment.Exit(0));
            root.AddCommand(exitCmd);

            // Если есть аргументы — выполнить один раз и выйти
            if (args.Length > 0)
                return root.InvokeAsync(args).Result;

            // Иначе — интерактивный REPL
            Console.WriteLine("Visma CLI: введите --help для списка команд, exit — для выхода.");
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                _ = root.Invoke(parts);
            }
        }
    }
}

