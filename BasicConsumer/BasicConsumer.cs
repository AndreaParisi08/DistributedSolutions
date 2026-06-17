using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("=== AVVIO CONSUMER ===");

// 1. Configurazione e Connessione (Identica al Producer)
var factory = new ConnectionFactory() { HostName = "localhost" };
await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

string queueName = "hello_queue";

// 2. Dichiarazione della Coda
// Best Practice: La dichiariamo anche qui. Se il Consumer si avviasse prima del Producer, 
// l'applicazione andrebbe in crash cercando di ascoltare una coda inesistente.
// RabbitMQ è idempotente: se la coda esiste già con gli stessi parametri, non fa nulla.
// Questo garantisce che entrambi i microservizi possano avviarsi in qualsiasi ordine senza causare errori.
await channel.QueueDeclareAsync(
    queue: queueName,
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

Console.WriteLine($"[*] In attesa di messaggi sulla coda '{queueName}'.");

// 3. Creazione del Consumer Asincrono
var consumer = new AsyncEventingBasicConsumer(channel);

// 4. Definizione dell'evento "Al Ricevimento del Messaggio"
consumer.ReceivedAsync += async (model, ea) =>
{
    // Il payload arriva come array di byte, dobbiamo decodificarlo
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"[x] Ricevuto ed elaborato: '{message}'");

    // Essendo un gestore di eventi asincrono, ritorniamo un Task completato
    await Task.CompletedTask;
};

// 5. Iscrizione alla Coda
// autoAck: true significa "Considera il messaggio elaborato con successo non appena lo ricevi".
// Nel prossimo modulo vedremo perché in produzione questo valore è quasi sempre 'false'.
await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: true,
    consumer: consumer
);

Console.WriteLine("Premi [Invio] per chiudere il Consumer.");
Console.ReadLine();
