using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using VismaTask1.Models;
using VismaTask1.Repositories;

namespace VismaTask1.Repositories
{

    public class JsonShortageRepository : IShortageRepository
    {
        private readonly string _filePath;

        public JsonShortageRepository(string filePath)
        {
            _filePath = filePath;
        }

        public List<Shortage> LoadAll()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Shortage>();
            }

            string? json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Shortage>>(json) ?? new List<Shortage>();
        }

        public void SaveAll(List<Shortage> items)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(items, options);
            File.WriteAllText(_filePath, json);
        }
    }
}
