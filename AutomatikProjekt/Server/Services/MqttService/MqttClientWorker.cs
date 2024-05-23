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
            await MQTTSetup();

        }

        internal async Task MQTTSetup()
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
                    if (e != null)
                    {

                        string? payload = System.Text.Encoding.Default.GetString(e.ApplicationMessage.PayloadSegment);
                        Root root = JsonConvert.DeserializeObject<Root>(payload)!;
                        //Console.WriteLine($"Payload: {payload}");
                        //Console.WriteLine($"Deserialized temperature: {root.data[0].values[0].value}");
                        //Console.WriteLine($"Deserialized TimeStamp: {root.data[0].values[0].Timestamp}");
                        if (e.ApplicationMessage.Topic == _temperatureTopic)
                        {
                            TemperatureSensor temperatureSensor = new() { TimeStamp = root.data[0].values[0].Timestamp, Temperature = root.data[0].values[0].value };
                            _influxDBService.Write(temperatureSensor);
                            await _sensorHub.Clients.All.SendAsync("ReceiveLatestTemperature", temperatureSensor);
                        }
                        else if (e.ApplicationMessage.Topic == _DistanceTopic)
                        {
                            DistanceSensor distanceSensor = new() { Distance = root.data[0].values[0].value, TimeStamp = root.data[0].values[0].Timestamp };
                            await _sensorHub.Clients.All.SendAsync("ReceiveDistanceSensor", distanceSensor);
                        }
                        else if (e.ApplicationMessage.Topic == _InductiveTopic)
                        {
                            InductiveSensor inductiveSensor = new() { TimeStamp = root.data[0].values[0].Timestamp };
                            if (root.data[0].values[0].value > 0)
                                inductiveSensor.IsMetal = true;
                            await _sensorHub.Clients.All.SendAsync("ReceiveInductiveSensor", inductiveSensor);
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
                    .WithTopicFilter(
                        x =>
                        {
                            x.WithTopic(_DistanceTopic);
                        })
                    .WithTopicFilter(
                        x =>
                        {
                            x.WithTopic(_InductiveTopic);
                        })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOption, CancellationToken.None);

                while (true) { }

            }

        }
    }
}
