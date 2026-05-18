using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoLists;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoLists
{
    public class CreateTodoListHandler : ICreateTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateTodoListHandler(ITodoListRepository todoListRepository,
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _todoListRepository = todoListRepository;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        => await _userManager.FindByEmailAsync(_contextAccessor.HttpContext?.User.Identity?.Name!);

        public async Task<AppResponse<TodoListResponse>> HandleAsync(TodoListRequest request)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var user = await GetCurrentUser();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var entity = TodoListMapper.ToEntity(request);
                entity.Id = Guid.NewGuid();
                entity.Name = request.Name;
                entity.Description = request.Description;
                entity.CreatedBy = user.Email;
                entity.CreatedOn = DateTime.UtcNow;
                await _todoListRepository.AddAsync(entity);

                var response = TodoListMapper.ToResponse(entity);
                return result.BuildResult(response, "Todo list created successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
