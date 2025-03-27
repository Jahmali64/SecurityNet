using Microsoft.Extensions.DependencyInjection;
using SecurityNet.Application.Services.Associations;
using SecurityNet.Application.Services.Auth;
using SecurityNet.Application.Services.Users;
using SecurityNet.Application.Services.UserTokens;

namespace SecurityNet.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services.AddScoped<IAssociationService, AssociationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserTokenService, UserTokenService>();

        return services;
    }
}
