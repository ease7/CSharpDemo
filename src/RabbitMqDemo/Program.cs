using System;
using EasyNetQ;
using Newtonsoft.Json;

namespace RabbitMqDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //RabbitMqDemo1.Publish(args);
            //RabbitMqDemo2.Publish(args);

            //RabbitMqDemo3.Publish(new string[] { "test" });

            //RabbitMqDemo4.Server();

            // 发布者确认
            RabbitMqDemo7.Start();
        }
    }
}
