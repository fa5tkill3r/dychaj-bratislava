using BP.Data.DbModels;

namespace BP.API.Services;

public interface IWeatherService
{
    public Task GetData();
    public Task AddSensor(Module module, string uniqueId);
}