using Application.Repository.Services;

namespace web.BackgroundServices;

public class AddApis : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AddApis(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            using (var scope = _serviceProvider.CreateScope()) // ساخت یک اسکوپ جدید
            {
                var ApiService = scope.ServiceProvider.GetRequiredService<ApiService>(); // دریافت سرویس

                await ApiService.UpateApiList();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}