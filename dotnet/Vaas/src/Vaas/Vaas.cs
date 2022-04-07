﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Vaas.Messages;
using Websocket.Client;
using Websocket.Client.Exceptions;

namespace Vaas
{
    public class Vaas
    {
        private string Token { get; }

        public WebsocketClient Client { get; set; }

        public string SessionId { get; set; }

        public bool Authenticated { get; set; }
        
        public Uri Url { get; set; } = new Uri("wss://gateway-vaas.gdatasecurity.de");

        private Dictionary<string, VerdictResponse> VerdictResponsesDict { get; } = new Dictionary<string, VerdictResponse>();

        public Vaas(string token)
        {
            Token = token;
        }

        public void Connect()
        {
            Client = new WebsocketClient(Url, CreateWebsocketClient());
            Client.ReconnectTimeout = null;
            Client.MessageReceived.Subscribe(msg =>
            {
                if (msg.MessageType == WebSocketMessageType.Text)
                {
                    var message = JsonSerializer.Deserialize<Message>(msg.Text);
                    switch (message.Kind)
                    {
                        case "AuthResponse":
                            var authenticationResponse = JsonSerializer.Deserialize<AuthenticationResponse>(msg.Text);
                            if (authenticationResponse.Success == true)
                            {
                                Authenticated = true;
                                SessionId = authenticationResponse.SessionId;
                            }
                            break;
                        
                        case "VerdictResponse":
                            var options = new JsonSerializerOptions() {Converters = {new JsonStringEnumConverter()}};
                            var verdictResponse = JsonSerializer.Deserialize<VerdictResponse>(msg.Text,options);
                            VerdictResponsesDict.Add(verdictResponse.Guid, verdictResponse);
                            break;
                    }
                   
                }
            });
            Client.Start().GetAwaiter().GetResult();
            if (!Client.IsStarted)
            {
                throw new WebsocketException("Could not start client");
            }
            
            Authenticate();
            Task.Run(SendPing);
        }

        private void Authenticate()
        {
            var authenticationRequest = new AuthenticationRequest(Token, null);
            string jsonString = JsonSerializer.Serialize(authenticationRequest);
            Client.Send(jsonString);
            for (var i = 0; i < 10; i++)
            {
                if (Authenticated == true)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            if (Authenticated != true)
            {
                throw new UnauthorizedAccessException();
            }
        }

        public async Task<Verdict> ForSha256Async(string sha256)
        {
            var value = await ForRequestAsync(new AnalysisRequest(sha256,SessionId));
            return value.Verdict;
        }
        
        public async Task<Verdict> ForFileAsync(string path)
        {
            var sha256 = SHA256CheckSum(path);
            var verdictResponse = await ForRequestAsync(new AnalysisRequest(sha256, SessionId));
            if (verdictResponse.Verdict == Verdict.Unknown)
            {
                var url = verdictResponse.Url;
                var token = verdictResponse.UploadToken;
                var data = await File.ReadAllBytesAsync(path);
                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.Authorization, token);
                    client.UploadData(url, "PUT", data);
                }
                var response = await WaitForResponseAsync(verdictResponse.Guid);

                return response.Verdict;

            }

            return verdictResponse.Verdict;
        }

        public async Task<List<Verdict>> ForSha256ListAsync(IEnumerable<string> sha256List)
        {
            return (await Task.WhenAll(sha256List.Select(ForSha256Async))).ToList();

        }

        public async Task<List<Verdict>> ForFileListAsync(IEnumerable<string> fileList)
        {
            return (await Task.WhenAll(fileList.Select(ForFileAsync))).ToList();
        }

        
        private async Task<VerdictResponse> ForRequestAsync(AnalysisRequest analysisRequest)
        {
            var jsonString = JsonSerializer.Serialize(analysisRequest);
            await Task.Run(()=>Client.Send(jsonString));

            return await WaitForResponseAsync(analysisRequest.Guid);
        }

        private async void SendPing()
        {
            while (Client.IsRunning)
            {
                Client.Send("ping");
                Thread.Sleep(10000);
            }
            
        }
        
        private async Task<VerdictResponse> WaitForResponseAsync(string guid)
        {
            VerdictResponse value;
            while (VerdictResponsesDict.TryGetValue(guid, out value) == false)
            {
                await Task.Delay(300);
            }
            VerdictResponsesDict.Remove(guid);
            return value;
        }
        
        private string SHA256CheckSum(string filePath)
        {
            using (SHA256 SHA256 = SHA256Managed.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return (Convert.ToHexString(SHA256.ComputeHash(fileStream))).ToLower();
            }
        }

        private static Func<ClientWebSocket> CreateWebsocketClient()
        {
            return () =>
            {
                var clientWebSocket = new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(30)
                    }
                };
                return clientWebSocket;
            };
        }
    }
}