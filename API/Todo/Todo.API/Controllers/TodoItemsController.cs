using MayNghien.Infrastructures.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Todo.DTOs.Requests;
using Todo.Services.Interfaces;

namespace Todo.API.Controllers
{
    [Route("todo-item")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemService _taskService;

        public TodoItemsController(ITodoItemService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var result = await _taskService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] TodoItemRequest request)
        {
            var result = await _taskService.CreateAsync(request);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] TodoItemRequest request)
        {
            var result = await _taskService.UpdateAsync(request);
            return Ok(result);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var result = await _taskService.DeleteAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            var result = await _taskService.SearchAsync(request);
            return Ok(result);
        }
    }
}
