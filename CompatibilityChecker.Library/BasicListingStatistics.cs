using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompatibilityChecker.Library
{
    class BasicListingStatistics : IReportStatistics
    {
        (int error, int warning, int information, int disabled) _severityCounts;
        (int error, int warning, int information, int disabled) IReportStatistics.SeverityCounts => _severityCounts;

        public BasicListingStatistics(int SeverityError, int SeverityWarning, int SeverityInformation, int SeverityDisabled)
        {
            _severityCounts = (error: SeverityError, warning: SeverityWarning, information: SeverityInformation, disabled: SeverityDisabled);
        }
    }
}
