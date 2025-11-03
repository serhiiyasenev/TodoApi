using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/items")]
    public class TodoValuesController(TodoContext context) : ControllerBase
    {
        [HttpGet("{itemId:int}/values", Name = "GetAllItemValuesById")]
        public IActionResult GetItemValuesByIdOfItem(int itemId)
        {
            try
            {
                var item = context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == itemId);
                if (item == null)
                {
                    return NotFound();
                }

                return Ok(item.TodoItemValues);
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
                var item = context.TodoItems.Include(ti => ti.TodoItemValues).FirstOrDefault(t => t.Id == itemId);
                if (item == null)
                {
                    return NotFound();
                }

                var itemValue = item.TodoItemValues.FirstOrDefault(v => v.Id == valueId);
                if (itemValue == null)
                {
                    return NotFound();
                }

                return Ok(itemValue.Value);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpPost("{itemId:int}/values", Name = "AddItemValueById")]
        public IActionResult AddValue(int itemId, [FromBody] TodoItemValue? itemValue)
        {
            try
            {
                if (itemValue == null)
                {
                    return BadRequest("Item value cannot be null");
                }

                var todo = context.TodoItems
                    .Include(todoItem => todoItem.TodoItemValues)
                    .FirstOrDefault(t => t.Id == itemId);

                if (todo == null)
                {
                    return NotFound();
                }

                // Ensure the foreign key is set
                itemValue.TodoItemId = itemId;

                // Add to the context directly for proper tracking
                context.TodoItemValues.Add(itemValue);
                context.SaveChanges();

                // Reload the item with values to get the updated list
                var updatedTodo = context.TodoItems
                    .Include(t => t.TodoItemValues)
                    .FirstOrDefault(t => t.Id == itemId);

                return CreatedAtRoute("GetAllItemValuesById", new { itemId = todo.Id }, updatedTodo?.TodoItemValues);
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }

        [HttpPut("{itemId:int}/values", Name = "UpdateItemValueById")]
        public IActionResult UpdateValue(int itemId, [FromBody] TodoItemValue? value)
        {
            try
            {
                if (value == null)
                {
                    return BadRequest("Value cannot be null");
                }

                var item = context.TodoItems
                    .Include(ti => ti.TodoItemValues)
                    .FirstOrDefault(t => t.Id == itemId);

                if (item == null)
                {
                    return NotFound();
                }

                var val = item.TodoItemValues.FirstOrDefault(v => v.Id == value.Id);

                if (val == null)
                {
                    // Ensure foreign key is set before adding
                    value.TodoItemId = itemId;
                    return AddValue(itemId, value);
                }

                val.Value = value.Value;
                context.TodoItemValues.Update(val);
                context.SaveChanges();

                return Ok($"New value is '{val.Value}' in item {item.Id} for value {val.Id}");
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
                var item = context.TodoItems
                    .Include(ti => ti.TodoItemValues)
                    .FirstOrDefault(t => t.Id == itemId);

                if (item == null)
                {
                    return NotFound();
                }

                var value = item.TodoItemValues.FirstOrDefault(v => v.Id == valueId);

                if (value == null)
                {
                    return NotFound();
                }

                context.TodoItemValues.Remove(value);
                context.SaveChanges();

                return Ok($"Value with id '{value.Id}' and value '{value.Value}' was deleted from item: {item.Id}");
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
                var item = context.TodoItems
                    .Include(ti => ti.TodoItemValues)
                    .FirstOrDefault(t => t.Id == itemId);

                if (item == null)
                {
                    return NotFound();
                }

                if (item.TodoItemValues.Count == 0)
                {
                    return NotFound("No values found for this item");
                }

                context.TodoItemValues.RemoveRange(item.TodoItemValues);
                context.SaveChanges();

                return Ok($"All values from item {item.Id} were deleted");
            }
            catch (Exception e)
            {
                return BadRequest("Wrong request: " + e.Message);
            }
        }
    }
}