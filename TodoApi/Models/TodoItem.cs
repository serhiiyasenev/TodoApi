using System.Collections.Generic;

namespace TodoApi.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
        public List<TodoItemValue> TodoItemValues { get; set; }
    }
}