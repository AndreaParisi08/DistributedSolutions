using System.Text;
using RabbitMQ.Client;

Console.WriteLine("=== AVVIO PRODUCER ===");

// 1. Configurazione della factory puntando al container Docker locale
var factory = new ConnectionFactory() { HostName = "localhost" };

// 2. Apertura della Connessione TCP (Singleton/Costosa)
// L'uso combinato di "await using" e "await" garantisce che aspettiamo la connessione reale
// e che la chiuderemo in modo asincrono, pulito e sicuro alla fine del programma.
await using var connection = await factory.CreateConnectionAsync();

// 3. Apertura del Canale in modo ASINCRONO
await using var channel = await connection.CreateChannelAsync();

// 4. Dichiarazione della Coda
// Se la coda non esiste sul server, RabbitMQ la creerà. Se esiste già, non fa nulla.
string queueName = "hello_queue";
await channel.QueueDeclareAsync(
    queue : queueName,
    durable : false,     // Se il server si riavvia, la coda viene persa
    exclusive : false,   // Può essere usata anche da altre connessioni
    autoDelete: false,  // Non cancellare la coda se i consumer si scollegano
    arguments : null     // Configurazioni avanzate aggiuntive
);

// 5. Preparazione del messaggio da inviare
string message = "Il mio primo messaggio.";

// RabbitMQ non accetta stringhe, accetta solo array di byte (payload binari)
var body = Encoding.UTF8.GetBytes(message);

// 6. Pubblicazione del messaggio nell'Exchange di Default
// Usando un exchange vuoto "", inviamo il messaggio direttamente alla coda 
// che ha lo stesso nome della routingKey.
await channel.BasicPublishAsync(
    exchange: "",
    routingKey: queueName,
    body: body
);

Console.WriteLine($"[x] Inviato: '{message}'");
Console.WriteLine("Premi [Invio] per uscire.");
Console.ReadLine();