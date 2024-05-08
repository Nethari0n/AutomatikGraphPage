using AutomatikProjekt.Shared;

namespace AutomatikProjekt.Client.Caller
{
    public interface IAPICaller
    {
        Task<DistanceSensor> GetDistanceAsync();
        Task<List<TemperatureSensor>> GetTemperatureAsync();
        Task<InductiveSensor> GetInductiveAsync();

    }
}
