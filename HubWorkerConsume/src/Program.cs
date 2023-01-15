using Hubs;
using HubWorkerConsume;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AzureSignalRConsoleApp.Client
{
    class Program
    {
        
        async static Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                IServiceProvider provider = builder.Services.BuildServiceProvider();

                builder.Services.AddSignalR()
                                .AddAzureSignalR();

                var hubConnection = new HubConnectionBuilder()
                .WithUrl("wss://xxxx.service.signalr.net/client/?hub=eventhub",
                    options => options.AccessTokenProvider = () =>
                    Task.FromResult(GenerateAccessTokenInternal("wss://xxxx.service.signalr.net/client/?hub=eventhub", null, TimeSpan.FromHours(1)))
                 )
                .Build();
                builder.Services.AddSingleton(hubConnection);

                builder.Services.AddHostedService<Worker>();
                var app = builder.Build();

                app.UseRouting();
                app.UseFileServer();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<EventHub>("/events");
                });
                await app.RunAsync();
            }
            catch (Exception ex)
            {

            }
        }
        private static string GenerateAccessTokenInternal(string audience, IEnumerable<Claim> claims, TimeSpan lifetime)
        {
            JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
            var expire = DateTime.UtcNow.Add(lifetime);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("xxxxx"));
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