namespace CompatibilityChecker
{
    public interface IReportStatistics
    {
        (int error, int warning, int information, int disabled) SeverityCounts { get; }
    }
}