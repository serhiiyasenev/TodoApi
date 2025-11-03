using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/items")]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoController(TodoContext context)
        {
            _context = context;

            if (_context.TodoItems.Any()) return;

            _context.TodoItems.Add(
                new TodoItem
                {
                    Id = 1,
                    Name = "Item1",
                    IsComplete = true,
                    TodoItemValues = [new TodoItemValue { Id = 1, Value = 22 }]
                }
            );

            _context.SaveChanges();
        }

        [HttpGet(Name = "GetAllItems")]
        [EnableQuery]
        public IActionResult GetAll()
        {
            try
            {
                var allItems = _context.TodoItems.Include(ti => ti.TodoItemValues);
                return Ok(allItems);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpGet("{id:int}", Name = "GetItemById")]
        public IActionResult GetById(int id)
        {
            try
            {
                var item = _context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == id);
                if (item == null)
                {
                    return NotFound();
                }

                return Ok(item);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpPost(Name = "CreateItem")]
        public IActionResult Create([FromBody] TodoItem? item)
        {
            try
            {
                if (item == null)
                {
                    return BadRequest();
                }

                var items = _context.TodoItems.ToList();

                if (items.Any(it => it.Id == item.Id))
                {
                    return BadRequest("Wrong request: item with id " + item.Id + " has already been added");
                }

                item.TodoItemValues ??= [];

                foreach (var val in item.TodoItemValues)
                {
                    _context.TodoItemValues.Add(val);
                }

                _context.TodoItems.Add(item);
                _context.SaveChanges();

                return CreatedAtRoute("GetItemById", new { id = item.Id }, item);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpPut("{id:int}", Name = "UpdateItemById")]
        public IActionResult Update(int id, [FromBody] TodoItem? item)
        {
            try
            {
                if (item == null || item.Id != id)
                {
                    return BadRequest();
                }

                var todo = _context.TodoItems.FirstOrDefault(t => t.Id == id);
                if (todo == null)
                {
                    return NotFound();
                }

                todo.IsComplete = item.IsComplete;
                todo.Name = item.Name;
                // Don't update TodoItemValues here - use the values endpoints for that

                _context.TodoItems.Update(todo);
                _context.SaveChanges();
                return Ok(todo);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpDelete("{id:int}", Name = "DeleteItemById")]
        public IActionResult Delete(int id)
        {
            try
            {
                var todo = _context.TodoItems.FirstOrDefault(t => t.Id == id);
                if (todo == null)
                {
                    return NotFound();
                }

                _context.TodoItems.Remove(todo);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }
    }
}