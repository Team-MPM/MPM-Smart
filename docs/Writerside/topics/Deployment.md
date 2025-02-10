# Deployment

## Deploying a Controller

To deploy a custom controller instance, make sure you have a linux based machine with supported
hardware configuration (see [Hardware Requirements](https://harware-requirement)).

You can use the included makefile to build the debian package and controller image. 
Alternatively you can also build the app from source manually using `dotnet build` and copy over the files manually.
To make sure the controller software starts automatically on boot, you can use the included systemd service file.

After following these steps you should be able to connect to your controller in the web ui 
and undergo the guided setup process.

## Deploying the plugin server

The server host project, based on dotnet aspire allows easy deployment no matter what environment you are using.
Use `dotnet publish` on the server host to automatically generate the manifest. This manifest can be used
for automatic incremental deployments. 

For example, to deploy to azure, one can simply use the azd commandline tool with 
- `azd init`
- `azd up`
- `azd publish`

Alternatively you can also use other tools like aspirate for custom kubernetes deployments.

Also make sure all the azure based cloud resources are set up accordingly and all connection strings
are set via user storage or appsettings.json.

## Deploying the Frontend

To deploy the frontend, use the `dotnet publish` command inside `src/frontend`.
Then navigate to the build outputs `wwwroot` directory and deploy this to a webserver of your choice.
YOu can, for example, run a local docker container: `docker run -it --rm -p 8080:80 --name mpm-smart-app -v .:/usr/local/apache2/htdocs:ro httpd`