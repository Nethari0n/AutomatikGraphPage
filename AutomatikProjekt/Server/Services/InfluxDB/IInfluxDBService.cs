using AutomatikProjekt.Shared;

namespace AutomatikProjekt.Server.Services.InfluxDB
{
    public interface IInfluxDBService
    {
        void Write(TemperatureSensor temperature);
        Task<List<TemperatureSensor>> QueryDB(string? minimum, string? maximum);
        Task<TemperatureSensor> GetLatestTemperature();
    }
}
