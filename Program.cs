using System;
using System.Globalization;
using System.CommandLine; 
using System.CommandLine.Invocation;
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

            // Настройка корневой команды
            var root = new RootCommand("Visma Resource Shortage CLI");

            // --- register
            var cmdReg = new Command("register", "Зарегистрировать новую заявку")
            {
                new Option<string>("--title",    description: "Заголовок заявки")    { IsRequired = true },
                new Option<Room>("--room",       description: "Комната")              { IsRequired = true },
                new Option<Category>("--category",description: "Категория")           { IsRequired = true },
                new Option<int>("--priority",    description: "Приоритет (1-10)")     { IsRequired = true }
            };
            cmdReg.Handler = CommandHandler.Create<string, Room, Category, int>((title, room, category, priority) =>
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
            });
            root.AddCommand(cmdReg);

            // --- list
            var cmdList = new Command("list", "Показать заявки")
            {
                new Option<string?>("--title",   "Фильтр по заголовку"),
                new Option<DateTime?>("--from",  "Дата от (yyyy-MM-dd)", parseArgument: result =>
                    DateTime.TryParseExact(result.Tokens[0].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var d) ? d : throw new ArgumentException("Неверный формат --from")),
                new Option<DateTime?>("--to",    "Дата до (yyyy-MM-dd)", parseArgument: result =>
                    DateTime.TryParseExact(result.Tokens[0].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var d) ? d : throw new ArgumentException("Неверный формат --to")),
                new Option<Category?>("--category","Фильтр по категории"),
                new Option<Room?>("--room",      "Фильтр по комнате")
            };
            cmdList.Handler = CommandHandler.Create<string?, DateTime?, DateTime?, Category?, Room?>(
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
                        Console.WriteLine($"{x.Title} | {x.Room} | {x.Category} | Pri:{x.Priority} | By:{x.Name} | {x.CreatedOn:yyyy-MM-dd}");
                    }
                });
            root.AddCommand(cmdList);

            // --- delete
            var cmdDel = new Command("delete", "Удалить заявку")
            {
                new Option<string>("--title", "Заголовок заявки") { IsRequired = true },
                new Option<Room>("--room",     "Комната")          { IsRequired = true }
            };
            cmdDel.Handler = CommandHandler.Create<string, Room>((title, room) =>
            {
                service.Delete(title, room, username, isAdmin);
                Console.WriteLine("OK: заявка удалена");
            });
            root.AddCommand(cmdDel);

            // --- exit
            var cmdExit = new Command("exit", "Выход из приложения");
            cmdExit.Handler = CommandHandler.Create(() => Environment.Exit(0));
            root.AddCommand(cmdExit);

            // Если есть аргументы — выполнить один раз и выйти
            if (args.Length > 0)
                return root.InvokeAsync(args).Result;

            // Иначе — интерактивный REPL
            Console.WriteLine("Visma CLI: введите help для списка команд, exit — для выхода.");
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

