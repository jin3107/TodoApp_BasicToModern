using Todo.Domain.Enums;

namespace Todo.Domain.Entities
{
    public class TodoItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public Tier Priority { get; set; }
        public DateTime? CompletedOn { get; set; }

        public Guid? TodoListId { get; set; }
        public TodoList? TodoList { get; set; }
    }
}
