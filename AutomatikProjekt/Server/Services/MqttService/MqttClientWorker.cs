using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet;
using System.Security.Authentication;
using Newtonsoft.Json;
using AutomatikProjekt.Server.Services.InfluxDB;
using AutomatikProjekt.Shared;
using Microsoft.AspNetCore.SignalR;
using AutomatikProjekt.Server.Hubs;
using System.Configuration;
using System.Diagnostics;
using MQTTnet.Protocol;

namespace AutomatikProjekt.Server.Services.MqttService
{
    public class MqttClientWorker : BackgroundService
    {
        private string? _clientID;
        private string? _host;
        private int? _port;
        private string? _username;
        private string? _password;
        private string? _temperatureTopic;
        private string? _DistanceTopic;
        private string? _InductiveTopic;

        private readonly IHubContext<SensorHub> _sensorHub;
        private readonly IInfluxDBService _influxDBService;

        public MqttClientWorker(IConfiguration config, IHubContext<SensorHub> hubContext, IInfluxDBService influxDBService)
        {
            _clientID = config["HiveMQ:ClientID"];
            _host = config["HiveMQ:Host"];
            _port = int.Parse(config["HiveMQ:Port"] ?? "8883");
            _username = config["HiveMQ:Username"];
            _password = config["HiveMQ:Password"];
            _temperatureTopic = config["HiveMQ:TopicTemperature"];
            _DistanceTopic = config["HiveMQ:TopicDistance"];
            _InductiveTopic = config["HiveMQ:TopicInductive"];

            _sensorHub = hubContext;
            _influxDBService = influxDBService;
        }

        protected async override Task ExecuteAsync(CancellationToken cancelToken)
        {
           await TemperatureMQTTSetup();

        }

        internal async Task TemperatureMQTTSetup()
        {
            MqttFactory mqttFactory = new();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                MqttClientOptions mqttClientOption = new MqttClientOptionsBuilder()
                    .WithClientId(_clientID)
                    .WithCleanSession()
                    .WithProtocolVersion(MqttProtocolVersion.V311)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(300))
                    .WithTlsOptions(
                            x =>
                            {
                                x.WithCertificateValidationHandler(_ => true);

                                x.WithSslProtocols(SslProtocols.Tls12);
                                
                            })
                    .WithTcpServer(_host, port: _port)
                    .WithCredentials(_username, _password)
                    .Build();
                mqttClient.ApplicationMessageReceivedAsync += async (e) =>
                {
                    Console.WriteLine("------------------------------Send Help-----------------------------------");
                    if (e != null)
                    {
                        string? payload = System.Text.Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment);
                        Root root = JsonConvert.DeserializeObject<Root>(payload)!;
                        Console.WriteLine($"Payload: {payload}");
                        Console.WriteLine($"Deserialized temperature: {root.data[0].values[0].value}");
                        Console.WriteLine($"Deserialized TimeStamp: {root.data[0].values[0].Timestamp}");
                        if (root != null)
                        {
                            TemperatureSensor temperatureSensor = new() { TimeStamp = root.data[0].values[0].Timestamp, Temperature = root.data[0].values[0].value  };
                            _influxDBService.Write(temperatureSensor);
                            await _sensorHub.Clients.All.SendAsync("ReceiveTemperatureSensorList", temperatureSensor);
                        }
                    }
                };

                await mqttClient.ConnectAsync(mqttClientOption, CancellationToken.None);
                Debug.WriteLine($"------------------------------Is connected: {mqttClient.IsConnected}-----------------------------------");

                var mqttSubscribeOption = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        x =>
                        {
                            x.WithTopic(_temperatureTopic);
                        })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOption, CancellationToken.None);

                while (true) { }

            }




        }

        internal async Task DistanceAndInductiveMQTTSetup()
        {
            MqttFactory mqttFactory = new();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                MqttClientOptions mqttClientOption = new MqttClientOptionsBuilder()
                    .WithClientId(_clientID)
                    .WithCleanSession()
                    .WithProtocolVersion(MqttProtocolVersion.V311)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
                    .WithTlsOptions(
                            x =>
                            {
                                x.WithSslProtocols(SslProtocols.Tls12);
                            })
                    .WithTcpServer(_host, port: _port)
                    .WithCredentials(_username, _password)
                    .Build();

                mqttClient.ApplicationMessageReceivedAsync += async (e) =>
                {
                    if (e != null)
                    {
                        string? payload = System.Text.Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment);
                        InductiveSensor inductiveSensor = JsonConvert.DeserializeObject<InductiveSensor>(payload)!;

                        if (inductiveSensor != null)
                        {
                            await _sensorHub.Clients.All.SendAsync("ReceiveInductiveSensorList", inductiveSensor);
                        }
                    }
                };

                await mqttClient.ConnectAsync(mqttClientOption, CancellationToken.None);

                var mqttSubscribeOption = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        x =>
                        {
                            x.WithTopic(_InductiveTopic)
                            .WithTopic(_DistanceTopic);
                        })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOption, CancellationToken.None);
            }
        }
    }
}
