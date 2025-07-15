using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VismaTask1.Models;

namespace VismaTask1.Services
{
    public interface IShortageService
    {
        IReadOnlyCollection<Shortage> GetAll(string currentUser, bool isAdmin);
        void Register(Shortage shortage, string currentUser);
        void Delete(string title, Room room, string currentUser, bool isAdmin);
        IReadOnlyCollection<Shortage> Filter(IEnumerable<Shortage> shortages,string? title = null,DateTime? from = null,DateTime? to = null,Category? category = null,Room? room = null);
    }
}
