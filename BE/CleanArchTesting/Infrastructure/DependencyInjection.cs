// Infrastructure/DependencyInjection.cs
using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Support both keys for convenience
        var connectionString = configuration.GetConnectionString("Cinema")
            ?? configuration.GetConnectionString("DBDefault")
            ?? "Server=localhost,1433;Database=Cinema;User Id=sa;Password=StrongP@ssword1;TrustServerCertificate=True";

        services.AddDbContext<CinemaDbContext>(opt => opt.UseSqlServer(connectionString));
        services.AddScoped<ICinemaDbContext>(sp => sp.GetRequiredService<CinemaDbContext>());

        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IVoucherService, VoucherService>();

        return services;
    }
}
