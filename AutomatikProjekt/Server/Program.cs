using AutomatikProjekt.Server.Hubs;
using AutomatikProjekt.Server.Services.InfluxDB;
using AutomatikProjekt.Server.Services.MqttService;
using AutomatikProjekt.Shared;
using Microsoft.AspNetCore.ResponseCompression;

namespace AutomatikProjekt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHostedService<MqttClientWorker>();
            builder.Services.AddSingleton<IInfluxDBService, InfluxDBService>();


            builder.Services.AddSignalR();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();


            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
            app.MapHub<SensorHub>("/sensorhub");
            app.MapGet("/distance", Distance).WithName("GetDistance").WithOpenApi();
            app.MapGet("/inductive", Inductive).WithName("GetInductive").WithOpenApi();
            app.MapGet("/temperature", Temperature).WithName("GetTemperature").WithOpenApi();
            app.MapGet("/latestTemperature", LatestTemperature).WithName("GetLatestTemperature").WithOpenApi();

            static DistanceSensor Distance()
            {
                return null;
            }

            static InductiveSensor Inductive()
            {
                return null;
            }

            static async Task<List<TemperatureSensor>> Temperature(IInfluxDBService influxDBService, DateTime startTime, DateTime endTime)
            {
                string dateTimeStart = startTime.ToString();
                string dateTimeEnd = endTime.ToString();
                var test = (await influxDBService.QueryDB(dateTimeStart, dateTimeEnd));

                return test;
            }

            static async Task<TemperatureSensor> LatestTemperature(IInfluxDBService influxDBService)
            {
                
                var test = (await influxDBService.GetLatestTemperature());

                return test;
            }   

            app.Run();
        }
    }
}
