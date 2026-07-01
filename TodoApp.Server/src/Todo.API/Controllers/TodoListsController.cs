using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.DTOs.Requests;
using Todo.Application.Interfaces.TodoLists;

namespace Todo.API.Controllers
{
    [Route("todo-lists")]
    [ApiController]
    [Authorize]
    public class TodoListsController : ControllerBase
    {
        private readonly ICreateTodoListHandler _create;
        private readonly IUpdateTodoListHandler _update;
        private readonly IDeleteTodoListHandler _delete;
        private readonly IGetTodoListByIdHandler _getById;
        private readonly ISearchTodoListHandler _search;

        public TodoListsController(
            ICreateTodoListHandler create,
            IUpdateTodoListHandler update,
            IDeleteTodoListHandler delete,
            IGetTodoListByIdHandler getById,
            ISearchTodoListHandler search)
        {
            _create = create;
            _update = update;
            _delete = delete;
            _getById = getById;
            _search = search;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
            => ToActionResult(await _getById.HandleAsync(id));

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TodoListRequest request)
            => ToActionResult(await _create.HandleAsync(request));

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] TodoListRequest request)
            => ToActionResult(await _update.HandleAsync(request));

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
            => ToActionResult(await _delete.HandleAsync(id));

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
            => ToActionResult(await _search.HandleAsync(request));

        private IActionResult ToActionResult<T>(AppResponse<T> result)
        {
            if (result.IsSuccess)
                return Ok(result);

            if (!string.IsNullOrWhiteSpace(result.Message) &&
                result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            return BadRequest(result);
        }
    }
}