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
        
        List<Shortage> LoadAll();

        
        void SaveAll(List<Shortage> items);
    }
}
