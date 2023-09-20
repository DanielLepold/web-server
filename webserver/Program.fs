open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

let getRequestPath (requestLine: string) =
    let parts = requestLine.Split [|' '|]
    if parts.Length >= 2 then
        parts.[1]
    else
        "/"

let isPathValid (requestedPath: string) (wwwDirectory: string) =
    let fullRequestedPath = Path.Combine(wwwDirectory, requestedPath.TrimStart('/'))
    fullRequestedPath.StartsWith(wwwDirectory) && File.Exists(fullRequestedPath)

let asyncHandleClient (client: TcpClient) wwwDirectory =
    async {
        let threadId = Thread.CurrentThread.ManagedThreadId
        Console.WriteLine($"Thread {threadId} is handling the client.")

        let stream = client.GetStream()
        let reader = new StreamReader(stream)
        let writer = new StreamWriter(stream)

        try
            let requestLine = reader.ReadLine()
            match requestLine with
            | null ->
                Console.WriteLine("Received an empty request.")
            | _ ->
                // Simulate a 4-second delay
                do! Async.Sleep(TimeSpan.FromSeconds(4.0))

                let path = getRequestPath requestLine
                match isPathValid path wwwDirectory with
                | true ->
                    let fullPath = Path.Combine(wwwDirectory, path.TrimStart('/'))
                    let contentType =
                        if fullPath.EndsWith(".html") then "text/html"
                        else if fullPath.EndsWith(".txt") then "text/plain"
                        else "application/octet-stream"  // Default content type

                    let response =
                        let content = File.ReadAllText(fullPath)
                        $"HTTP/1.1 200 OK\r\nContent-Type: {contentType}\r\n\r\n{content}"

                    Console.WriteLine($"Thread {threadId} is sending the response back to the client... {response}.")
                    writer.WriteLine(response)
                | false ->
                    let notFoundResponse = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n404 - Not Found"
                    Console.WriteLine($"Thread {threadId} is sending a 404 response... {notFoundResponse}.")
                    writer.WriteLine(notFoundResponse)

                writer.Flush()
        with
            | ex -> Console.WriteLine($"An error occurred on thread {threadId}: {ex.Message}")

        Console.WriteLine($"Thread {threadId} is closing the client...")
        client.Close()
    }

[<EntryPoint>]
let main args =
    match args.Length  with
    | 1 ->
        let wwwDirectory = args.[0]

        match (Directory.Exists wwwDirectory) with
        | false ->
            Console.WriteLine("The specified www directory does not exist.")
        | true ->
            let ipAddress = IPAddress.Parse("127.0.0.1")
            let port = 8080
            let listener = new TcpListener(ipAddress, port)
            listener.Start()
            Console.WriteLine($"Server is listening on port: {port}...")

            while true do
                let client = listener.AcceptTcpClient()
                async {
                    // Handle each client asynchronously using Async.StartChild
                    let! _ = Async.StartChild(asyncHandleClient client wwwDirectory)
                    return ()
                } |> Async.Start
    | _ ->
      Console.WriteLine("Usage: webserver.exe <www-directory>")

    0 // Exit code
