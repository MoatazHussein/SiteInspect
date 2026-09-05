using Microsoft.EntityFrameworkCore;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;

namespace SiteInspect.Infrastructure.Persistence.Initialization.Steps;

internal sealed class TemplatesSeedStep(ApplicationDbContext dbContext) : ISeedStep
{
    public string Name => "Inspection templates";
    public int Order => SeedStepOrder.Templates;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var templateItems = DemoSeedData.CreateTemplateItems();
        var template = new InspectionTemplate(
            DemoSeedData.TemplateId,
            "Electrical and Safety Inspection",
            1,
            true,
            templateItems);

        if (!await dbContext.InspectionTemplates.AnyAsync(item => item.Id == DemoSeedData.TemplateId, cancellationToken))
        {
            dbContext.InspectionTemplates.Add(template);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
