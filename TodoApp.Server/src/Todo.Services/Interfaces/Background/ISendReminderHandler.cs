using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Services.Interfaces.Background
{
    public interface ISendReminderHandler
    {
        Task HandleAsync();
    }
}
