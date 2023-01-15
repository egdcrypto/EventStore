using EGD.Command.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace HubWorkerConsume
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HubConnection? _connection;
        private readonly TaskCompletionSource<string> resp = new TaskCompletionSource<string>();

        public Worker(ILogger<Worker> logger, HubConnection? hubConnection)
        {
            _logger = logger;
            _connection = hubConnection;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_connection != null)
                {

                    Console.WriteLine("Starting connection. Press Ctrl-C to close.");
                    var cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (sender, a) =>
                    {
                        a.Cancel = true;
                        cts.Cancel();
                    };

                    var hubConnection = new HubConnectionBuilder()

                    .WithUrl("wss://xxxx.service.signalr.net/client/?hub=eventhub",
                        options => options.AccessTokenProvider = () =>
                        Task.FromResult(GenerateAccessTokenInternal("wss://xxxxx.service.signalr.net/client/?hub=eventhub", null, TimeSpan.FromHours(1)))
                     )
                    .Build();

                    _connection.On<CommandEventStore>("BroadcastCommandEvent", commandEvent =>
                    {
                        Console.WriteLine($"Timestamp: {commandEvent.Timestamp}\nCorrelationId: {commandEvent.CorrelationId}\nAction: {commandEvent.Action}\n\n");
                    });
                    try
                    {
                        await _connection.StartAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }


                    _connection.Closed += e =>
                    {
                        Console.WriteLine("Connection closed with error: {0}", e);

                        cts.Cancel();
                        return Task.CompletedTask;
                    };

                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(-1, cts.Token);
                    if (cts.Token.IsCancellationRequested)
                        break;
                }
                else
                {
                    break;
                }
            }
        }

        private static string GenerateAccessTokenInternal(string audience, IEnumerable<Claim> claims, TimeSpan lifetime)
        {
            JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
            var expire = DateTime.UtcNow.Add(lifetime);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("xxxx"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: null,
                audience: audience,
                subject: claims == null ? null : new ClaimsIdentity(claims),
                expires: expire,
                signingCredentials: credentials);
            return JwtTokenHandler.WriteToken(token);
        }
    }
    
}