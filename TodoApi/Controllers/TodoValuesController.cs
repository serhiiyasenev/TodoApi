using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Produces("application/json")]
    [Route("api/items")]
    public class TodoValuesController : ODataController
    {
        private readonly TodoContext _context;

        public TodoValuesController(TodoContext context)
        {
            _context = context;
        }

        [HttpGet("{itemId:int}/values", Name = "GetAllItemValuesById")]
        public IActionResult GetItemValuesByIdOfItem(int itemId)
        {
            try
            {
                _context.TodoItems.UpdateRange();
                var item = _context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == itemId);
                if (item == null)
                {
                    return NotFound();
                }

                return new ObjectResult(item.TodoItemValues);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpGet("{itemId:int}/values/{valueId:int}", Name = "GetItemValueByIdAndValueId")]
        public IActionResult GetItemValueByIdOfItem(int itemId, int valueId)
        {
            try
            {
                _context.TodoItems.UpdateRange();
                var item = _context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == itemId);
                if (item == null)
                {
                    return NotFound();
                }

                double value = 0;

                if (item.TodoItemValues != null)
                {
                    value = item.TodoItemValues.First(v => v.Id == valueId).Value;
                }

                return new ObjectResult(value);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpPost("{itemId:int}/values", Name = "AddItemValueById")]
        public IActionResult AddValue(int itemId, [FromBody] TodoItemValue itemValue)
        {
            try
            {
                if (itemValue == null || itemId.Equals(null))
                {
                    return BadRequest();
                }

                var todo = _context.TodoItems.FirstOrDefault(t => t.Id == itemId);
                if (todo == null)
                {
                    return NotFound();
                }

                if (todo.TodoItemValues == null)
                {
                    todo.TodoItemValues = new List<TodoItemValue>();
                }

                todo.TodoItemValues.Add(itemValue);
                _context.TodoItems.Update(todo);
                _context.SaveChanges();

                return CreatedAtRoute("GetTodo", new
                {
                    id = todo.Id,
                    name = todo.Name,
                    isComplete = todo.IsComplete,
                    values = todo.TodoItemValues
                }, _context.TodoItems.Where(item => item.Id == itemId).Select(item => item.TodoItemValues));
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpPut("{itemId:int}/values", Name = "UpdateItemValueById")]
        public IActionResult UpdateValue(int itemId, [FromBody] TodoItemValue value)
        {
            try
            {
                if (value == null)
                {
                    return BadRequest();
                }

                var item = _context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == itemId);

                if (item == null)
                {
                    return NotFound();
                }

                var val = item.TodoItemValues.FirstOrDefault(v => v.Id == value.Id);

                if (val == null)
                {
                    AddValue(itemId, value);
                }
                else
                {
                    val.Value = value.Value;
                }

                _context.TodoItems.Update(item);
                _context.SaveChanges();
                return new ObjectResult($"New value is '{val?.Value}' in item {item.Id} for value {val?.Id}");
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpDelete("{itemId:int}/values/{valueId:int}", Name = "DeleteItemValueById")]
        public IActionResult Delete(int itemId, int valueId)
        {
            try
            {
                var item = _context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == itemId);
                if (item == null)
                {
                    return NotFound();
                }

                var value = item.TodoItemValues.FirstOrDefault(v => v.Id == valueId);

                if (value == null)
                {
                    return NotFound();
                }

                _context.TodoItemValues.Remove(value);
                _context.SaveChanges();

                string result = $"Value with id '{value.Id}' and value '{value.Value}' was deleted " +
                                $"from item: {item.Id}";

                return new ObjectResult(result);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpDelete("{itemId:int}/values", Name = "DeleteAllItemValues")]
        public IActionResult DeleteAllValues(int itemId)
        {
            try
            {
                var item = _context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == itemId);
                if (item == null)
                {
                    return NotFound();
                }

                List<TodoItemValue> values = item.TodoItemValues.ToList();

                if (values.Count == 0)
                {
                    return NotFound();
                }

                _context.TodoItemValues.RemoveRange(values);
                _context.SaveChanges();

                string result = $"All Values from item: {item.Id} was deleted";

                return new ObjectResult(result);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }
    }
}