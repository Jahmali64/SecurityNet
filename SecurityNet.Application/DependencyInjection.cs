using Microsoft.Extensions.DependencyInjection;
using SecurityNet.Application.Associations;
using SecurityNet.Application.Auth;
using SecurityNet.Application.Users;

namespace SecurityNet.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services.AddScoped<IAssociationService, AssociationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
