# Architecture

## Introduction

The solution is generally organized into the following parts:
- Dotnet Solution **.sln**/**.slnx**
- Visual Studio Code Workspace **.code-workspace** for Platform IO

## Dotnet Solution Architecture

![solution_architecture](solution_architecture.png)

### Frontend

The frontend is responsible for rendering the user interface and handling user input.

The frontend is a Blazor WebAssembly application. It is written in C# and runs in the browser.
It makes use of the PWA (Progressive Web App) features to provide an offline-first experience.

We primarily make use of the following libraries:
- MudBlazor
- FluentValidation

### Backend

The backend is the primary application component running on the controller unit.
It is responsible for loading all the plugins and managing the communication between them.
It handles the communication between devices and acts as a bridge between the frontend and the plugins.

### Control Process

Manages the lifetime of the backend process and handles plugin updates and automatic restarts.

### Api Schema

Shared class library that contains the data transfer objects (DTOs) used by the frontend and the backend.

### Shared

Shared class library that contains the shared code between the backend and the plugin layer.

### Plugin Base

Base code and Interfaces for Plugins to reference.

### Data

Data Layer of the core Backend Application containing all EF Core Entities and the Database Context configurations.

### Server/ServerHost

THe Plugin Server providing Plugins for download via Azure Cloud Storage.
It also contains the documentation reference and a List of all Mpm-Smart devices.

### Tests

Solution folder containing all the Unit tests separated into the different layers.
There is also a **TestBase** class library that provides a unified testing experience. 
We primarily use XUnit for testing, FluentAssertion for assertions, and Moq for mocking.