using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using VismaTask1.Models;
using VismaTask1.Services;

namespace VismaTask1.Commands
{
    public class RegisterCommand : Command
    {
        public RegisterCommand(IShortageService service, ILogger<RegisterCommand> logger, string username, bool isAdmin)
            : base("register", "Register a new application")
        {
            var titleOpt = new Option<string>("--title") { Description = "Application title", IsRequired = true };
            var roomOpt = new Option<Room>("--room") { Description = "Room", IsRequired = true };
            var categoryOpt = new Option<Category>("--category") { Description = "Category", IsRequired = true };
            var priorityOpt = CreatePriorityOption();

            AddOption(titleOpt);
            AddOption(roomOpt);
            AddOption(categoryOpt);
            AddOption(priorityOpt);

            this.Handler = CommandHandler.Create<string, Room, Category, int>((title, room, category, priority) =>
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
                    logger.LogWarning(ex, "Duplicate or low-priority application.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error occurred during registration.");
                    logger.LogError(ex, "Unhandled error in RegisterCommand.");
                }
            });
        }

        private static Option<int> CreatePriorityOption()
        {
            var priorityOption = new Option<int>("--priority")
            {
                Description = "Priority (1–10)",
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
