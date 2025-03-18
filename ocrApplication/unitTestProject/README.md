# OCR Unit Test Project

## Overview
This project contains comprehensive unit and integration tests for the OCR Application Suite, ensuring the reliability, accuracy, and performance of both the command-line application and GUI components. The test suite validates core OCR functionality, image preprocessing, text similarity analysis, and user interface components.

## Key Features
- Extensive test coverage for OCR functionality
- Integration tests for end-to-end workflows
- Mock objects for isolated component testing
- GUI component testing
- Performance validation

## Test Categories

### OCR Component Tests
- **OcrExtractionToolsTests**: Validates core OCR extraction functionality
- **EnsembleOcrTests**: Tests ensemble OCR approach and result combination
- **TextSimilarityTests**: Validates text comparison and similarity metrics

### Image Processing Tests
- **ImagePreprocessingTests**: Tests various image preprocessing techniques

### GUI Component Tests
- **GuiTests**: Basic GUI functionality tests
- **GuiComponentTests**: Detailed GUI component testing
- **ViewButtonsTests**: Tests button functionality in the UI
- **ThreeViewButtonsTests**: Tests specific view button implementations

### Integration Tests
- **IntegrationTests**: End-to-end workflow testing

## Installation and Setup

### Prerequisites
- .NET 8.0 SDK and runtime
- MSTest framework
- Moq mocking library

### Dependencies
The test project relies on the following key packages:
- MSTest.TestAdapter (3.2.0)
- MSTest.TestFramework (3.2.0)
- Microsoft.NET.Test.Sdk (17.9.0)
- Moq (4.20.70) - Mocking framework
- coverlet.collector (6.0.0) - Code coverage tool
- Emgu.CV (4.9.0.5494) and related packages - For image processing tests
- EPPlus (7.6.0) - For Excel export testing

### Configuration
The tests use a test-specific configuration file (`ocr_config.json`) that's copied to the output directory during build.

## Running the Tests

### Run All Tests
```bash
cd unitTestProject
dotnet test
```

### Run Specific Test Category
```bash
dotnet test --filter "Category=OCR"
dotnet test --filter "Category=GUI"
dotnet test --filter "Category=Integration"
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Test Structure

### 1. Unit Tests
Individual components are tested in isolation:
- Input validation
- Core functionality
- Edge cases
- Error handling

### 2. Integration Tests
Tests components working together:
- OCR workflow from image to text
- Preprocessing pipeline
- Export functionality

### 3. Mock Objects
The `Mocks` directory contains mock implementations for:
- OCR engines
- File systems
- External services

### 4. Helper Methods
The `TestHelpers.cs` file provides common functionality for:
- Test image generation
- Result comparison
- Test environment setup

## Adding New Tests

### Steps to Add a New Test Class
1. Create a new test class file in the appropriate category
2. Add the MSTest attributes (`[TestClass]`, `[TestMethod]`, etc.)
3. Implement test methods following the AAA pattern (Arrange, Act, Assert)
4. Run the tests to verify

### Example Test Structure
```csharp
[TestClass]
public class NewFeatureTests
{
    [TestInitialize]
    public void Setup()
    {
        // Setup code
    }

    [TestMethod]
    [TestCategory("Category")]
    public void FeatureMethod_Scenario_ExpectedResult()
    {
        // Arrange
        var input = // ...

        // Act
        var result = // ...

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Cleanup code
    }
}
```

## Best Practices

### For Writing Tests
- Use descriptive test names (MethodName_Scenario_ExpectedResult)
- Follow the AAA pattern (Arrange, Act, Assert)
- Use test categories for organization
- Keep tests independent and isolated
- Use appropriate assertions

### For Test Maintenance
- Update tests when functionality changes
- Review test coverage regularly
- Address failing tests promptly
- Refactor tests to reduce duplication

## Troubleshooting

### Common Issues
- **Test Discovery Issues**: Make sure test classes have `[TestClass]` attribute and methods have `[TestMethod]` attribute
- **Path Issues**: Ensure test data files use relative paths
- **Environment Dependencies**: Mock external dependencies when possible
- **Platform-Specific Issues**: Use conditional compilation for platform-specific tests

### Debugging Tests
- Use Visual Studio or VS Code for step-by-step debugging
- Add `Console.WriteLine` statements for debugging information
- Run tests individually to isolate issues

## Continuous Integration
The test suite is designed to run in CI/CD pipelines to ensure code quality:
- Automatic test execution on commits
- Code coverage reports
- Test result visualization

## Future Enhancements
- Additional performance benchmarking tests
- Expanded GUI testing coverage
- Stress tests for large image datasets
- Internationalization testing 