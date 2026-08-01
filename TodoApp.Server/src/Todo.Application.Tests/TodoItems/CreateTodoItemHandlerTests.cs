using Moq;
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
    public class CreateTodoItemHandlerTests
    {
        private readonly Mock<ITodoItemRepository> _todoItemRepositoryMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly CreateTodoItemHandler _handler;

        public CreateTodoItemHandlerTests()
        {
            _todoItemRepositoryMock = new Mock<ITodoItemRepository>();
            _userServiceMock = new Mock<IUserService>();
            _handler = new CreateTodoItemHandler(_todoItemRepositoryMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsSuccessAndCallsAddAsyncOnce()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync(new CurrentUserDto { Id = "u1", Email = "a@test.com", Roles = new List<string> { "User" } });

            var request = new TodoItemRequest
            {
                Title = "Học bài",
                Description = "Ôn thi giữa kỳ",
                DueDate = DateTime.UtcNow.AddDays(1),
                Priority = Tier.Medium
            };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Học bài", result.Data!.Title);
            Assert.False(result.Data.IsCompleted);

            _todoItemRepositoryMock.Verify(
                r => r.AddAsync(It.Is<TodoItem>(t =>
                    t.Title == "Học bài" &&
                    t.CreatedBy == "a@test.com" &&
                    t.IsCompleted == false)),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_UserNotFound_ReturnsErrorAndNeverCallsCreateTodoItemAsync()
        {
            // Arrange
            _userServiceMock
                .Setup(s => s.GetCurrentUserAsync())
                .ReturnsAsync((CurrentUserDto?)null);

            var request = new TodoItemRequest { Title = "Học bài" };

            // Act
            var result = await _handler.HandleAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Unauthorized.", result.Message);

            _todoItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TodoItem>()), Times.Never);
        }
    }
}
