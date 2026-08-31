namespace SiteInspect.Domain.Inspections.InspectionAggregate;

public enum InspectionStatus
{
    Assigned,
    InProgress,
    Submitted,
    CorrectiveActionsOpen,
    Completed,
    Cancelled,
}
