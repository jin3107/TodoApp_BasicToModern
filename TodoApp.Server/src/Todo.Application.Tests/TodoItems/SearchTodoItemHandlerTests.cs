using MayNghien.Infrastructures.Models.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using Todo.Application.Common;
using Todo.Application.Implementations.TodoItems;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Domain.Entities;
using Todo.DTOs.Auth;

namespace Todo.Application.Tests.TodoItems
{
    public class SearchTodoItemHandlerTests
    {
        private readonly Mock<ITodoItemRepository> _todoItemRepositoryMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly SearchTodoItemHandler _handler;

        public SearchTodoItemHandlerTests()
        {
            _todoItemRepositoryMock = new Mock<ITodoItemRepository>();
            _userServiceMock = new Mock<IUserService>();
            _handler = new SearchTodoItemHandler(_todoItemRepositoryMock.Object, _userServiceMock.Object, new Mock<ILogger<SearchTodoItemHandler>>().Object);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_CalculatesPagingCorrectly()
        {
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@gmail.com", Roles = new List<string> { "User" } });

            var fakePage = new PagedResult<TodoItem>
            {
                Items = new List<TodoItem> { new() { Title = "Học bài" } },
                TotalCount = 25
            };
            _todoItemRepositoryMock
                .Setup(r => r.SearchAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<TodoItem, bool>>>(),
                    It.IsAny<SortByInfo?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(fakePage);

            var request = new SearchRequest
            {
                PageIndex = 3,
                PageSize = 10,
                Filters = new List<Filter>()
            };

            var result = await _handler.HandleAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(25, result.Data!.TotalRows);
            Assert.Equal(3, result.Data!.TotalPages);  // ceil(25/10) = 3
            Assert.Equal(3, result.Data.CurrentPage);
            Assert.Single(result.Data.Data);
        }

        [Fact]
        public async Task HandleAsync_NonSuperAdmin_ScopesPredicateToOwnEmail()
        {
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "owner@test.com", Roles = new List<string> { "User" } });

            _todoItemRepositoryMock
                .Setup(r => r.SearchAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<TodoItem, bool>>>(),
                    It.IsAny<SortByInfo?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new PagedResult<TodoItem>());

            var request = new SearchRequest { PageIndex = 1, PageSize = 10, Filters = new List<Filter>() };

            await _handler.HandleAsync(request);

            _todoItemRepositoryMock.Verify(r => r.SearchAsync(
                It.Is<System.Linq.Expressions.Expression<Func<TodoItem, bool>>>(expr =>
                    expr.Compile()(new TodoItem { CreatedBy = "u1", IsDeleted = false }) &&
                    !expr.Compile()(new TodoItem { CreatedBy = "someone-else-id", IsDeleted = false })),
                It.IsAny<SortByInfo?>(),
                1, 10), Times.Once);
        }
    }
}
