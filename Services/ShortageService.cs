using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using VismaTask1.Models;
using VismaTask1.Repositories;

namespace VismaTask1.Services
{
    public class ShortageService : IShortageService
    {
        private readonly IShortageRepository _repository;
        private readonly ILogger<ShortageService> _logger;

        public ShortageService(IShortageRepository repository,ILogger<ShortageService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public IReadOnlyCollection<Shortage> GetAll(string currentUser, bool isAdmin)
        {
            try
            {
                var all = _repository.LoadAll();
                return isAdmin ? all : all.Where(s => s.Name == currentUser).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting list of applications for user {User}", currentUser);
                return Array.Empty<Shortage>();
            }
        }

        public void Register(Shortage shortage, string currentUser)
        {
            try
            {
                var all = _repository.LoadAll();
                var existing = all.FirstOrDefault(s => s.Title.Equals(shortage.Title, StringComparison.OrdinalIgnoreCase) &&s.Room == shortage.Room);

                if (existing != null)
                {
                    if (shortage.Priority > existing.Priority)
                    {
                        all.Remove(existing);
                        all.Add(new Shortage
                        {
                            Title = shortage.Title,
                            Name = currentUser,
                            Room = shortage.Room,
                            Category = shortage.Category,
                            Priority = shortage.Priority,
                            CreatedOn = DateTime.UtcNow
                        });
                        _logger.LogInformation("Updated application {Title} by user {User}", shortage.Title, currentUser);
                    }
                    else
                    {
                        throw new InvalidOperationException("A similar request already exists with the same or higher priority.");
                    }
                }
                else
                {
                    all.Add(new Shortage
                    {
                        Title = shortage.Title,
                        Name = currentUser,
                        Room = shortage.Room,
                        Category = shortage.Category,
                        Priority = shortage.Priority,
                        CreatedOn = DateTime.UtcNow
                    });
                    _logger.LogInformation("New request {Title} created by {User}", shortage.Title, currentUser);
                }

                _repository.SaveAll(all);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering application {Title} by user {User}", shortage.Title, currentUser);
                throw; // We pass it up so that the top level displays the error to the user.
            }
        }
        

        public void Delete(string title, Room room, string currentUser, bool isAdmin)
        {
            try
            {
                var all = _repository.LoadAll();
                var item = all.FirstOrDefault(s =>s.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&s.Room == room);

                if (item == null)
                {
                    throw new InvalidOperationException("No such element found.");
                }

                if (!isAdmin && item.Name != currentUser)
                {
                    throw new UnauthorizedAccessException("You can only delete your own entries.");
                }
                    
                all.Remove(item);
                _repository.SaveAll(all);
                _logger.LogInformation("Application {Title} has been deleted by user {User}", title, currentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting application {Title} by user {User}", title, currentUser);
                throw;
            }
        }

        public IReadOnlyCollection<Shortage> Filter(IEnumerable<Shortage> shortages,string? title = null,DateTime? from = null,DateTime? to = null,Category? category = null,Room? room = null)
        {
            var query = shortages.AsQueryable();

            try
            {
                if (!string.IsNullOrWhiteSpace(title))
                {
                    query = query.Where(s => s.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
                }

                if (from.HasValue)
                {
                    query = query.Where(s => s.CreatedOn.Date >= from.Value.Date);
                }

                if (to.HasValue)
                {
                    query = query.Where(s => s.CreatedOn.Date <= to.Value.Date);
                }

                if (category.HasValue)
                {
                    query = query.Where(s => s.Category == category.Value);
                }

                if (room.HasValue)
                {
                    query = query.Where(s => s.Room == room.Value);
                }

                return query.OrderByDescending(s => s.Priority).ThenByDescending(s => s.CreatedOn).ToList();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error filtering applications");
                return Array.Empty<Shortage>();
            }
        }
    }
}
