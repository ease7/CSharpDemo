using System;
using EasyNetQ;
using Newtonsoft.Json;

namespace RabbitMqDemo.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            //RabbitMqDemo2.Consume();
            //RabbitMqDemo3.Consume(new string[] { "test" });

            var rpcClient = new RpcClient();

            Console.WriteLine(" [x] Requesting fib(30)");
            var response = rpcClient.Call("30");
            Console.WriteLine(" [.] Got '{0}'", response);

            rpcClient.Close();

            Console.ReadLine();
        }

       
    }
}
