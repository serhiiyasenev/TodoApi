using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TodoApi.Models;

namespace TodoApi.IntegrationTests;

public class TodoControllerIntegrationTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsOk_WithTodoItems()
    {
        // Act
        var response = await _client.GetAsync("/api/items");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        items.Should().NotBeNull();
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithODataQuery_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/items?$filter=isComplete eq true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        items.Should().NotBeNull();
        items.Should().AllSatisfy(item => item.IsComplete.Should().BeTrue());
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOk_WithTodoItem()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/items", new TodoItem
        {
            Id = 100,
            Name = "Test Item for GetById",
            IsComplete = false
        });
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync("/api/items/100");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var item = await response.Content.ReadFromJsonAsync<TodoItem>();
        item.Should().NotBeNull();
        item!.Id.Should().Be(100);
        item.Name.Should().Be("Test Item for GetById");
        item.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/items/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidItem_ReturnsCreated_WithTodoItem()
    {
        // Arrange
        var newItem = new TodoItem
        {
            Id = 200,
            Name = "New Test Item",
            IsComplete = false,
            TodoItemValues = [new TodoItemValue { Id = 201, Value = 42 }]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/items", newItem);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdItem = await response.Content.ReadFromJsonAsync<TodoItem>();
        createdItem.Should().NotBeNull();
        createdItem!.Id.Should().Be(200);
        createdItem.Name.Should().Be("New Test Item");
        createdItem.TodoItemValues.Should().HaveCount(1);
        createdItem.TodoItemValues.First().Value.Should().Be(42);
    }

    [Fact]
    public async Task Create_WithDuplicateId_ReturnsBadRequest()
    {
        // Arrange
        var item1 = new TodoItem { Id = 300, Name = "First Item", IsComplete = false };
        await _client.PostAsJsonAsync("/api/items", item1);

        var item2 = new TodoItem { Id = 300, Name = "Duplicate Item", IsComplete = false };

        // Act
        var response = await _client.PostAsJsonAsync("/api/items", item2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNullItem_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/items", (TodoItem?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidItem_ReturnsOk_WithUpdatedItem()
    {
        // Arrange
        var createItem = new TodoItem { Id = 400, Name = "Original Item", IsComplete = false };
        await _client.PostAsJsonAsync("/api/items", createItem);

        var updateItem = new TodoItem { Id = 400, Name = "Updated Item", IsComplete = true };

        // Act
        var response = await _client.PutAsJsonAsync("/api/items/400", updateItem);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedItem = await response.Content.ReadFromJsonAsync<TodoItem>();
        updatedItem.Should().NotBeNull();
        updatedItem!.Id.Should().Be(400);
        updatedItem.Name.Should().Be("Updated Item");
        updatedItem.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var updateItem = new TodoItem { Id = 99998, Name = "Non-existent Item", IsComplete = false };

        // Act
        var response = await _client.PutAsJsonAsync("/api/items/99998", updateItem);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var updateItem = new TodoItem { Id = 500, Name = "Item", IsComplete = false };

        // Act
        var response = await _client.PutAsJsonAsync("/api/items/501", updateItem);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var createItem = new TodoItem { Id = 600, Name = "Item to Delete", IsComplete = false };
        await _client.PostAsJsonAsync("/api/items", createItem);

        // Act
        var response = await _client.DeleteAsync("/api/items/600");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify item is deleted
        var getResponse = await _client.GetAsync("/api/items/600");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/items/99997");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullCrudWorkflow_CreatesUpdatesAndDeletesItem()
    {
        // Create
        var newItem = new TodoItem { Id = 700, Name = "Workflow Item", IsComplete = false };
        var createResponse = await _client.PostAsJsonAsync("/api/items", newItem);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read
        var getResponse = await _client.GetAsync("/api/items/700");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
        item!.Name.Should().Be("Workflow Item");

        // Update
        item.Name = "Updated Workflow Item";
        item.IsComplete = true;
        var updateResponse = await _client.PutAsJsonAsync("/api/items/700", item);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify Update
        var getUpdatedResponse = await _client.GetAsync("/api/items/700");
        var updatedItem = await getUpdatedResponse.Content.ReadFromJsonAsync<TodoItem>();
        updatedItem!.Name.Should().Be("Updated Workflow Item");
        updatedItem.IsComplete.Should().BeTrue();

        // Delete
        var deleteResponse = await _client.DeleteAsync("/api/items/700");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify Delete
        var getDeletedResponse = await _client.GetAsync("/api/items/700");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
