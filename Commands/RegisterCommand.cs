using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using VismaTask1.Models;
using VismaTask1.Services;

namespace VismaTask1.Commands
{
    public class RegisterCommand : BaseCommand
    {
        public RegisterCommand(IShortageService service, ILogger<RegisterCommand> logger, string username, bool isAdmin)
        : base("register", "Register a new application", service, logger, username, isAdmin)
        {
            foreach (var opt in FactoryOptionsCommand.CreateRegisterOptions())
            {
                AddOption(opt);
            }
            this.Handler = CommandHandler.Create<string, Room, Category, int>(Execute);
        }

        private void Execute(string title, Room room, Category category, int priority)
        {
            try
            {
                var s = new Shortage
                {
                    Title = title,
                    Name = Username,
                    Room = room,
                    Category = category,
                    Priority = priority,
                    CreatedOn = DateTime.UtcNow
                };
                Service.Register(s, Username);
                Console.WriteLine("OK: application registered");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Attention: {ex.Message}");
                Logger.LogWarning(ex, "Duplicate or low-priority application.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error occurred during registration.");
                Logger.LogError(ex, "Unhandled error in RegisterCommand.");
            }
        }
 
    }
}
