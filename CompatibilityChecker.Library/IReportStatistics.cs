namespace CompatibilityChecker.Library
{
    public interface IReportStatistics
    {
        (int error, int warning, int information, int disabled) SeverityCounts { get; }
    }
}