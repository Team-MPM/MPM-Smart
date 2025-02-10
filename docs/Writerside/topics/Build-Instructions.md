# Build Instructions

To build the dotnet solution, open the solution file in the IDE of your choice or build it from
the command line using the `dotnet build` command. Node: Even when running only a specific project,
performing a full dotnet build on the solution first is recommended so that all the plugins and 
dependencies are built correctly the first time you run them locally.

To build the device firmware, use the esp-idf commandline took to build and flash the image.

To build the documentation as PDF, open the JetBrains Writerside workspace and build the documentation
using the included run configuration.

The Makefile in the root of the repository provides shortcuts for mose these commands. 

To build the debian packages and controller images from source, consult the `make help` command.
Note that this requires you to be on a linux system and have the specified dependencies installed.

To run the frontend in release mode locally, perform a `dotnet publish -c Release` on the frontend,
then navigate to the output directory based on `bin/Release/net9.0/publish/wwwroot` and run a local
webserver to serve the files in this directory (tested using apache2 and nginx). This can easily be 
achieved using docker: 
`docker run -it --rm -p 8080:80 --name mpm-smart-app -v .:/usr/local/apache2/htdocs:ro httpd`