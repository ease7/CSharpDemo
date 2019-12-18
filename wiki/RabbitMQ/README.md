## 1. 队列命名规范

{项目名称}.{模块名称}.{其他}

## 2. 消息模型

### 2.1 Simple Queue （简单队列）

消息的生产者负责生产消息，消费者负责处理消息，在下图种，“P”是我们的生产者，“C”是我们的消费者。中间的框是一个队列-RabbitMQ代表使用者保留的消息缓冲区。

![SimpleQueue](imgs/001.png)

消息发送代码范例

``` C#
public static void StartBasicPublish()
{
    var factory = new ConnectionFactory()
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672/")
    };
    using (var connection = factory.CreateConnection())
    {
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
  
            string message = "Hello World!";
            var body = Encoding.UTF8.GetBytes(message);
  
            channel.BasicPublish(exchange: "",
                                 routingKey: "hello",
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine(" [x] Sent {0}", message);
        }
    }
  
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}
```

消息接收

``` C#
public static void StartConsumer()
{
    var factory = new ConnectionFactory()
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672/")
    };
    using (var connection = factory.CreateConnection())
    {
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
  
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
            };
            channel.BasicConsume(queue: "hello",
                                 autoAck: true,
                                 consumer: consumer);
  
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
  
}
```

### 2.2 Work Queue(工作队列)

![工作队列图示](imgs/002.png)

工作队列的主要思想是避免在执行密集型任务而不得不等待所有得任务完成。我们安排任务在以后完成，我们将任务封装为消息并将其发送到队列，后台运行得工作进程将接收任务消息并最终执行任务作业。当运行许多任务消费者时，任务将在他么之间共享。

这个概念在Web应用程序中特别有用，因为在Web应用程序中，不可能在较短的HTTP请求内处理复杂的任务。

工作队列与简单队列不同的是，我们启动了多个消费者去处理消息，这种消息模式是用得最多的一种模型，这里就不再放范例代码了。

默认情况下，RabbitMQ将每个消息一次发送给下一个消费者，平均而言，每个消费者都会收到相同数量的消息。

#### 消息确认

如果其中一个消费者者开始一项漫长的任务并仅部分完成而死掉，会发生什么。使用我们当前的代码，RabbitMQ一旦向消费者发送了一条消息，便立即将其标记为删除。在这种情况下，如果您杀死一个消费者，我们将丢失正在处理的消息，我们还将丢失所有发送给该特定消费者但尚未处理的消息。

但是我们不想丢失任何任务，如果一个消费者死亡，我们希望将任务交付给另一个消费者。

为了确保消息永不丢失，RabbitMQ支持 消息确认。消费者发送回一个确认（告知），告知RabbitMQ特定的消息已被接收，处理，并且RabbitMQ可以自由删除它。

如果消费者死了（其通道已关闭，连接已关闭或TCP连接丢失）而没有发送确认，RabbitMQ将了解消息未完全处理，并将重新排队。如果同时有其他消费者在线，它将很快将其重新分发给另一个消费者。这样，您可以确保即使工人偶尔死亡也不会丢失任何消息。

没有任何消息超时；消费者死亡时，RabbitMQ将重新传递消息。即使处理一条消息花费非常非常长的时间也没关系。

默认情况下，手动消息确认处于打开状态。在前面的示例中，我们通过将autoAck（“自动确认模式”）参数设置为true来明确关闭它们。完成任务后，是时候删除此标志并手动从工作程序发送适当的确认了。

``` C#
public static void StartConsumer()
{
    var factory = new ConnectionFactory()
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672/")
    };
    using (var connection = factory.CreateConnection())
    {
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "hello",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
  
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
  
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                Console.WriteLine(" [x] Received {0}", message);
            };
            channel.BasicConsume(queue: "hello", autoAck: false, consumer: consumer);
  
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
  
}
```
使用此代码，我们可以确保，即使您在处理消息时使用CTRL + C杀死工作人员，也不会丢失任何信息。工人死亡后不久，所有未确认的消息将重新发送。

TIPS：使用手动确认模式时，一定不要忘记确认。

#### 队列消息持久化

手动确认可以确保即使消费者死亡，任务也不会丢失。但是，如果RabbitMQ服务器停止，我们的任务仍然会丢失。

RabbitMQ退出或崩溃时，它将忘记队列和消息，除非您告知不要这样做。要确保消息不会丢失，需要做两件事：我们需要将队列和消息都标记为持久。

首先，我们需要确保RabbitMQ永远不会丢失我们的队列。为此，我们需要将其声明为持久的 （durable: true）

``` C#
channel.QueueDeclare(queue: "hello", durable: true, exclusive: false, autoDelete: false, arguments: null);
```

为了确保重新启动后，消息也不会丢失，还需要设置消息的持久化（Persistent=true)

``` C#
var properties = channel.CreateBasicProperties();
properties.Persistent = true;
```

将消息标记为持久性并不能完全保证不会丢失消息。尽管它告诉RabbitMQ将消息保存到磁盘，但是RabbitMQ接受消息并且尚未保存消息时，还有很短的时间。而且，RabbitMQ不会对每条消息都执行异步保存，它可能只是保存到缓存中，而没有真正写入磁盘。持久性保证并不强，但是对于我们的简单任务队列而言，这已经绰绰有余了。如果您需要更强有力的保证，则可以使用 发布者确认。

#### 任务公平分配

在有两名消费者的情况下，当有的消息都很处理很复杂，有的消息处理很轻松时，就有可能出现一个消费者一直忙碌而另一个消费者几乎不做任何工作。RabbitMQ对此一无所知，仍平均分配消息。

发生这种情况是因为RabbitMQ在消息进入队列时才调度消息。它不会查看使用者的未确认消息数。它只是盲目地将每第n条消息发送给第n个使用者。

以下设置告诉RabbitMQ一次不要给消费者一个以上的消息，也就是说在消费者确认上一条消息完成之前，不会再将新的消息发送给消费者，而是将其分派给下一个空闲的消费者。

``` C# 
channel.BasicQos(0, 1, false);
```

如果所有的消费者都忙，队列消息还在不停增加的情况下，还需要留意队列消息达到上限。

### 2.3 Publish/Subscribe（发布/订阅模式） 

![publish](imgs/003.png)

此模式用日志系统为例说明，在我们的日志系统中，接收器程序的每个运行副本都将获得消息。这样，我们将能够运行一个接收器并将日志定向到磁盘。同时我们将能够运行另一个接收器并在屏幕上查看日志。

本质上，已经发布的消息将被广播到所有的接收者。

RabbitMQ消息传递模型中的核心思想是生产者从不将任何消息直接发送到队列，生产者甚至根本不知道是否将消息传递到任何队列。

相反，生产者只能将消息发送到交换机。一方面，交换机接收来自生产者的消息，另一方面，将它们推入队列。交换机必须确切知道如何处理收到的消息，是否应将其附加到特定队列，是否应该将其附加到许多队列中，还是应该丢弃它。如何处理消息由exchange type定义。

exchange type有以下几种类型：direct, topic, headers ,fanout. 这里日志所需要的就是 fanout 类型，定义如下：

```C#
string message = "Hello World!";
var body = Encoding.UTF8.GetBytes(message);
  
channel.BasicPublish(exchange: "logs",
                     routingKey: "",
                     basicProperties: null,
                     body: body);
```

这里的日志消费者，只能处理当前正在发送的消息，而无法处理已经发布的旧消息。无论何时，连接到Rabbit后，自动创建一个新的队列，默认是随机的名称；一旦我们断开连接，队列将会被自动移除。

发送者代码示例：

```C#
public static void Publish(string[] args)
{
  
    var factory = new ConnectionFactory()
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672/")
    };
    using (var connection = factory.CreateConnection())
    {
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare("logs", ExchangeType.Fanout);
            string input = "";
  
            while (true)
            {
                input = Console.ReadLine();
  
  
                var body = Encoding.UTF8.GetBytes(input);
  
                channel.BasicPublish(exchange: "logs",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", input);
  
                if (input == "exit")
                {
                    break;
                }
            }
        }
    }
  
    Console.WriteLine(" Press 'exit' to exit.");
    Console.ReadLine();
}
```

接受者代码示例

```C#
public static void Consume()
{
    var factory = new ConnectionFactory()
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672/")
    };
    using (var connection = factory.CreateConnection())
    {
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare("logs", ExchangeType.Fanout, durable: true);
            var queueName = channel.QueueDeclare().QueueName;
  
            Console.WriteLine("Queue name {0}", queueName);
            channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "");
            Console.WriteLine(" [*] Waiting for logs.");
            var consumer = new EventingBasicConsumer(channel);
  
            consumer.Received += (model, ea) =>
             {
                 var body = ea.Body;
                 var message = Encoding.UTF8.GetString(body);
                 Console.WriteLine(" [x] {0}", message);
             };
  
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
  
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
```
### 2.4 Routing（路由模式）

广播模式算是录有模式的简化版，被绑定的队列无差别接受广播的任何消息，那么路由模式可以特定的接受某些类型的消息。例如：我们将只能将严重错误消息定向到日志文件（以节省磁盘空间），同时仍然能够在控制台上打印所有日志消息。

#### Bindings 绑定

绑定时 exchange 和 队列之间的关系。可以简单的理解为：队列对来自此exchange的消息感兴趣。

```C#
channel.QueueBind(queue: queueName,
                  exchange: "direct_logs",
                  routingKey: "black");
```

#### 直接交换

广播模式可以将所有消息广播给所有使用者，我们想要扩展它以允许根据消息的严重性过滤消息。例如，我们可能希望将日志消息写入磁盘的脚本仅接收严重错误，而不会在警告或信息日志消息上浪费磁盘空间。

 使用 fanout 类型，这种类型并没有给我们带来太大的灵活性，它只能进行无意识的广播。

使用 direct 类型，这种类型背后的算法很简单，消息进入其 binding key 和消息的 routingKey完全匹配的队列。

![](imgs/004.png)

上图设置中，我们看到绑定了两个队列的 direct exchange x，第一个队列绑定为orange, 第二个绑定key为两个：black, green.

使用routingKey为 orange 发布到 exchange 的消息将会路由到队列 Q1, routingKey 为 black 或者 green 将转到 Q2，所有其他消息将会被丢弃。

#### 多重绑定


![](imgs/005.png)

用相同的绑定密钥绑定多个队列是完全合法的。在我们的示例中，我们可以使用绑定键 black 在 X 和 Q1 之间添加绑定。在这种情况下，direct 交换类型将类似于 fanout，并将消息广播到所有匹配的队列。带有黑色路由键的消息将同时传递给 Q1和Q2。

生产者代码示例：

``` C# 
public static void Publish(string[] args)
{
    var factory = new ConnectionFactory()
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672/")
    };
    using (var connection = factory.CreateConnection())
    {
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);
            var severity = (args.Length > 0) ? args[0] : "info";
            var message = (args.Length > 1)
                ? string.Join(" ", args.Skip(1).ToArray())
                : "Hello world";
  
            var body = Encoding.UTF8.GetBytes(message);
  
            channel.BasicPublish(exchange: "direct_logs",
                                 routingKey: severity,
                                 basicProperties: null,
                                 body: body);
  
            Console.WriteLine(" [x] Sent {0}", message);
        }
    }
  
  
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}
```

消费者代码示例：

```C#
public static void Consume(string[] args)
{
    var factory = new ConnectionFactory()
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672/")
    };
    using (var connection = factory.CreateConnection())
    {
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);
            var queueName = channel.QueueDeclare().QueueName;
  
  
            foreach (var severity in args)
            {
                channel.QueueBind(queue: queueName, exchange: "direct_logs", routingKey: severity);
            }
             
            Console.WriteLine(" [*] Waiting for messages.");
  
            var consumer = new EventingBasicConsumer(channel);
  
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] {0}", message);
            };
  
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
```

### 2.5 Topic（主题模式）

主题模式中，发给 topic exchange 的消息的routingKey可以时多个关键字，关键字以“.”号分隔，这些关键字最好与消息的某些功能息息相关。例如： "stock.usd.nyse", "nyse.vmw", "quick.orange.rabbit". routingKey可以包含任何单词，最多255个字节。

binding key 也必须是相同的格式，topic exchange 和 direct exchange 的消息匹配模式相似：特定的 routing key 发送的消息将传递到所有能匹配的 binding key 的队列，topic exchange 有两个特殊的匹配语法：

a. * 号代替一个单词

b. # 号代替零个或者多个单词

![](imgs/006.png)

主题模式在特定的匹配规则下可以与 fanout 和 direct 两种模式一样使用：

a. 队列 binding key 设置为 # 号时，就与 fanout 模式一样可以接收 exchange 的所有消息了

b. 队列 binding key 不使用 # 和 * 号模糊关键字时，配置特定的关键字，就和 direct 模式一样，接收特定的 routing key 的消息。

### 2.6 RPC（请求/回复模式) 

前面几种消息模型处理消息时，消息制造者并不需要知道消息结果，适用于处理一些比较消耗资源（时间、硬件等）的任务。当远程服务处理完消息并返回结果时，可以使用 RPC 模式。

尽管 RPC 模式使用比较普遍，但是也不能滥用 RPC，使用之前应该明确以下内容：

a. 明确的知道哪些函数时本地调用，哪些函数时远程调用

b. 系统设计文档里面清除表示各个组件之间的依赖关系

c. 做好远程服务如果长时间宕机时的错误处理

如果对以上几点不是很明确，尽可能的使用异步调用代替 RPC

![](imgs/007.png)

RPC 的工作过程：

a. 客户端启动时，创建一个匿名的唯一队列

b. 对于 RPC 请求，客户端发送一条带有两个属性的消息： ReplyTo（设置为回调队列） 和 CorrelationId ( 对每个请求设置为唯一的值）

c. 请求被发送到 rpc_queue 队列

d. RPC 工作程序（服务器）等待该队列上的请求，出现请求时，它执行工作，并将包含结果的消息发送给 ReplyTo 属性声明的队列。

f. 客户端等待回调队列上的数据。出现消息时，如果CorrelationId属性与请求中的值匹配，则将消息返回给应用程序。

Server 代码如下：

``` C# 
public static void Server()
{
    var factory = new ConnectionFactory()
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672/")
    };
  
    using (var connection = factory.CreateConnection())
    {
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "rpc_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
  
  
            channel.BasicQos(0, 1, false);
  
            var consumer = new EventingBasicConsumer(channel);
  
            channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);
            Console.WriteLine(" [x] Awaiting RPC requests");
  
            consumer.Received += (model, ea) =>
            {
                string response = null;
  
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
  
                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    int n = int.Parse(message);
                    Console.WriteLine(" [.] fib({0})", message);
                    response = fib(n).ToString();
  
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                      basicProperties: replyProps, body: responseBytes);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                }
            };
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
```

RpcClient 代码如下：

```C#
using System;
using System.Collections.Generic;
using System.Text;
  
namespace RabbitMqDemo
{
    using System;
    using System.Collections.Concurrent;
    using System.Text;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
  
    public class RpcClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
  
        public RpcClient()
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672/")
            };
  
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
  
            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;
  
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
        }
  
        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: messageBytes);
  
            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);
  
            return respQueue.Take();
        }
  
        public void Close()
        {
            channel.Close();
            connection.Close();
        }
    }
  
   
}
```

发起请求：

```C#
static void Main(string[] args)
{
    var rpcClient = new RpcClient();
  
    Console.WriteLine(" [x] Requesting fib(30)");
    var response = rpcClient.Call("30");
    Console.WriteLine(" [.] Got '{0}'", response);
  
    rpcClient.Close();
  
    Console.ReadLine();
}

```
相对来看，RPC 模式更像是把简单队列的一种组合使用。

### 2.7 Publisher Confirms（发布者确认）

发布者确认对是AMQP 0.9.1 协议的扩展，默认情况下未启用。发布者确认通过ConfirmSelect 方法在 channel 级别启用。

```C#
var channel = connection.CreateModel();
channel.ConfirmSelect();
```

这个方法必须在每个 channel 创建时调用。发布者确认默认只需要开启一次就可以了，并不需要每次发布消息都启动。

#### Strategy # 1: 单个消息单独确认

``` C#
byte[] body = ...;
BasicProperties properties = ...;
channel.BasicPublish(exchange, queue, properties, body);
// uses a 5 second timeout
channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
```

像往常一样发布一条消息，并等待通过 Channel.WaitForConfirmsOrDie(TimeSpan) 方法进行确认。确认消息后，该方法将立即返回。如果未在超时时间内确认该消息或该消息没有被确认（这意味着代理出于某种原因无法处理该消息），则该方法将引发异常。异常的处理通常包括记录错误消息和/或重试发送消息。

此技术非常简单，但也有一个主要缺点：由于消息的确认会阻止所有后续消息的发布，因此它会大大降低发布速度。这种方法不会提供每秒超过数百条已发布消息的吞吐量。但是，对于某些应用程序来说这可能已经足够了。


#### Strategy # 2: 批量确认消息

为了改进前面的示例，我们可以发布一批消息，并等待整个批次被确认。以下示例使用了100个批次：

```C#
var batchSize = 100;
var outstandingMessageCount = 0;
while (ThereAreMessagesToPublish())
{
    byte[] body = ...;
    BasicProperties properties = ...;
    channel.BasicPublish(exchange, queue, properties, body);
    outstandingMessageCount++;
    if (outstandingMessageCount == batchSize)
    {
        channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
        outstandingMessageCount = 0;
    }
}
if (outstandingMessageCount > 0)
{
    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
}
```

与等待确认单个消息相比，等待一批消息被确认可以极大地提高吞吐量（对于远程RabbitMQ节点，这最多可以达到20-30次）。一个缺点是我们不知道发生故障时到底出了什么问题，因此我们可能必须将整个批处理保存在内存中，以记录有意义的内容或重新发布消息。而且该解决方案仍然是同步的，因此它阻止了消息的发布。

#### Strategy # 3: 异步确认

代理异步确认已发布的消息，只需在客户端上注册一个回调即可收到这些确认的通知：

```C#
var channel = connection.CreateModel();
channel.ConfirmSelect();
channel.BasicAcks += (sender, ea) =>
{
  // code when message is confirmed
};
channel.BasicNacks += (sender, ea) =>
{
  //code when message is nack-ed
};
```

#### 摘要

某些应用程序中，确保将发布的消息送到 exchange 非常重要，需要根据实际情况选择如何处理：

a. 逐条消息确认，同步等待确认：简单，单吞吐量非常有限。

b.批量消息确认，同步等待批量确认：简单，合理的吞吐量，但是很难推断出什么时候除了问题

c.异步处理：最佳性能和资源使用，在出现错误的情况下可以很好的空值

## 3 使用建议

### 使用连接字符串代替多项配置

虽然单独配置用户名，密码，连接实例名称等也是可以的，但是为了减少配置项的数量，建议使用 uri 的方式直接创建连接。

### connection 和 channel 都是需要释放的资源

应用程序反复打开连接而不关闭它们，最终将导致节点资源枯竭。RabbitMQ的管理界面提供了打开连接数的图表，建议程序在压力测试过程中，关注一下连接数。

以下是建议的创建connection 和 channel 的方式
```C#
var factory = new ConnectionFactory()
{
    Uri = new Uri("amqp://guest:guest@localhost:5672/")
};
 
using (var connection = factory.CreateConnection())
{
    using (var channel = connection.CreateModel())
    {
        ...
    }
}
```

### 建议代码创建队列，而不是手动创建

无论是创建 queue，还是创建 exchange ，或者创建绑定关系，建议使用用代码创建，代码里RabbitMQ声明创建 exchange, queue 等是幂等的（不存在才创建），减少手动创建带来不必要的错误。

需要注意的是，声明创建队列时，同名的队列配置不同会抛出异常

例如：已经存在名称 hello 的队列，配置 durable=true，再声明创建的 hello 的队列使用默认值不进行持久化，就会抛出异常。

### 谨慎使用发布者确认

理论上来说，确认消息能到达 exchange 能提高消息处理效率，如果需要确认，请选择异步确认。

即使项目实际要求对消息处理不是那么严格，建议声明 BasicNacks 来使用日志记录失败的消息。

### 队列持久化保证

系统在使用队列时，就要考虑服务器重启的情况，非自动创建的 queue和 exchange ，建议声明为持久化，消息持久化根据实际需求选择。

### 失败处理队列

有时候消费者处理消息时不可避免会出错，为了确保每个消息能够被处理，又不影响主业务的消息处理速度，我们可以把错误的任务重新发送给一个专用来处理错误的队列，使用单独错误队列里去做一些重试等操作。
