# DotNetty.Extensions Base On DotNetty
# For Tcp、Udp、WebSocket Both Server And Client
##### 
### TcpServer
```c#
static void Main(string[] args)
{
    var server = new TcpSocketServer(8888);

    server.OnPipeline(pipeline =>
    {
        //心跳
        //pipeline.AddLast(new IdleStateHandler(5, 0, 0));

        //编码解码器
        //pipeline.AddLast(new LengthFieldPrepender(2));
        //pipeline.AddLast(new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

        //tls证书
        //var cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
        //pipeline.AddLast(TlsHandler.Server(cert));
    });

    server.OnStart(() =>
    {
        Console.WriteLine("服务启动成功");
    });

    server.OnConnectionConnect(conn =>
    {
        Console.WriteLine("OnConnectionConnect:" + conn.Id);
        Console.WriteLine("当前连接数:" + server.GetConnectionCount());
    });

    server.OnConnectionReceive((conn, bytes) =>
    {
        Console.WriteLine("OnConnectionReceive:" + bytes);
        conn.SendAsync(bytes);
    });

    server.OnConnectionException((conn, ex) =>
    {
        Console.WriteLine("OnConnectionException:" + ex);
    });

    server.OnConnectionClose(conn =>
    {
        Console.WriteLine("OnConnectionClose:" + conn.Id);
        Console.WriteLine("当前连接数:" + server.GetConnectionCount());
    });

    server.OnStop(ex =>
    {
        Console.WriteLine(ex);
        //restart
        //server.StartAsync();
    });

    server.StartAsync();

    Console.ReadKey();
}

```

### TcpClient
```c#
static void Main(string[] args)
{
    var client = new TcpSocketClient("127.0.0.1", 8888);

    client.OnPipeline(pipeline =>
    {
        //心跳
        //pipeline.AddLast(new IdleStateHandler(5, 0, 0));

        //编码解码器
        //pipeline.AddLast(new LengthFieldPrepender(2));
        //pipeline.AddLast(new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

        //tls证书
        //var cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
        //var targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
        //pipeline.AddLast(new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));

    });

    client.OnConnect(() =>
    {
        Console.WriteLine("OnConnect");
        var bytes = Encoding.UTF8.GetBytes("Hello Word");
        client.SendAsync(bytes);
    });

    client.OnReceive(bytes =>
    {
        Console.WriteLine("OnReceive:" + bytes);
    });

    client.OnException(ex =>
    {
        Console.WriteLine("OnException:" + ex);

    });

    client.OnClose(ex =>
    {
        Console.WriteLine("OnClose:" + ex);
        //restart
        //client.ConnectAsync();
    });

    client.ConnectAsync();

    Console.ReadKey();
}
```

### UDP
```c#
static void Main(string[] args)
{
    var udp = new UdpSocket(8888);

    udp.OnStart(() =>
    {
        Console.WriteLine("UDP服务启动8888");
    });

    udp.OnRecieve((endPoint, bytes) =>
    {
        Console.WriteLine(endPoint);
        Console.WriteLine(Encoding.UTF8.GetString(bytes));
        udp.SendAsync(endPoint, bytes);
    });

    udp.OnException(ex =>
    {
        Console.WriteLine(ex);
    });

    udp.OnStop(ex =>
    {
        Console.WriteLine("Close:" + ex);
        //restart
        //udp.StartAsync();
    });

    udp.StartAsync();

    Console.ReadKey();
}
```

### WebSocketServer
```c#
static void Main(string[] args)
{
    var server = new WebSocketServer(8888);

    server.OnPipeline(pipeline =>
    {
        //心跳
        //pipeline.AddLast(new IdleStateHandler(5, 0, 0));

        //tls证书
        //var cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
        //pipeline.AddLast(TlsHandler.Server(cert));

    });

    server.OnStart(() =>
    {
        Console.WriteLine("服务启动成功");
    });

    server.OnConnectionConnect(conn =>
    {
        Console.WriteLine("OnConnectionConnect:" + conn.Id);
        Console.WriteLine("当前连接数:" + server.GetConnectionCount());
        conn.SendTextAsync("嘿,欢迎来到服务器");
    });

    server.OnConnectionReceiveText((conn, text) =>
    {
        Console.WriteLine("OnConnectionReceiveText:" + text);
    });

    server.OnConnectionReceiveBinary((conn, bytes) =>
    {
        Console.WriteLine("OnConnectionReceiveBinary:" + bytes);
    });

    server.OnConnectionException((conn, ex) =>
    {
        Console.WriteLine("OnConnectionException:" + ex);
    });

    server.OnConnectionClose(conn =>
    {
        Console.WriteLine("OnConnectionClose:" + conn.Id);
        Console.WriteLine("当前连接数:" + server.GetConnectionCount());
    });

    server.OnStop(ex =>
    {
        Console.WriteLine(ex);
        //restart
        //server.StartAsync();
    });

    server.StartAsync();

    Console.ReadKey();
}
```

### WebSocketClient
```c#
static void Main(string[] args)
{
    var client = new WebSocketClient("ws://127.0.0.1:8888");

    client.OnPipeline(pipeline =>
    {
        //心跳
        //pipeline.AddLast(new IdleStateHandler(5, 0, 0));

        //tls证书
        //var cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
        //var targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
        //pipeline.AddLast(new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));

    });

    client.OnConnect(() =>
    {
        Console.WriteLine("OnConnect");
        client.SendTextAsync("Hello Word");
        client.SendBinaryAsync(new byte[] { 1 });

    });

    client.OnReceiveText(text =>
    {
        Console.WriteLine("OnReceiveText:" + text);
    });

    client.OnReceiveBinary(bytes =>
    {
        Console.WriteLine("OnReceiveBinary:" + bytes);
    });

    client.OnException(ex =>
    {
        Console.WriteLine("OnException:" + ex);

    });

    client.OnClose(ex =>
    {
        Console.WriteLine("OnClose:" + ex);
        //restart
        client.ConnectAsync();
    });

    client.ConnectAsync();


    Console.ReadKey();
}
```

