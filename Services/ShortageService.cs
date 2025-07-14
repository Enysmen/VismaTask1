using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var all = _repository.LoadAll();

            var existing = all.FirstOrDefault(s =>
                s.Title.Equals(shortage.Title, StringComparison.OrdinalIgnoreCase) &&
                s.Room == shortage.Room);

            if (existing != null)
            {
                if (shortage.Priority > existing.Priority)
                {
                    all.Remove(existing);
                    all.Add(shortage with
                    {
                        Name = currentUser,
                        CreatedOn = DateTime.UtcNow
                    });
                }
                else
                {
                    throw new InvalidOperationException("Такой запрос уже существует с таким же или более высоким приоритетом.");
                }
            }
            else
            {
                all.Add(shortage with
                {
                    Name = currentUser,
                    CreatedOn = DateTime.UtcNow
                });
            }

            _repository.SaveAll(all);
        }

        public void Delete(string title, Room room, string currentUser, bool isAdmin)
        {
            var all = _repository.LoadAll();

            var item = all.FirstOrDefault(s =>
                s.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
                s.Room == room);

            if (item == null)
                throw new InvalidOperationException("Такой элемент не найден.");

            if (!isAdmin && item.Name != currentUser)
                throw new UnauthorizedAccessException("Удалять можно только свои записи.");

            all.Remove(item);
            _repository.SaveAll(all);
        }

        public IReadOnlyCollection<Shortage> Filter(
            IEnumerable<Shortage> shortages,
            string? title = null,
            DateTime? from = null,
            DateTime? to = null,
            Category? category = null,
            Room? room = null)
        {
            var query = shortages.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(s => s.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (from.HasValue)
                query = query.Where(s => s.CreatedOn >= from.Value);

            if (to.HasValue)
                query = query.Where(s => s.CreatedOn <= to.Value);

            if (category.HasValue)
                query = query.Where(s => s.Category == category.Value);

            if (room.HasValue)
                query = query.Where(s => s.Room == room.Value);

            return query
                .OrderByDescending(s => s.Priority)
                .ThenByDescending(s => s.CreatedOn)
                .ToList();
        }
    }
}
