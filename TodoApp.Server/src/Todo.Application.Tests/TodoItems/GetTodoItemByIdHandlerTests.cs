using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Application.Implementations.TodoItems;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Domain.Entities;
using Todo.DTOs.Auth;
using Todo.DTOs.Requests;

namespace Todo.Application.Tests.TodoItems
{
    public class GetTodoItemByIdHandlerTests
    {
        private readonly Mock<ITodoItemRepository> _todoItemRepositoryMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly GetTodoItemByIdHandler _handler;

        public GetTodoItemByIdHandlerTests()
        {
            _todoItemRepositoryMock = new Mock<ITodoItemRepository>();
            _userServiceMock = new Mock<IUserService>();
            _handler = new GetTodoItemByIdHandler(_todoItemRepositoryMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsSuccessAndCallsGetByIdAsyncOnce()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@test.com", Roles = new List<string> { "User" } });

            var itemId = Guid.NewGuid();
            _todoItemRepositoryMock
               .Setup(s => s.GetByIdAsync(itemId))
               .ReturnsAsync(new TodoItem { Id = itemId, Title = "Học bài", IsDeleted = false });

            // Act
            var result = await _handler.HandleAsync(itemId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Học bài", result.Data!.Title);
            Assert.False(result.Data.IsCompleted);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_RepositoryReturnsNull_ReturnsNotFoundError()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@test.com", Roles = new List<string> { "User" } });

            var itemId = Guid.NewGuid();
            _todoItemRepositoryMock
               .Setup(s => s.GetByIdAsync(itemId))
               .ReturnsAsync((TodoItem?)null);

            // Act
            var result = await _handler.HandleAsync(itemId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Item not found or deleted.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_TaskIsDeleted_ReturnsNotFoundError()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@test.com", Roles = new List<string> { "User" } });

            var itemId = Guid.NewGuid();
            _todoItemRepositoryMock
               .Setup(s => s.GetByIdAsync(itemId))
               .ReturnsAsync(new TodoItem { Id = itemId, Title = "Học bài", IsDeleted = true });

            // Act
            var result = await _handler.HandleAsync(itemId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Item not found or deleted.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsErrorAndNeverCallsGetByIdAsync()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync((CurrentUserDto?)null);

            var itemId = Guid.NewGuid();
            _todoItemRepositoryMock
               .Setup(s => s.GetByIdAsync(itemId))
               .ReturnsAsync(new TodoItem { Id = itemId });

            // Act
            var result = await _handler.HandleAsync(itemId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Unauthorized", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Never);
        }
    }
}
