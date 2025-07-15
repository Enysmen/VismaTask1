using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;

using VismaTask1.Models;
using VismaTask1.Repositories;

namespace VismaTask1.Services
{
    public class ShortageService : IShortageService
    {
        private readonly IShortageRepository _repository;

        public ShortageService(IShortageRepository repository)
        {
            _repository = repository;
        }

        public IReadOnlyCollection<Shortage> GetAll(string currentUser, bool isAdmin)
        {
            var all = _repository.LoadAll();
            return isAdmin ? all : all.Where(s => s.Name == currentUser).ToList();
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
                }

                _repository.SaveAll(all);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error registering application {Title} by user {User}", shortage.Title, currentUser);
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
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting application {Title} by user {User}", title, currentUser);
                throw;
            }
        }

        public IReadOnlyCollection<Shortage> Filter(IEnumerable<Shortage> shortages,string? title = null,DateTime? from = null,DateTime? to = null,Category? category = null,Room? room = null)
        {
            var query = shortages.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(s => s.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            if (from.HasValue)
            {
                query = query.Where(s => s.CreatedOn >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(s => s.CreatedOn <= to.Value);
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
    }
}
