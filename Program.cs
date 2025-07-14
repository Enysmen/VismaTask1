using System.Globalization;

using VismaTask1.Models;
using VismaTask1.Repositories;
using VismaTask1.Services;

namespace VismaTask1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Ввод имени и определение роли
            Console.Write("Введите ваше имя: ");
            var username = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Имя пользователя не может быть пустым.");
                return;
            }
            bool isAdmin = username.Equals("admin", StringComparison.OrdinalIgnoreCase);

            // 2. Инициализация репозитория и сервиса
            var repository = new JsonShortageRepository("shortages.json");
            var service = new ShortageService(repository);

            // 3. Разбор аргументов
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            var command = args[0].ToLowerInvariant();
            var options = args.Skip(1).ToArray();

            try
            {
                switch (command)
                {
                    case "register":
                        HandleRegister(options, service, username);
                        break;

                    case "list":
                        HandleList(options, service, username, isAdmin);
                        break;

                    case "delete":
                        HandleDelete(options, service, username, isAdmin);
                        break;

                    case "help":
                    default:
                        PrintUsage();
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Доступ запрещён: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Непредвиденная ошибка: {ex.Message}");
            }
        }

        static void HandleRegister(string[] opts, ShortageService service, string user)
        {
            // Ожидаем: --title <текст> --room <room> --category <cat> --priority <1-10>
            var p = new OptionParser(opts);

            var title = p.GetValue("--title")
                ?? throw new InvalidOperationException("Не задан параметр --title.");
            var room = Enum.Parse<Room>(
                p.GetValue("--room") ?? throw new InvalidOperationException("Не задан параметр --room."),
                true);
            var category = Enum.Parse<Category>(
                p.GetValue("--category") ?? throw new InvalidOperationException("Не задан параметр --category."),
                true);
            var priority = int.Parse(p.GetValue("--priority")
                ?? throw new InvalidOperationException("Не задан параметр --priority."));

            var shortage = new Shortage
            {
                Title = title,
                Name = user,
                Room = room,
                Category = category,
                Priority = priority,
                CreatedOn = DateTime.UtcNow
            };

            service.Register(shortage, user);
            Console.WriteLine("Заявка успешно зарегистрирована.");
        }

        static void HandleList(string[] opts, ShortageService service, string user, bool isAdmin)
        {
            // Параметры фильтра: --title, --from, --to, --category, --room
            var p = new OptionParser(opts);
            DateTime? from = p.TryGetDate("--from");
            DateTime? to = p.TryGetDate("--to");
            string? title = p.GetValue("--title");
            var cat = p.TryGetEnum<Category>("--category");
            var room = p.TryGetEnum<Room>("--room");

            var all = service.GetAll(user, isAdmin);
            var filtered = service.Filter(all, title, from, to, cat, room);

            if (!filtered.Any())
            {
                Console.WriteLine("Ничего не найдено.");
                return;
            }

            foreach (var s in filtered)
            {
                Console.WriteLine(
                    $"{s.Title} | {s.Room} | {s.Category} | Приоритет: {s.Priority} | " +
                    $"Создал: {s.Name} | Дата: {s.CreatedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            }
        }

        static void HandleDelete(string[] opts, ShortageService service, string user, bool isAdmin)
        {
            // Ожидаем: --title <текст> --room <room>
            var p = new OptionParser(opts);
            var title = p.GetValue("--title")
                ?? throw new InvalidOperationException("Не задан параметр --title.");
            var room = Enum.Parse<Room>(
                p.GetValue("--room") ?? throw new InvalidOperationException("Не задан параметр --room."),
                true);

            service.Delete(title, room, user, isAdmin);
            Console.WriteLine("Заявка удалена.");
        }

        static void PrintUsage()
        {
            Console.WriteLine("Использование:");
            Console.WriteLine("  register --title <text> --room <room> --category <cat> --priority <1-10>");
            Console.WriteLine("  list [--title <text>] [--from <yyyy-MM-dd>] [--to <yyyy-MM-dd>]");
            Console.WriteLine("       [--category <cat>] [--room <room>]");
            Console.WriteLine("  delete --title <text> --room <room>");
            Console.WriteLine("  help");
            Console.WriteLine();
            Console.WriteLine("Room: MeetingRoom, Kitchen, Bathroom");
            Console.WriteLine("Category: Electronics, Food, Other");
        }
    }

    // Простая утилита для разбора пар --option value
    class OptionParser
    {
        private readonly string[] _args;
        public OptionParser(string[] args) => _args = args;

        public string? GetValue(string name)
        {
            for (int i = 0; i < _args.Length; i += 2)
            {
                if (_args[i].Equals(name, StringComparison.OrdinalIgnoreCase)
                    && i + 1 < _args.Length)
                    return _args[i + 1];
            }
            return null;
        }

        public DateTime? TryGetDate(string name)
        {
            var v = GetValue(name);
            return DateTime.TryParseExact(v ?? "",
                "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d) ? d : null;
        }

        public T? TryGetEnum<T>(string name) where T : struct, Enum
        {
            var v = GetValue(name);
            return Enum.TryParse<T>(v, true, out var e) ? e : (T?)null;
        }
    }
}

