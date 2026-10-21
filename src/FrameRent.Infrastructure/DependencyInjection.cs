using FrameRent.Application.Interfaces;
using FrameRent.Application.Services;
using FrameRent.Domain.Interfaces;
using FrameRent.Infrastructure.Identity;
using FrameRent.Infrastructure.Persistence;
using FrameRent.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrameRent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFrameRentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IRentalService, RentalService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
