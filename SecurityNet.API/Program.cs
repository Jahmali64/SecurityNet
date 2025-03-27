using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SecurityNet.Application;
using SecurityNet.Infrastructure;
using SecurityNet.Shared.Models;

namespace SecurityNet.API;

public static class Program {
    public static void Main(string[] args) {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        var jwtSettings = new JwtSettings();
        builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped(typeof(CancellationToken), serviceProvider => {
            IHttpContextAccessor httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            return httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
        });
        builder.Services.AddCors(options => {
            options.AddPolicy(name: "DevelopmentCorsPolicy", configurePolicy: policy => policy.AllowAnyOrigin());
        });

        builder.Services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ValidateIssuerSigningKey = true
            };
        });
        builder.Services.AddAuthorization();

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment()) {
            app.MapScalarApiReference();
            app.MapOpenApi();
            app.UseCors("DevelopmentCorsPolicy");
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}
