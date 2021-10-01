using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLmessanger.Abstraction;
using TLmessanger.Commands;

namespace TLmessanger.Services
{
    public class CommandService : ICommandService
    {
        private readonly List<TelegramCommand> _commands;

        public CommandService()
        {
            _commands = new List<TelegramCommand>
            {
                new StartCommand(),
                new MainCommand()
            };
        }

        public List<TelegramCommand> Get() => _commands;
    }
}
