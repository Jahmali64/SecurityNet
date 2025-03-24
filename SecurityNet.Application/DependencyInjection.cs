using Microsoft.Extensions.DependencyInjection;
using SecurityNet.Application.Associations;

namespace SecurityNet.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services.AddScoped<IAssociationService, AssociationService>();

        return services;
    }
}
