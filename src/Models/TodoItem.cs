namespace TodoApi.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public required string Name { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public List<TodoItemValue> TodoItemValues { get; set; } = [];
    }
}