using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLmessanger.Abstraction
{
    public interface ICommandService
    {
        List<TelegramCommand> Get();
    }
}
