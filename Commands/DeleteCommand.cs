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
    public class DeleteCommand : BaseCommand
    {
        public DeleteCommand(IShortageService service,ILogger<DeleteCommand> logger,string username,bool isAdmin)
        :base("delete", "Delete a request", service, logger, username, isAdmin)
        {
            var titleOpt = new Option<string>("--title") 
            { 
                Description = "Application title", IsRequired = true 
            };
            var roomOpt = new Option<Room>("--room") 
            { 
                Description = "Room", IsRequired = true 
            };

            AddOption(titleOpt);
            AddOption(roomOpt);

            this.Handler = CommandHandler.Create<string, Room>(Execute);
        }

        private void Execute(string title, Room room)
        {
            try
            {
                Service.Delete(title, room, Username, IsAdmin);
                Console.WriteLine("OK: request deleted");
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogWarning(ex, "Delete validation failed");
                Console.WriteLine($"Attention: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogWarning(ex, "Unauthorized delete attempt");
                Console.WriteLine("Access denied: You can only delete your own entries.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error in delete");
                Console.WriteLine("An unexpected error occurred while deleting.");
            }
        }
    }
}
