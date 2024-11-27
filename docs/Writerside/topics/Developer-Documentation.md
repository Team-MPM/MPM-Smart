# Developer Documentation

This document is intended for developers who want to contribute to the Writerside project. 
It provides an overview of the project's architecture, the technologies used, and the development process.

## Required Tools
- .NET 9 SDK
- Dotnet ef tools (For Entity Framework Core)
- Platform IO (For ESP32 development)
- JetBrains Writerside (Documentation)
- Docker
- A modern web browser
- Git
- Make

## Optional Tools
- Azd
- Rider
- Visual Studio Code
- cppcheck

## GitHub Actions

### Automatic Testing

The project uses GitHub Actions to run tests on every push/pr to the master branch.
These tests are mandatory and must pass before a PR can be merged.
When contributing new code, also try to achieve full test coverage where possible.

### Continuous Integration

The server image will be automatically built and published on push to the master branch. 
The documentation will be published on GitHub pages.

In the future, there will be also plans to automatically publish all the base packages on nuget and
build debian packages, including a full raspberry pi image.

### Code Quality

The project uses GitHub Actions to run code quality checks on every push/pr to the master branch.
These checks are mandatory and must pass before a PR can be merged.

## Getting the Code

To get the code, clone the repository:

```bash
git clone --recursive https://github.com/Team-MPM/MPM-Smart
```