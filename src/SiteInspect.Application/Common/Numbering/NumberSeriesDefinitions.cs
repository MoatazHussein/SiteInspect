using SiteInspect.Application.Common.Abstractions.Numbering;

namespace SiteInspect.Application.Common.Numbering;

public static class NumberSeriesDefinitions
{
    public static readonly NumberSeriesDefinition Inspections = new(
        "Inspection",
        "INS",
        6);
}
