# DotNetty.Extensions Base On DotNetty

##### 
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
