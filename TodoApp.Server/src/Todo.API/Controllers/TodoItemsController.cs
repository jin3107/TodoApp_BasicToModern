using MayNghien.Infrastructures.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Todo.DTOs.Requests;
using Todo.Services.Interfaces;
using Todo.Services.Interfaces.TodoItems;

namespace Todo.API.Controllers
{
    [Route("todo-items")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ICreateTodoItemHandler _create;
        private readonly IUpdateTodoItemHandler _update;
        private readonly IDeleteTodoItemHandler _delete;
        private readonly IGetTodoItemByIdHandler _getById;
        private readonly ISearchTodoItemHandler _search;

        public TodoItemsController(
            ICreateTodoItemHandler create,
            IUpdateTodoItemHandler update,
            IDeleteTodoItemHandler delete,
            IGetTodoItemByIdHandler getById,
            ISearchTodoItemHandler search)
        {
            _create = create;
            _update = update;
            _delete = delete;
            _getById = getById;
            _search = search;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
            => Ok(await _getById.HandleAsync(id));

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TodoItemRequest request)
            => Ok(await _create.HandleAsync(request));

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] TodoItemRequest request)
            => Ok(await _update.HandleAsync(request));

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
            => Ok(await _delete.HandleAsync(id));

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
            => Ok(await _search.HandleAsync(request));
    }
}
