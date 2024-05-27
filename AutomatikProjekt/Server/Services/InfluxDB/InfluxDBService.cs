using AutomatikProjekt.Shared;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Writes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace AutomatikProjekt.Server.Services.InfluxDB
{
    public class InfluxDBService : IInfluxDBService
    {
        private string? _token = string.Empty;
        private string? _hostName = string.Empty;
        private string? _bucketName = string.Empty;
        private string? _orgId = string.Empty;
        private string? _measurement = string.Empty;

        public InfluxDBService(IConfiguration config)
        {
            _token = config["InfluxDB:INFLUXDB_TOKEN"];
            _hostName = config["InfluxDB:Host"];
            _bucketName = config["InfluxDB:Bucket"];
            _orgId = config["InfluxDB:Organization"];
            _measurement = "temperature";

        }

        public void Write(TemperatureSensor temperature)
        {
            using var client = new InfluxDBClient(_hostName, _token);

            using (var writeApi = client.GetWriteApi())
            {
                var point = PointData.Measurement(_measurement)
                    .Field(nameof(temperature.Temperature), temperature.Temperature)
                    .Field(nameof(temperature.TimeStamp), temperature.TimeStamp.ToString());

                writeApi.WritePoint(point, _bucketName, _orgId);
            }
        }

        public async Task<List<TemperatureSensor>> QueryDB(string? minimum, string? maximum)
        {
            using var influxDBClient = new InfluxDBClient(_hostName, _token);

            string startTimeString;
            string endTimeString;
            List<string> range = new();

            if (minimum != null)
            {
                DateTime dateFrom = DateTime.Parse(minimum);
                startTimeString = dateFrom.ToString("yyyy-MM-ddTHH:mm:ssZ");
                range.Add($"start: {startTimeString}");
            }

            if (maximum != null)
            {
                DateTime dateTo = DateTime.Parse(maximum);
                endTimeString = dateTo.ToString("yyyy-MM-ddTHH:mm:ssZ");
                range.Add($"stop: {endTimeString}");
            }

            var query = $"from(bucket:\"{_bucketName}\")";


            if (minimum != null && maximum != null)
            {
                query += $" |> range({string.Join(",", range.ToArray())}) |> sort(columns: [\"time\"])"; //TODO : change coloumn name to match time coloumn in database
            }
            else
            {
                query += $" |> range(start: 0) |> sort(columns: [\"TimeStamp\"])"; //TODO : change coloumn name to match time coloumn in database
            }

            var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(query, _orgId);

            IDictionary<int, TemperatureSensor> temperatureSensors = new Dictionary<int, TemperatureSensor>();

            foreach ( var fluxTable in fluxTables)
            {
                for (int i = 0; i < fluxTable.Records.Count(); i++)
                {
                    if (!temperatureSensors.ContainsKey(i))
                    {
                        TemperatureSensor temperatureSensor = new TemperatureSensor(); //TODO : This will maybe die, change key.
                        temperatureSensors.Add(i, temperatureSensor);
                    }

                    if (fluxTable.Records[i].GetValueByKey("_field").ToString().Equals("Temperature"))
                    {
                        temperatureSensors[i].Temperature = Convert.ToDouble(fluxTable.Records[i].GetValueByKey("_value"));
                    }
                    if (fluxTable.Records[i].GetValueByKey("_field").ToString().Equals("TimeStamp"))
                    {
                        temperatureSensors[i].TimeStamp = Convert.ToDateTime(fluxTable.Records[i].GetValueByKey("_value"));
                    }
                }
            }
            return temperatureSensors.Select(x => x.Value).ToList();
        }

        public async Task<TemperatureSensor> GetLatestTemperature()
        {
            using InfluxDBClient influxDBClient = new InfluxDBClient(_hostName, _token);

            var query = $"from(bucket:\"{_bucketName}\") |> range(start: 0) |> last()";
            var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(query, _orgId);

            TemperatureSensor temperatureSensor = new();

            foreach (var fluxTable in fluxTables)
            {
                if (fluxTable.Records[0].GetValueByKey("_field").ToString().Equals("Temperature"))
                {
                    temperatureSensor.Temperature = Convert.ToDouble(fluxTable.Records[0].GetValueByKey("_value"));
                }
                if (fluxTable.Records[0].GetValueByKey("_field").ToString().Equals("TimeStamp"))
                {
                    temperatureSensor.TimeStamp = Convert.ToDateTime(fluxTable.Records[0].GetValueByKey("_value"));
                }
            }

            return temperatureSensor;
        }
    }

    
}
