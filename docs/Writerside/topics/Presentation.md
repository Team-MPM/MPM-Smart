# Presentation

## Introduction

Good afternoon, dear teachers, colleagues and classmates. Today, we are proud to present to you, Mpm-Smart. 
The Smart Home revolution is coming for all of us. I am certain, that we all own a lot of different technology 
that aims at making our lives easier. But often, we find ourselves in situations where all those different systems
just don't work together. This is where Mpm-Smart comes in. Mpm-Smart is a Smart Home solution that aims to bring 
unify existing system together with new innovation in a single, easy to use, and open-source platform. 

## Solution

To accomplish this, we have developed a multi-layered architecture that is designed to be modular and extensible.
At the core of the system, we have a centralized Controller based on a solid plugin architecture. This controller
is responsible for managing all the devices, aggregate their data, and provide a unified interface to the user.

To make the world of extensible plugins more accessible to the every-day user we also ship a fully functional 
Plugin Registry. This registry allows users to easily discover, install, and manage plugins from a central location.
Power-users can also use the registry to publish their own plugins and share them with the community.

To maximize the user experience, we have also developed a modern and responsive web interface. Based on modern
technologies we implemented a Progressive Web App (PWA) that can be installed on literally any device and provides 
an accessible and easy to use interface to the user.

To also comply with modern security standards, we have implemented a full user management system with fine-grained
access control. We use modern encryption standards to ensure that all data is secure and private at any time.
All first party devices are also secured using mTLS to guarantee that only authorized devices can communicate with
the controller.

## Technologies

The Mpm-Smart solution is built on a variety of technologies. The core of the system is based on the .NET platform.
We use the latest version of the .NET 9 SDK and the Entity Framework Core for our backend. The frontend is built using
Blazor WebAssembly, a modern web framework that allows us to write C# code that runs natively in the browser.

For the device firmware, we use the ESP-IDF, a powerful framework for developing IoT devices based on the ESP32 chip.

To ensure the quality of our code, we use a variety of tools and services. We have integrated GitHub Actions into our
workflow to automatically run unit and integration tests, build and publish the server image, and publish the 
documentation. We also use code quality tools like cppcheck to ensure that our code is clean and maintainable.

We also leveraged many cloud servers including not ony Azure Redis Cache, Azure Blob Storage, and Azure SQL Database.

## Architecture

Frontend: Blazor WebAssembly WPA using the MudBlazor UI Framework

Controller: ASP.NET Core Web API using EF Core and Identity Framework

Plugin System: Dynamic assembly loading via reflection and inter-process communication

Plugin Registry: Azure Blob Storage and Azure Redis Cache and Azure SQL Database

## What is a plugin?

Plugin allow us to extend the core functionality of the system. A plugin is a self-contained piece of code that can
be dynamically loaded and executed by the controller. Plugins can be used to add new features, integrate with third-party
services, or interact with all sorts of hardware devices. 

These plugins can both manage their own resources like static assets, docker and lxc containers or even their own 
subprocesses and also share/integrate those resources with the core system and other plugins.

You want a plugin to collect data from your smart thermostat and display it in the web interface? No problem.
You want a plugin to control your smart lights based on the weather forecast? Easy.
You want a plugin to send you a notification when your laundry is done? Done.

## Our original Goals and Mockups

Olaf or Jake here you go!

## Little Demo

Quickly just show some of the nicer pages, probably no time for a full demo.

## Conclusion
 
You can check out our app right now under https://mpm-smart.g-martin.work or visit our GitHub repository at
https://github.com/Team-MPM/MPM-Smart/ and don't forget to leave a star! 