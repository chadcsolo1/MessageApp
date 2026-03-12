using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

//***Producer***
Console.WriteLine("Producer");

//Create Connection factory
var factory = new ConnectionFactory() { HostName = "localhost" };
//Create a new connection - this will open a new connection to the RabbitMQ broker
using var connection = await factory.CreateConnectionAsync();
//Establisha new channel - this is how we will communicate with RabbitMQ
using var channel = await connection.CreateChannelAsync();

// Ensure the queue exists (create it if not already there)
//Via the channel we can declare a queue, bind to a queue, unbind from a queue, work with exchanges, and acknowledge & consume messages.
//await channel.QueueDeclareAsync(
//    queue: "message",
//    durable: true, // save to disk so the queue isn’t lost on broker restart
//    exclusive: false, // can be used by other connections
//    autoDelete: false, // don’t delete when the last consumer disconnects
//    arguments: null); // Will create a new queue if it does not exist - making this an idempotent operation - safe to call it multiple times.



//Using an exchange instead of a queue - this will allow us to publish messages to the exchange and have them routed to the appropriate queue(s) based on the routing key.
await channel.ExchangeDeclareAsync(
    exchange: "messages",
    durable: true,
    autoDelete: false,
    type: ExchangeType.Fanout);

await Task.Delay(1000);

//For loop that will publish 10 messages
for (int i = 0; i < 10; i++)
{
    var messageAlt = $"{DateTime.UtcNow} - {Guid.CreateVersion7()}";
    var bodyAlt = Encoding.UTF8.GetBytes(messageAlt);

    // Publish the message - direct to the queue
    //await channel.BasicPublishAsync(
    //    exchange: string.Empty,
    //    routingKey: "message",
    //    mandatory: true,
    //    basicProperties: new BasicProperties { Persistent = true },
    //    body: bodyAlt);

    // Publish the message - to the exchange
    await channel.BasicPublishAsync(
    exchange: "messages",
    routingKey: string.Empty,
    mandatory: true,
    basicProperties: new BasicProperties { Persistent = true },
    body: bodyAlt);

    Console.WriteLine($"Sent: {messageAlt}");

    await Task.Delay(2000);
}



