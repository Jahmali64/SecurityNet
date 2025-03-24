using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecurityNet.Infrastructure.DbContexts;

namespace SecurityNet.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        services.AddDbContextFactory<SecurityNetDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(name: "DefaultConnection")));
        
        return services;
    }
}
