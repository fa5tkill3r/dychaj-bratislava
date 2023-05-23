using BP.Data.DbModels;
using BP.Data.Models;

namespace BP.API.Services.WeatherServices;

public interface IWeatherService
{
    public Task GetData();
    public Task AddSensor(Module module, string uniqueId);
    public Task<List<GetSensorsDto>> GetSensors();
}