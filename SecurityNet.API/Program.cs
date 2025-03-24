using Scalar.AspNetCore;
using SecurityNet.Application;
using SecurityNet.Infrastructure;

namespace SecurityNet.API;

public static class Program {
    public static void Main(string[] args) {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment()) {
            app.MapScalarApiReference();
            app.MapOpenApi();
            app.UseCors("DevelopmentCorsPolicy");
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}
