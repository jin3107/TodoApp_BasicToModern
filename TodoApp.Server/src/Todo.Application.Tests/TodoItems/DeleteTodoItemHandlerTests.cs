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

namespace Todo.Application.Tests.TodoItems
{
    public class DeleteTodoItemHandlerTests
    {
        private readonly Mock<ITodoItemRepository> _todoItemRepositoryMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly DeleteTodoItemHandler _handler;

        public DeleteTodoItemHandlerTests() 
        {
            _todoItemRepositoryMock = new Mock<ITodoItemRepository>();
            _userServiceMock = new Mock<IUserService>();
            _handler = new DeleteTodoItemHandler(_todoItemRepositoryMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsErrorAndNeverCallsDeleteAsync()
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
            Assert.Equal("Unauthorized.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Never);
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
        public async Task HandleAsync_ValidRequest_ReturnsSuccessAndCallsDeleteAsyncOnce()
        {
            // Arrange
            _userServiceMock
                .Setup(r => r.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@test.com", Roles = new List<string> { "User" } });

            var itemId = Guid.NewGuid();
            var task = new TodoItem { Id = itemId, IsDeleted = false, CreatedBy = "a@test.com" };
            _todoItemRepositoryMock
                .Setup(s => s.GetByIdAsync(itemId))
                .ReturnsAsync(task);

            // Act
            var result = await _handler.HandleAsync(itemId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Item deleted successfully.", result.Data);
            Assert.True(task.IsDeleted);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_UserNotOwnerAndNotSuperAdmin_ReturnsForbiddenError()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@test.com", Roles = new List<string> { "User" } });

            var itemId = Guid.NewGuid();
            _todoItemRepositoryMock
                .Setup(s => s.GetByIdAsync(itemId))
                .ReturnsAsync(new TodoItem { Id = itemId, IsDeleted = false, CreatedBy = "someone-else@test.com" });

            // Act
            var result = await _handler.HandleAsync(itemId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Forbidden: you do not own this resource.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_SuperAdminNotOwner_ReturnsSuccessAndCallsDeleteAsyncOnce()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "admin@test.com", Roles = new List<string> { "SuperAdmin" } });

            var itemId = Guid.NewGuid();
            var task = new TodoItem { Id = itemId, IsDeleted = false, CreatedBy = "someone-else@test.com" };
            _todoItemRepositoryMock
                .Setup(s => s.GetByIdAsync(itemId))
                .ReturnsAsync(task);

            // Act
            var result = await _handler.HandleAsync(itemId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Item deleted successfully.", result.Data);
            Assert.True(task.IsDeleted);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
        }
    }
}
