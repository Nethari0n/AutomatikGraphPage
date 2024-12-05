using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using AutomatikProjekt.Shared;
using AutomatikProjekt.Server.Services.InfluxDB;

namespace AutomatikProjekt.Server.Hubs
{
    public class SensorHub : Hub
    {
        IInfluxDBService _influxDBService;

        public SensorHub(IInfluxDBService influxDBService)
        {
            _influxDBService = influxDBService;
        }

        public async Task GetTemperatureSensor()
        {
            try
            {
                List<TemperatureSensor> temperatureSensors = await _influxDBService.QueryDB(null, null);
                await Clients.Caller.SendAsync("ReceiveTemperatureSensorList", temperatureSensors);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

      
    }
}
