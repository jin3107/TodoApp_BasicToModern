using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Todo.Application.Implementations.TodoItems;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Domain.Entities;
using Todo.Domain.Enums;
using Todo.DTOs.Auth;
using Todo.DTOs.Requests;
using Xunit;

namespace Todo.Application.Tests.TodoItems
{
    public class UpdateTodoItemHandlerTests
    {
        private readonly Mock<ITodoItemRepository> _todoItemRepositoryMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UpdateTodoItemHandler _handler;

        public UpdateTodoItemHandlerTests()
        {
            _todoItemRepositoryMock = new Mock<ITodoItemRepository>();
            _userServiceMock = new Mock<IUserService>();
            _handler = new UpdateTodoItemHandler(_todoItemRepositoryMock.Object, _userServiceMock.Object, new Mock<ILogger<UpdateTodoItemHandler>>().Object);
        }

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsErrorAndNeverCallsUpdateAsync()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync((CurrentUserDto?)null);

            var request = new TodoItemRequest { Id = Guid.NewGuid(), Title = "Học bài" };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Unauthorized.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
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

            var request = new TodoItemRequest { Id = itemId, Title = "Học bài" };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Item not found or deleted.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
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

            var request = new TodoItemRequest { Id = itemId, Title = "Học bài (sửa)" };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Item not found or deleted.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
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

            var request = new TodoItemRequest { Id = itemId, Title = "Học bài (sửa)" };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Forbidden: you do not own this resource.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsSuccessAndUpdatesFields()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@test.com", Roles = new List<string> { "User" } });

            var itemId = Guid.NewGuid();
            var task = new TodoItem
            {
                Id = itemId,
                Title = "Học bài",
                Description = "Cũ",
                IsDeleted = false,
                IsCompleted = false,
                CreatedBy = "u1"
            };
            _todoItemRepositoryMock
                .Setup(s => s.GetByIdAsync(itemId))
                .ReturnsAsync(task);

            var request = new TodoItemRequest
            {
                Id = itemId,
                Title = "Học bài (sửa)",
                Description = "Ôn thi cuối kỳ",
                DueDate = DateTime.UtcNow.AddDays(2),
                Priority = Tier.High,
                IsCompleted = false
            };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Item updated successfully.", result.Message);
            Assert.Equal("Học bài (sửa)", result.Data!.Title);

            Assert.Equal("Học bài (sửa)", task.Title);
            Assert.Equal("Ôn thi cuối kỳ", task.Description);
            Assert.Null(task.CompletedOn);

            _todoItemRepositoryMock.Verify(r => r.GetByIdAsync(itemId), Times.Once);
            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_MarkAsCompletedWithoutCompletedOn_SetsCompletedOnToUtcNow()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@test.com", Roles = new List<string> { "User" } });

            var itemId = Guid.NewGuid();
            var task = new TodoItem { Id = itemId, Title = "Học bài", IsDeleted = false, CreatedBy = "u1" };
            _todoItemRepositoryMock
                .Setup(s => s.GetByIdAsync(itemId))
                .ReturnsAsync(task);

            var beforeCall = DateTime.UtcNow;
            var request = new TodoItemRequest { Id = itemId, Title = "Học bài", IsCompleted = true, CompletedOn = null };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(task.CompletedOn);
            Assert.True(task.CompletedOn >= beforeCall);
        }

        [Fact]
        public async Task HandleAsync_SuperAdminNotOwner_ReturnsSuccessAndUpdatesTask()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "admin@test.com", Roles = new List<string> { "SuperAdmin" } });

            var itemId = Guid.NewGuid();
            var task = new TodoItem { Id = itemId, Title = "Học bài", IsDeleted = false, CreatedBy = "someone-else@test.com" };
            _todoItemRepositoryMock
                .Setup(s => s.GetByIdAsync(itemId))
                .ReturnsAsync(task);

            var request = new TodoItemRequest { Id = itemId, Title = "Học bài (admin sửa)" };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Học bài (admin sửa)", task.Title);

            _todoItemRepositoryMock.Verify(r => r.UpdateAsync(task), Times.Once);
        }
    }
}
