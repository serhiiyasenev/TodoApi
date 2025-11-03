using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TodoApi.Models;

namespace TodoApi.IntegrationTests;

public class TodoValuesControllerIntegrationTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<int> CreateTestItemAsync(string name = "Test Item")
    {
        var item = new TodoItem { Id = Random.Shared.Next(10000, 99999), Name = name, IsComplete = false };
        var response = await _client.PostAsJsonAsync("/api/items", item);
        response.EnsureSuccessStatusCode();
        return item.Id;
    }

    [Fact]
    public async Task GetAllItemValuesById_WithValidItemId_ReturnsOk_WithValues()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item with values");
        var value1 = new TodoItemValue { Id = Random.Shared.Next(10000, 99999), Value = 10, TodoItemId = itemId };
        var value2 = new TodoItemValue { Id = Random.Shared.Next(10000, 99999), Value = 20, TodoItemId = itemId };

        await _client.PostAsJsonAsync($"/api/items/{itemId}/values", value1);
        await _client.PostAsJsonAsync($"/api/items/{itemId}/values", value2);

        // Act
        var response = await _client.GetAsync($"/api/items/{itemId}/values");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var values = await response.Content.ReadFromJsonAsync<List<TodoItemValue>>();
        values.Should().NotBeNull();
        values.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllItemValuesById_WithInvalidItemId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/items/99999/values");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetItemValueByIdAndValueId_WithValidIds_ReturnsOk_WithValue()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for value lookup");
        var valueId = Random.Shared.Next(10000, 99999);
        var value = new TodoItemValue { Id = valueId, Value = 42, TodoItemId = itemId };

        await _client.PostAsJsonAsync($"/api/items/{itemId}/values", value);

        // Act
        var response = await _client.GetAsync($"/api/items/{itemId}/values/{valueId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedValue = await response.Content.ReadFromJsonAsync<int>();
        returnedValue.Should().Be(42);
    }

    [Fact]
    public async Task GetItemValueByIdAndValueId_WithInvalidItemId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/items/99999/values/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetItemValueByIdAndValueId_WithInvalidValueId_ReturnsNotFound()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item without specific value");

        // Act
        var response = await _client.GetAsync($"/api/items/{itemId}/values/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddValue_WithValidItemValue_ReturnsCreated_WithValues()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for adding value");
        var valueId = Random.Shared.Next(10000, 99999);
        var newValue = new TodoItemValue { Id = valueId, Value = 99, TodoItemId = itemId };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/items/{itemId}/values", newValue);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var values = await response.Content.ReadFromJsonAsync<List<TodoItemValue>>();
        values.Should().NotBeNull();
        values.Should().Contain(v => v.Id == valueId && v.Value == 99);
    }

    [Fact]
    public async Task AddValue_WithNullValue_ReturnsBadRequest()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for null value test");

        // Act
        var response = await _client.PostAsJsonAsync<TodoItemValue?>($"/api/items/{itemId}/values", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddValue_WithInvalidItemId_ReturnsNotFound()
    {
        // Arrange
        var newValue = new TodoItemValue { Id = 1234, Value = 10, TodoItemId = 99999 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/items/99999/values", newValue);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateValue_WithValidValue_ReturnsOk_WithMessage()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for update value");
        var valueId = Random.Shared.Next(10000, 99999);
        var originalValue = new TodoItemValue { Id = valueId, Value = 50, TodoItemId = itemId };

        await _client.PostAsJsonAsync($"/api/items/{itemId}/values", originalValue);

        var updatedValue = new TodoItemValue { Id = valueId, Value = 75, TodoItemId = itemId };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/items/{itemId}/values", updatedValue);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsStringAsync();
        message.Should().Contain("75");
        message.Should().Contain(valueId.ToString());
    }

    [Fact]
    public async Task UpdateValue_WithNonExistentValue_CreatesNewValue()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for creating value via update");
        var newValueId = Random.Shared.Next(10000, 99999);
        var newValue = new TodoItemValue { Id = newValueId, Value = 123, TodoItemId = itemId };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/items/{itemId}/values", newValue);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task UpdateValue_WithNullValue_ReturnsBadRequest()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for null update test");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/items/{itemId}/values", (TodoItemValue?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateValue_WithInvalidItemId_ReturnsNotFound()
    {
        // Arrange
        var value = new TodoItemValue { Id = 1234, Value = 10, TodoItemId = 99999 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/items/99999/values", value);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteItemValueById_WithValidIds_ReturnsOk_WithMessage()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for delete value");
        var valueId = Random.Shared.Next(10000, 99999);
        var value = new TodoItemValue { Id = valueId, Value = 33, TodoItemId = itemId };

        await _client.PostAsJsonAsync($"/api/items/{itemId}/values", value);

        // Act
        var response = await _client.DeleteAsync($"/api/items/{itemId}/values/{valueId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsStringAsync();
        message.Should().Contain("deleted");
        message.Should().Contain(valueId.ToString());
    }

    [Fact]
    public async Task DeleteItemValueById_WithInvalidItemId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/items/99999/values/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteItemValueById_WithInvalidValueId_ReturnsNotFound()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for invalid value delete");

        // Act
        var response = await _client.DeleteAsync($"/api/items/{itemId}/values/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAllItemValues_WithValidItemId_ReturnsOk_WithMessage()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item for delete all values");
        var value1 = new TodoItemValue { Id = Random.Shared.Next(10000, 99999), Value = 11, TodoItemId = itemId };
        var value2 = new TodoItemValue { Id = Random.Shared.Next(10000, 99999), Value = 22, TodoItemId = itemId };

        await _client.PostAsJsonAsync($"/api/items/{itemId}/values", value1);
        await _client.PostAsJsonAsync($"/api/items/{itemId}/values", value2);

        // Act
        var response = await _client.DeleteAsync($"/api/items/{itemId}/values");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsStringAsync();
        message.Should().Contain("All values");
        message.Should().Contain("deleted");
    }

    [Fact]
    public async Task DeleteAllItemValues_WithInvalidItemId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/items/99999/values");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAllItemValues_WithNoValues_ReturnsNotFound()
    {
        // Arrange
        var itemId = await CreateTestItemAsync("Item with no values");

        // Act
        var response = await _client.DeleteAsync($"/api/items/{itemId}/values");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FullValueWorkflow_CreatesUpdatesAndDeletesValue()
    {
        // Create item
        var itemId = await CreateTestItemAsync("Item for value workflow");
        var valueId = Random.Shared.Next(10000, 99999);

        // Add value
        var newValue = new TodoItemValue { Id = valueId, Value = 100, TodoItemId = itemId };
        var addResponse = await _client.PostAsJsonAsync($"/api/items/{itemId}/values", newValue);
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get value
        var getResponse = await _client.GetAsync($"/api/items/{itemId}/values/{valueId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var value = await getResponse.Content.ReadFromJsonAsync<int>();
        value.Should().Be(100);

        // Update value
        var updatedValue = new TodoItemValue { Id = valueId, Value = 200, TodoItemId = itemId };
        var updateResponse = await _client.PutAsJsonAsync($"/api/items/{itemId}/values", updatedValue);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify update
        var getUpdatedResponse = await _client.GetAsync($"/api/items/{itemId}/values/{valueId}");
        var updatedValueResult = await getUpdatedResponse.Content.ReadFromJsonAsync<int>();
        updatedValueResult.Should().Be(200);

        // Delete value
        var deleteResponse = await _client.DeleteAsync($"/api/items/{itemId}/values/{valueId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify delete
        var getDeletedResponse = await _client.GetAsync($"/api/items/{itemId}/values/{valueId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
