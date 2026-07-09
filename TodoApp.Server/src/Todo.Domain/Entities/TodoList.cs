namespace Todo.Domain.Entities
{
    public class TodoList : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<TodoItem>? TodoItems { get; set; }
    }
}
