using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Abstractions.Storage;
using SiteInspect.Application.Features.Authentication.Common;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Infrastructure.Health;
using SiteInspect.Infrastructure.Identity.Entities;
using SiteInspect.Infrastructure.Identity.Services;
using SiteInspect.Infrastructure.Messaging;
using SiteInspect.Infrastructure.Persistence;
using SiteInspect.Infrastructure.Persistence.Initialization;
using SiteInspect.Infrastructure.Persistence.ReadServices.Inspections;
using SiteInspect.Infrastructure.Persistence.Repositories;
using SiteInspect.Infrastructure.Storage;

namespace SiteInspect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is required.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddSingleton<IInspectionAttachmentStorage, LocalInspectionAttachmentStorage>();
        services.AddScoped<IInspectionTemplateRepository, InspectionTemplateRepository>();
        services.AddScoped<IInspectorDirectory, InspectorDirectory>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddScoped<IInspectionReadService, InspectionReadService>();
        services.AddScoped<IInspectionManagementReadService, InspectionManagementReadService>();
        services.TryAddScoped<ICurrentUser, NullCurrentUser>();
        services.TryAddSingleton(TimeProvider.System);

        services
            .AddHealthChecks()
            .AddCheck(
                "sql-server",
                new SqlServerHealthCheck(connectionString),
                tags: ["ready", "database"]);

        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

        AddMessaging(services, configuration);

        return services;
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetValue("Messaging:Enabled", false))
        {
            return;
        }

        var rabbitMqOptions = configuration
            .GetRequiredSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException("RabbitMQ configuration is required when messaging is enabled.");

        if (string.IsNullOrWhiteSpace(rabbitMqOptions.Password))
        {
            throw new InvalidOperationException(
                "Messaging:RabbitMq:Password must be supplied outside source control when messaging is enabled.");
        }

        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();
            configurator.UsingRabbitMq((context, bus) =>
            {
                bus.Host(new Uri(rabbitMqOptions.Host), host =>
                {
                    host.Username(rabbitMqOptions.Username);
                    host.Password(rabbitMqOptions.Password);
                });

                bus.ConfigureEndpoints(context);
            });
        });
    }
}
