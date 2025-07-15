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
            foreach (var opt in FactoryOptionsCommand.CreateListOptions())
            {
                AddOption(opt);
            }
              
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
    }
}
