using System.Reflection;
using Application.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRepositories(this IServiceCollection services, Assembly assembly)
    {
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.BaseType != null
                        && t.BaseType.IsGenericType
                        && t.BaseType.GetGenericTypeDefinition() == typeof(BaseRepository<>))
            .ToList();


        foreach (var repo in repositoryTypes)


        {
            var interfaceType = repo.GetInterfaces().FirstOrDefault();


            if (interfaceType != null)
                services.AddScoped(interfaceType, repo);


            else
                services.AddScoped(repo);
        }
    }
}