using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using VismaTask1.Services;

namespace VismaTask1.Commands
{
    public abstract class BaseCommand : Command
    {
        protected readonly IShortageService Service;
        protected readonly ILogger Logger;
        protected readonly string Username;
        protected readonly bool IsAdmin;

        protected BaseCommand(string name, string description,IShortageService service, ILogger logger,string username,bool isAdmin)
        :base(name, description)
        {
            Service = service;
            Logger = logger;
            Username = username;
            IsAdmin = isAdmin;
        }
    }
}
