using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SiteInspect.Application.Common.Abstractions.Numbering;
using SiteInspect.Application.Common.Behaviors;
using SiteInspect.Application.Common.Numbering;

namespace SiteInspect.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INumberSeriesService, NumberSeriesService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
