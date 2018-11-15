using Ipreo.Eventhub.Logger.Serilog;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Produces("application/json")]
    [Route("api/items")]
    public class TodoController : ODataController
    {
        private readonly TodoContext _context;
        private readonly ILogger _logger;

        public TodoController(TodoContext context)
        {
            _context = context;

            if (!_context.TodoItems.Any())
            {
                context.Database.OpenConnection();
                context.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.TodoItems ON; INSERT INTO dbo.TodoItems (Id, Name, IsComplete) VALUES (0, 'test1', 1);");
                _context.TodoItems.Add(
                    new TodoItem
                    {
                        Id = 1,
                        Name = "Item1",
                        IsComplete = true,
                        TodoItemValues = new List<TodoItemValue> { new TodoItemValue { Id = 22, Value = 22 }
                        }
                    }
                );
                _context.SaveChanges();
            }
            var todoApiLogger = new SerilogFactory(null, LogConfiguration.ConfigurationRoot);
            _logger = todoApiLogger.CreateLogger("todoapilogger");
        }

        [HttpGet(Name = "GetAllItems")]
        [EnableQuery]
        public IActionResult GetAll()
        {
            try
            {
                _context.TodoItems.UpdateRange();
                var allItems = _context.TodoItems.Include(ti => ti.TodoItemValues).ToList();

                _logger.LogInformation(
                    "Get Values: {0} with ids: {1}", string.Join(";", allItems.Select(i => i.Name)),
                    string.Join(";", allItems.Select(i => i.Id)));

                return new ObjectResult(allItems);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Exception: ", e.Message);
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpGet("{id:int}", Name = "GetItemById")]
        public IActionResult GetById(int id)
        {
            try
            {
                _context.TodoItems.UpdateRange();
                var item = _context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == id);
                if (item == null)
                {
                    return NotFound();
                }

                return new ObjectResult(item);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpPost(Name = "CreateItem")]
        public IActionResult Create([FromBody] TodoItem item)
        {
            try
            {
                if (item == null)
                {
                    return BadRequest();
                }

                List<TodoItem> items = _context.TodoItems.ToList();

                if (items.Count(it => it.Id == item.Id) != 0)
                {
                    return BadRequest("Wrong request: item with id " + item.Id + " has already been added");
                }

                if (item.TodoItemValues == null)
                {
                    item.TodoItemValues = new List<TodoItemValue>();
                }
                foreach (var val in item.TodoItemValues)
                {
                    _context.TodoItemValues.Add(val);
                }

                _context.TodoItems.Add(item);
                _context.SaveChanges();

                return CreatedAtRoute("GetTodo", new { id = item.Id, name = item.Name, isComplete = item.IsComplete, values = item.TodoItemValues }, item);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpPut("{id:int}", Name = "UpdateItemById")]
        public IActionResult Update(int id, [FromBody] TodoItem item)
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
                todo.TodoItemValues = item.TodoItemValues;

                _context.TodoItems.Update(todo);
                _context.SaveChanges();
                return CreatedAtRoute("GetTodo", new { id = item.Id, name = item.Name, isComplete = item.IsComplete, values = item.TodoItemValues }, item);
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
                return new NoContentResult();
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }
    }
}