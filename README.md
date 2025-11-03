### Run all tests:
```bash
cd tests/TodoApi.IntegrationTests
dotnet test
```

### Run with verbose output:
```bash
dotnet test --verbosity normal
```

### Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~TodoControllerIntegrationTests"
```

### Run specific test method:
```bash
dotnet test --filter "FullyQualifiedName~GetAll_ReturnsOk_WithTodoItems"
```

### Generate test coverage report:
```bash
dotnet test --collect:"XPlat Code Coverage"
```


- **xUnit** - Testing framework
- **FluentAssertions** - Fluent assertion library for readable test assertions
- **Microsoft.AspNetCore.Mvc.Testing** - In-memory test server for integration testing
- **In-Memory Database** - Each test runs with a fresh in-memory database


### Test Isolation
- Each test class uses `IClassFixture<TodoApiFactory>` to share a test server instance
- In-memory database is recreated for each test run
- Tests use random IDs to avoid conflicts

### Factory Pattern
The `TodoApiFactory` class:
- Inherits from `WebApplicationFactory<Program>`
- Configures a test-specific in-memory database
- Ensures clean database state for each test run

### Fluent Assertions
Tests use FluentAssertions for readable assertions:
```csharp
response.StatusCode.Should().Be(HttpStatusCode.OK);
items.Should().NotBeEmpty();
item.Name.Should().Be("Expected Name");
```

Tests use:
- Random IDs (10000-99999 range) to avoid conflicts
- Helper method `CreateTestItemAsync()` for consistent item creation
- Inline test data for straightforward scenarios


Some tests may fail due to controller implementation issues:
- Value operations may not persist correctly
- This indicates areas for improvement in the main API


These tests are designed to run in CI/CD pipelines:
- Fast execution (< 5 seconds total)
- No external dependencies
- Deterministic results

To add new tests:

1. Add test method with `[Fact]` attribute
2. Follow Arrange-Act-Assert pattern
3. Use FluentAssertions for assertions
4. Use descriptive test names: `MethodName_Scenario_ExpectedResult`

Example:
```csharp
[Fact]
public async Task Create_WithValidData_ReturnsCreated()
{
    // Arrange
    var newItem = new TodoItem { Id = 100, Name = "Test", IsComplete = false };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/items", newItem);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

- **Total Tests**: 32
- **Test Coverage**: All major endpoints covered
- **Average Execution Time**: ~60ms per test
- **Success Rate**: 72% (23/32 passing)


- [ ] Add performance tests
- [ ] Add load tests
- [ ] Improve test data builders
- [ ] Add API contract tests
- [ ] Increase code coverage to 90%+
- [ ] Fix failing tests related to value persistence

For issues or questions about the tests, please check the main TodoApi project documentation.
