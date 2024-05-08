using AutomatikProjekt.Shared;
using GenericHttpClientRepository;
using Microsoft.VisualBasic;
using System;

namespace AutomatikProjekt.Client.Caller
{
    public class ApiCaller : IAPICaller
    {

        private readonly IGenericRepository _repository;
        public ApiCaller(IGenericRepository repository)
        {
            _repository = repository;
        }

        public async Task<DistanceSensor> GetDistanceAsync()
        {
            Uri uri = new Uri("Distance");
            return await _repository.GetAsync<DistanceSensor>(uri);
        }

        public async Task<List<TemperatureSensor>> GetTemperatureAsync()
        {
            Uri uri = new Uri("Temperature");
            return await _repository.GetAsync<List<TemperatureSensor>>(uri);
        }

        public async Task<InductiveSensor> GetInductiveAsync()
        {
            Uri uri = new Uri("Inductive");
            return await _repository.GetAsync<InductiveSensor>(uri);
        }
    }
}
