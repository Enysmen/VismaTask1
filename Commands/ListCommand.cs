using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using VismaTask1.Models;
using VismaTask1.Services;

namespace VismaTask1.Commands
{
    public class ListCommand : BaseCommand
    {
        public ListCommand(IShortageService service,ILogger<ListCommand> logger,string username,bool isAdmin)
        :base("list", "Show applications", service, logger, username, isAdmin)
        {
            var titleOpt = new Option<string?>("--title") 
            { 
                Description = "Filter by title" 
            };
            var fromOpt = new Option<DateTime?>("--from", parseArgument: ParseDate) 
            { 
                Description = "Date from (yyyy-MM-dd)" 
            };
            var toOpt = new Option<DateTime?>("--to", parseArgument: ParseDate) 
            { 
                Description = "Date to (yyyy-MM-dd)" 
            };
            var categoryOpt = new Option<Category?>("--category") 
            { 
                Description = "Filter by category" 
            };
            var roomOpt = new Option<Room?>("--room") 
            { 
                Description = "Filter by room" 
            };

            AddOption(titleOpt);
            AddOption(fromOpt);
            AddOption(toOpt);
            AddOption(categoryOpt);
            AddOption(roomOpt);

            this.Handler = CommandHandler.Create<string?, DateTime?, DateTime?, Category?, Room?>(Execute);
        }

        private void Execute(string? title, DateTime? from, DateTime? to, Category? category, Room? room)
        {
            try
            {
                var all = Service.GetAll(Username, IsAdmin);
                var filtered = Service.Filter(all, title, from, to, category, room);

                if (filtered.Count == 0)
                {
                    Console.WriteLine("Nothing found.");
                    return;
                }

                foreach (var x in filtered)
                {
                    Console.WriteLine($"{x.Title} | {x.Room} | {x.Category} | Pri:{x.Priority} | By:{x.Name} | {x.CreatedOn:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error in list");
                Console.WriteLine("An error occurred while listing applications.");
            }
        }

        private static DateTime? ParseDate(ArgumentResult result)
        {
            var token = result.Tokens.SingleOrDefault()?.Value;
            if (DateTime.TryParseExact(token, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                return d;
            }
           
            result.ErrorMessage = $"Invalid date format '{token}', expected yyyy-MM-dd";
            return null;
        }
    }
}
