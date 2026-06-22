using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Domain.Entities;
using Todo.DTOs.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoItems;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoItems
{
    public class GetTodoItemByIdHandler : IGetTodoItemByIdHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetTodoItemByIdHandler(ITodoItemRepository todoItemRepository, 
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _todoItemRepository = todoItemRepository;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        => await _userManager.FindByEmailAsync(_contextAccessor.HttpContext?.User.Identity?.Name!);

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(Guid id)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var user = await GetCurrentUser();
                if (user == null)
                    return result.BuildError("Unauthorized");

                var task = await _todoItemRepository.FindByAsync(p => p.Id == id).FirstOrDefaultAsync();
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                var response = TodoItemMapper.ToResponse(task);
                return result.BuildResult(response);
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
