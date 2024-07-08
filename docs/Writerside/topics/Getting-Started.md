# Getting Started

# Getting Started

This guide will walk you through setting up your development environment for both the C# server project and the 
C/C++-based codebase for IoT devices. Focus on the parts relevant to you.

## System Requirements
- **OS**: Windows 10+
- **RAM**: At least 16GB of RAM, I would say, if you want to run the entire cloud stack locally, 32GB is the minimum!
- **CPU**: A kinda decent CPU is recommended 
- **Storage**: If you don't have the dependencies installed, you will need around 20–100GB of free space for the dependencies. The project itself is not that big (yet).

## Prerequisites
- **Windows Subsystem for Linux ([WSL](https://learn.microsoft.com/en-us/windows/wsl/install))**: Ensure WSL is installed on your Windows machine for a Linux-compatible environment.
  - Make sure you have the latest WSL2 kernel installed.
  - Set WSL 2 as the default version.
  - Install a Linux distribution from the Microsoft Store. (Debian-based distributions are recommended to get started)
  - Make sure you can access wsl from the terminal! (try running `wsl` in the terminal)
  - Get familiar with the WSL integration with Windows (file system, networking, etc.) 
    - `/mount/c/` is the path to your window C: drive in WSL.
    - You can also access wsl files via File Explorer at `\\wsl$`.
    - You can run linux commands in windows using `wsl <command>`. (try: `wsl ls`)
    - You can run windows commands in WSL using the .exe extension (try: `notepad.exe <file>`).
    - You can also pipe between systems, for example, using wsl grep on windows ipconfig output: `ipconfig | wsl grep IPv4`.
    - This is not a WSL tutorial, but knowing these will help you work with WSL more efficiently!!!
- **Docker**: Required for containerization of the application.
  - Install Docker Desktop for Windows.
  - Make sure your WSL integration is set up correctly.
  - Familiarize yourself with Docker commands and containerization concepts!!! (Dockerfile, docker-compose, docker build/run, etc.)
  - Enable Kubernetes in Docker Desktop for local Kubernetes cluster support. (Only needed if you plan to deploy the app locally)
- **.NET 8 SDK**: Necessary for building and running the C# project.
  - [Download and install .NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
  - Verify the installation by running `dotnet --version` in the terminal.
  - Install workloads and tools
    ```PowerShell
    dotnet workload update
    dotnet workload install maui aspire
    dotnet tool install -g dotnet-ef
    ```
- **Rider/CLion IDEs**: Recommended for development, but VS, VSCode and everything else works too.
- **Postman/JetClient**: Some software for testing Api endpoints is recommended.
- **Arduino IDE**: Even when using CLion, I still recommend having the Arduino IDE installed for quick testing and debugging. 

