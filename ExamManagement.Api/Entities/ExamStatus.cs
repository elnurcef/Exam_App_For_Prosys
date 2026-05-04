namespace ExamManagement.Api.Entities;

public static class ExamStatus
{
    public const string Scheduled = "Scheduled";
    public const string Graded = "Graded";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Scheduled, Graded, Cancelled];

    public static bool IsValid(string status) => All.Contains(status);
}
