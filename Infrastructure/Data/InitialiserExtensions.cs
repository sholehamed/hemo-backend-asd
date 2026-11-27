using Application.Repository.Services;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly MenuService MenuService;
    private readonly RoleService RoleService;
    private readonly SubSystemService SubSystemService;
    private readonly UserService UserService;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context, RoleService roleService, UserService userService,
        SubSystemService subSystemService, MenuService menuService)
    {
        _logger = logger;
        _context = context;
        RoleService = roleService;
        UserService = userService;
        SubSystemService = subSystemService;
        MenuService = menuService;
    }


    public async Task InitialiseAsync()
    {
        try
        {
             await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            //await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    public async Task TrySeedAsync()
    {
        await SubSystemService.InserDefaults(CancellationToken.None);
        await InserDefaults<Menu>("Menu", CancellationToken.None);
        await UserService.InserDefaults(CancellationToken.None);
    }

    public async Task InserDefaults<T>(string JsonFileName, CancellationToken cancellationToken) where T : BaseEntity
    {
        try
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JsonData", $"{JsonFileName}.json");
            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);
                var NewItems = JsonConvert.DeserializeObject<List<T>>(jsonData);
                if (NewItems.Any())
                {
                    var IsNotEmpty = _context.Set<T>().Any();
                    if (!IsNotEmpty)
                    {
                        foreach (var item in NewItems)
                        {
                            await _context.Set<T>().AddAsync(item, cancellationToken);
                            await _context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
        }
    }
}