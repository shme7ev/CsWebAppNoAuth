# WebAppNoAuth Integration Tests

This project contains comprehensive integration tests for the JWT authentication functionality in the WebAppNoAuth application.

## Test Coverage

### JwtAuthenticationTests.cs
Tests the core JWT authentication functionality:
- Unauthenticated requests to protected endpoints return 401
- Invalid JWT tokens are rejected
- Valid JWT tokens grant access to protected endpoints
- Token generation endpoint works without authentication
- Various edge cases for login (empty/whitespace usernames)
- Admin dashboard displays product data correctly
- Public home page is accessible without authentication
- Multiple valid requests work with the same token
- Anonymous endpoints allow access without token

### JwtTokenServiceTests.cs
Tests the JWT token service directly:
- Token service can be resolved from DI container
- Valid tokens are created for various username formats
- Generated tokens are unique
- Tokens contain expected expiration times
- Tokens can be validated by the same key
- Service handles concurrent token generations
- Token length is reasonable

## Running the Tests

```bash
# Run all tests
dotnet test WebAppNoAuth.IntegrationTests

# Run tests with verbose output
dotnet test WebAppNoAuth.IntegrationTests -v normal

# Run tests without rebuilding (faster subsequent runs)
dotnet test WebAppNoAuth.IntegrationTests --no-build

# Run specific test class
dotnet test WebAppNoAuth.IntegrationTests --filter "FullyQualifiedName~JwtAuthenticationTests"

# Run specific test method
dotnet test WebAppNoAuth.IntegrationTests --filter "FullyQualifiedName=WebAppNoAuth.IntegrationTests.JwtAuthenticationTests.Valid_JWT_Token_Grants_Access_To_Protected_Endpoint"
```

## Test Categories

### Authentication Tests
- **401 Unauthorized responses** for unauthenticated requests
- **Token validation** for malformed or invalid tokens
- **Successful authentication** with valid tokens
- **Anonymous access** where permitted

### Functional Tests
- **Token generation** through web forms
- **User interface** rendering and navigation
- **Data display** in protected areas
- **Form validation** and error handling

### Edge Case Tests
- **Empty/whitespace inputs**
- **Special characters** in usernames
- **Concurrent requests**
- **Multiple authentication scenarios**

## Test Constants

The `TestConstants.cs` file provides shared constants for:
- Endpoint URLs
- HTTP status codes
- HTTP headers
- Content types
- JWT-related values

## Custom Test Factory

The `CustomWebApplicationFactory.cs` provides a configurable test environment that:
- Sets the environment to "Testing"
- Allows service overrides for test scenarios
- Provides consistent test setup across all test classes


## Prerequisites

- .NET 10.0 SDK
- Running PostgreSQL database (configured in appsettings.json)
- Main WebAppNoAuth application builds successfully

## Continuous Integration

These tests can be integrated into CI/CD pipelines to ensure:
- Authentication continues to work after code changes
- No regressions in protected endpoint behavior
- Token generation and validation remain functional
- User experience is maintained

To get a token:

curl -X POST http://localhost:5033/Admin/Login -H "Content-Type: application/x-www-form-urlencoded" -d "username=testuser"

To use the token:

curl -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdHVzZXIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6InRlc3R1c2VyIiwianRpIjoiMWQwMDI5ZmItY2IzYy00YjIzLWJlNzAtNmViZTllMzIwYTkxIiwiZXhwIjoxNzcxNzcyMzUzLCJpc3MiOiJXZWJBcHBOb0F1dGgiLCJhdWQiOiJXZWJBcHBOb0F1dGhVc2VycyJ9.LCJMzba_9GckuNYoVPxfm7AguHhRW9j4-qRuEsuY-UE" http://localhost:5033/Admin

To run tests with env variable:

appsettings.Testing.json: "DefaultConnection": ${TEST_DATABASE_CONNECTION_STRING}

TEST_DATABASE_CONNECTION_STRING="Host=localhost;Database=test_db;Username=testuser;Password=testpass" dotnet test

