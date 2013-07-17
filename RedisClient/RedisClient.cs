using System;
using System.Dynamic;
using System.Net.Sockets;
using System.Text;

namespace RedisClient
{
    public class RedisClient : DynamicObject, IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private TcpClient client;

        public RedisClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            client = new TcpClient();
            client.Connect(_host, _port);

            using (NetworkStream networkStream = client.GetStream())
            {
                int operationsCount = 1;
                operationsCount += args.Length;

                string redisOperationName = binder.Name;
                string operation = string.Format("*{0}\r\n" + "${1}\r\n{2}\r\n", operationsCount, redisOperationName.Length, redisOperationName);

                foreach (dynamic arg in args)
                {
                    string operationArgument = "";
                    if (arg is int || arg is bool || arg is char || arg is double || arg is float || arg is short || arg is long || arg is ulong || arg is ushort)
                    {
                        operationArgument = string.Format("${0}\r\n{1}\r\n", Math.Floor(Math.Log10(arg) + 1), arg);
                    }
                    else if (arg is string || arg is char[])
                    {
                        operationArgument = string.Format("${0}\r\n{1}\r\n", Encoding.ASCII.GetBytes((string)arg).Length, arg);
                    }

                    operation += operationArgument;
                }

                byte[] bytes = Encoding.ASCII.GetBytes(operation);
                networkStream.Write(bytes, 0, bytes.Length);
                networkStream.Flush();

                var buffer = new byte[100];
                int length = networkStream.Read(buffer, 0, 100);
                string status = Encoding.ASCII.GetString(buffer, 0, length);
                result = status;
                
                client.Close();
                
                return true;
            }
        }

        public void Dispose()
        {
            if (client != null && client.Connected)
                client.Close();
        }
    }
}