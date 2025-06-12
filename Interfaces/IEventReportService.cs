namespace StudetCouncilPlannerAPI.Interfaces;

public interface IEventReportService
{
    Task<byte[]> GeneratePlanNextMonthAsync();
    Task<byte[]> GenerateReportForMonthAsync(int year, int month);
    Task<byte[]> GenerateUserEventsReportAsync(Guid userId);
}