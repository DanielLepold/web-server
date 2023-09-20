# Web-server in fsharp
This is a simple web server implementation in fsharp for the following purposes:
- concurrent client handling
- restrict file access
- handling arguments in main

## Concurrent Client Handling
The primary objective was to implement concurrent client handling.
This means that this server is able to accept multiple incoming client connections
and manages them concurrently without blocking one another.

To test the multiple threading, I added 4 sec delay after a request is arrived.

## Restrict file access
The logic behind is to check whether the requested path provided by the client falls within the "www" directory or its subdirectories.
If the requested path is outside of this designated area, I return a 404 Not Found response.
This restriction ensures that only authorized files are served.

## Handling arguments in main
When developing console applications, it's often necessary to pass arguments
or configuration parameters to the program.
In F# and many other programming languages,
this is commonly done by handling command-line arguments in the main function.

## Usage
To start the application use the following syntax in the folder of the webserver.exe file:
- ./webserver root

I added the "www" directory content to my "root" folder.

But you can add any public www directory you like:
Start the server with your own "www" directory located at for example /var/www/public
- ./webserver /var/www/public

After the application is running, you can reach and
test the requests for example with the following commands:

- curl http://localhost:8080/index.html - 200 Ok response
- curl http://localhost:8080/hello - 404 Not found response

For representation I just added a simple web page into the index.html file.
