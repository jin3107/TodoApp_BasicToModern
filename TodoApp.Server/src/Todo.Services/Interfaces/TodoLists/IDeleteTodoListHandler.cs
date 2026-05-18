using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Services.Interfaces.TodoLists
{
    public interface IDeleteTodoListHandler
    {
        Task<AppResponse<string>> HandleAsync(Guid id);
    }
}
