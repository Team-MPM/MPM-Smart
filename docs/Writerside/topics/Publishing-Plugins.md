# Publishing Plugins

## Create plugin package

You can use the builtin `dotnet publish` command to create the plugin package file.
Make sure the .csproj file is configured to include all additional assets and resources needed 
in order for the plugin to work.

## Register plugin package

Go to the [MPM-Smart Plugin Server](https://mpm-smart.g-martin.work) 
make sure you are signed in and register your plugin package.

## Upload plugin package

Use the upload button to upload the built package file. It will be automatically validated to 
conform to the structure guidelines outlined in the plugin overview.
