using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VismaTask1.Models;

namespace VismaTask1.Repositories
{
    // Interface for the data storage layer
    public interface IShortageRepository
    {
        // Loads all requests from storage
        List<Shortage> LoadAll();

        // Saves the full list of requests to storage
        void SaveAll(List<Shortage> items);
    }
}
